using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace NodeEditor
{
    public partial class Node
    {
        protected int nodeId;
        public Rect rect;
        public string title;
        public bool isDragged;
        public bool isSelected;
        

        public List<ConnectionPoint> inPoints;
        protected GUIStyle inPointStyle;
        protected Func<ConnectionPoint, ConnectionPoint> onClickInPoint;

        public List<ConnectionPoint> outPoints;
        protected GUIStyle outPointStyle;
        protected Func<ConnectionPoint, ConnectionPoint> onClickOutPoint;

        protected Action<ConnectionPoint> onRemoveConnectionPoint;

        public GUIStyle titleStyle = new GUIStyle();

        public GUIStyle style;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;

        public Node(
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        {
            style = nodeStyle;
            
            inPoints = new List<ConnectionPoint>();
            this.inPointStyle = inPointStyle;
            this.onClickInPoint = onClickInPoint;

            outPoints = new List<ConnectionPoint>();
            this.outPointStyle = outPointStyle;
            this.onClickOutPoint = onClickOutPoint;

            this.onRemoveConnectionPoint = onRemoveConnectionPoint;

            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;

            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;

        }

        public Node(
            int id,
            Vector2 position,
            float width,
            float height,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        {
            nodeId = id;
            title = "Node " + id;
            rect = new Rect(position.x, position.y, width, height);
            style = nodeStyle;

            inPoints = new List<ConnectionPoint>();
            this.inPointStyle = inPointStyle;
            this.onClickInPoint = onClickInPoint;

            outPoints = new List<ConnectionPoint>();
            this.outPointStyle = outPointStyle;
            this.onClickOutPoint = onClickOutPoint;

            this.onRemoveConnectionPoint = onRemoveConnectionPoint;

            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;

            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public void Draw()
        {
            LayoutSize = Vector2.zero;
            LayoutPosition = new Vector2(rect.x + LayoutOffset.x, rect.y + LayoutOffset.y);
            for (int i = 0; i < inPoints.Count; i++)
            {
                inPoints[i].Draw();
            }
            for (int i = 0; i < outPoints.Count; i++)
            {
                outPoints[i].Draw();
            }
            GUI.Box(rect, "", style);
            DrawNode(LayoutPosition);
            //if(resizeFlag)
            //{
            //    LayoutConnectionPoints();
            //    resizeFlag = false;
            //}
        }

        private int titleCount = 1;
        private void AddTitle()
        {
            titleCount++;
        }

        private void SubTitle()
        {
            if (titleCount > 1)
                titleCount--;
        }

        public virtual Vector2 DrawNode(Vector2 position)
        {
            BeginHorizontal();
            for (int i = 0; i < titleCount; i++)
                LayoutLabel(title, titleStyle);
            EndHorizontal();
            BeginHorizontal();
            LayoutButton("+", AddTitle);
            LayoutButton("-", SubTitle);
            EndHorizontal();
            //float height = 20f;
            //LayoutLabel(
            //    new Rect(
            //        position.x,
            //        rect.y + rect.height / 2 - 10,
            //        rect.width - LayoutOffset.x * 2,
            //        height),
            //    title,
            //    titleStyle);
            //position.y += height;
            //if (resizeFlag)
            //{
            //    LayoutConnectionPoints();
            //    resizeFlag = false;
            //}

            return position;
        }

        #region Layout component methods
        protected Vector2 LayoutPosition;
        protected Vector2 LayoutOffset = new Vector2(10, 10);
        private Vector2 LayoutSize;

        private Vector2 currentPosition;

        private Stack<List<Action<float, float>>> horizontalLayout = new Stack<List<Action<float, float>>>();

        private void BeginHorizontal()
        {   
            horizontalLayout.Push(new List<Action<float, float>>());
        }

        private void EndHorizontal()
        {
            if (horizontalLayout.Count > 0)
            {
                var list = horizontalLayout.Pop();
                float width = (rect.width - LayoutOffset.x * 2 - currentPosition.x) / list.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    float x = LayoutPosition.x + currentPosition.x + width * i;
                    list[i].Invoke(x, width);
                }
                NewLine();
            }
        }

        protected void LayoutLabel(string text)
        {
            if (horizontalLayout.Count > 0)
            {
                horizontalLayout.Peek().Add((x, width) =>
                {
                    currentPosition.y = 20f;
                    GUI.Label(new Rect(x, LayoutPosition.y, width, 20), text);
                });
            }
            else
            {
                currentPosition.y = 20f;
                GUI.Label(new Rect(LayoutPosition.x + currentPosition.x, LayoutPosition.y, rect.width - LayoutOffset.x * 2 - currentPosition.x, 20), text);
                NewLine();
            }
        }

        protected void LayoutLabel(string text, GUIStyle style)
        {
            if (horizontalLayout.Count > 0)
            {
                horizontalLayout.Peek().Add((x, width) =>
                {
                    currentPosition.y = 20f;
                    GUI.Label(new Rect(x, LayoutPosition.y, width, 20), text, style);
                });
            }
            else
            {
                currentPosition.y = 20f;
                GUI.Label(new Rect(LayoutPosition.x + currentPosition.x, LayoutPosition.y, rect.width - LayoutOffset.x * 2 - currentPosition.x, 20), text, style);
                NewLine();
            }
        }

        protected void LayoutButton(string text, Action action)
        {
            if (horizontalLayout.Count > 0)
            {
                horizontalLayout.Peek().Add((x, width) =>
                {
                    currentPosition.y = 20f;
                    if (GUI.Button(new Rect(x, LayoutPosition.y, width, 20), text))
                        action?.Invoke();
                });
            }
            else
            {
                currentPosition.y = 20f;
                if(GUI.Button(new Rect(LayoutPosition.x + currentPosition.x, LayoutPosition.y, rect.width - LayoutOffset.x * 2 - currentPosition.x, 20), text))
                    action?.Invoke();
                NewLine();
            }
        }

        protected void LayoutButton(string text, Action action, GUIStyle style)
        {
            if (horizontalLayout.Count > 0)
            {
                horizontalLayout.Peek().Add((x, width) =>
                {
                    currentPosition.y = 20f;
                    if (GUI.Button(new Rect(x, LayoutPosition.y, width, 20), text, style))
                        action?.Invoke();
                });
            }
            else
            {
                currentPosition.y = 20f;
                if (GUI.Button(new Rect(LayoutPosition.x + currentPosition.x, LayoutPosition.y, rect.width - LayoutOffset.x * 2 - currentPosition.x, 20), text, style))
                    action?.Invoke();
                NewLine();
            }
        }

        protected void NewLine()
        {
            LayoutPosition = new Vector2(rect.x + LayoutOffset.x, LayoutPosition.y + currentPosition.y);
            LayoutSize.y += currentPosition.y;
            float totalSize = LayoutSize.y + LayoutOffset.y * 2;
            if ((totalSize > rect.height && totalSize > connectionPointHeight) || (connectionPointHeight < totalSize && totalSize < rect.height))
            {
                rect.height = totalSize;
                LayoutConnectionPoints();
            }
            currentPosition = Vector2.zero;
        }

        #endregion

        public bool ProcessEvents(Event e)
        {
            ProcessConnectionPointEvents(e);
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;
                            style = selectedNodeStyle;
                            onSelectNode(this);
                        }
                        else
                        {
                            GUI.changed = true;
                            if (isSelected)
                                onCancelSelectNode(this);
                            isSelected = false;
                            style = defaultNodeStyle;

                        }
                    }

                    if (e.button == 1 && isSelected)
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    isDragged = false;
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }
            return false;
        }

        public virtual void AddOutPoint(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                ConnectionPoint point = new ConnectionPoint(
                    this, 
                    ConnectionPointType.Out, 
                    outPointStyle, 
                    onClickOutPoint,
                    OnConnectConnectionPoint,
                    OnDisconnectConnectionPoint,
                    OnRemoveConnectionPoint);
                outPoints.Add(point);
            }

            LayoutConnectionPoints();
        }

        public virtual void AddInPoint(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                ConnectionPoint point = new ConnectionPoint(
                    this, 
                    ConnectionPointType.In, 
                    inPointStyle,
                    onClickInPoint, 
                    OnConnectConnectionPoint, 
                    OnDisconnectConnectionPoint,
                    OnRemoveConnectionPoint);
                inPoints.Add(point);
            }

            LayoutConnectionPoints();
        }

        private float connectionPointHeight;
        protected void LayoutConnectionPoints()
        {
            float heightOfPoint = 30f;
            float interval = 10f;
            int maxPointNum = Mathf.Max(inPoints.Count, outPoints.Count);
            if (maxPointNum > 0)
            {
                connectionPointHeight = maxPointNum * heightOfPoint + (maxPointNum + 1) * interval;
                if(connectionPointHeight > rect.height)
                    rect.height = connectionPointHeight + LayoutOffset.y * 2;

                LayoutConnectionPointList(inPoints, rect.height);
                LayoutConnectionPointList(outPoints, rect.height);
            }
        }

        private void LayoutConnectionPointList(List<ConnectionPoint> list, float totalHeight)
        {
            float divPoint = 1f / (list.Count + 1);
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i].type == ConnectionPointType.Out)
                    list[i].offset.x = rect.width - 8f;
                else if(list[i].type == ConnectionPointType.In)
                    list[i].offset.x = -list[i].rect.width + 8f;
                list[i].offset.y = rect.height * divPoint * (i + 1) - list[i].rect.height * 0.5f;
            }
        }

        protected virtual void OnDisconnectConnectionPoint(ConnectionPoint own, ConnectionPoint other) { }

        protected virtual void OnConnectConnectionPoint(ConnectionPoint own, ConnectionPoint other) { }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            AddGenericItem(genericMenu);
            genericMenu.ShowAsContext();
        }

        private void ProcessConnectionPointEvents(Event e)
        {
            ProcessConnectionPointListEvents(inPoints, e);
            ProcessConnectionPointListEvents(outPoints, e);
        }

        private void ProcessConnectionPointListEvents(List<ConnectionPoint> list, Event e)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ProcessEvents(e);
            }
        }

        protected bool OnRemoveConnectionPoint(ConnectionPoint point)
        {
            bool result =  RemoveConnectionPoint(point);
            if (result)
            {
                onRemoveConnectionPoint?.Invoke(point);
                LayoutConnectionPoints();
            }
            return result;  
        }

        protected virtual bool RemoveConnectionPoint(ConnectionPoint point)
        {
            bool result = false;
            if (point.type == ConnectionPointType.In)
                result = inPoints.Remove(point);
            else if(point.type == ConnectionPointType.Out)
                result = outPoints.Remove(point);
            return result;
        }

        protected virtual void AddGenericItem(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            menu.AddItem(new GUIContent("Add In Point"), false, () => AddInPoint());
            menu.AddItem(new GUIContent("Add Out Point"), false, () => AddOutPoint());

            menu.AddItem(new GUIContent("Copy"), false, () => onCopyNode(this));
        }

        protected void OnClickRemoveNode()
        {
            onRemoveNode?.Invoke(this);
        }

        public int GetInstanceID()
        {
            return nodeId;
        }

        public virtual void Save(BinaryWriter writer)
        {
            writer.Write(nodeId);
            writer.Write(title);
            writer.Write(rect.x);
            writer.Write(rect.y);
            writer.Write(rect.width);
            writer.Write(rect.height);
            writer.Write(inPoints.Count);
            writer.Write(outPoints.Count);
        }

        public virtual void Load(BinaryReader reader, int header)
        {
            nodeId = reader.ReadInt32();
            if (header >= 1)
                title = reader.ReadString();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float width = reader.ReadSingle();
            float height = reader.ReadSingle();
            rect = new Rect(x, y, width, height);

            int inPointsCount = reader.ReadInt32();
            for (int i = inPoints.Count; i < inPointsCount; i++)
            {
                AddInPoint();
            }
            int outPointsCount = reader.ReadInt32();
            for (int i = outPoints.Count; i < outPointsCount; i++)
            {
                AddOutPoint();
            }
            LayoutConnectionPoints();
        }
    }

    public partial class Node
    {
        public Action<Node> onSelectNode;
        public Action<Node> onCancelSelectNode;
        public Action<Node> onCopyNode;
        public Action<Node> onRemoveNode;
    }
}