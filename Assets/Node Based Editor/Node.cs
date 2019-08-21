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
            pos = new Vector2(rect.x + LayoutOffset.x, rect.y + LayoutOffset.y);
            cursor = 0;
            addHeight = 0;
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
            if(resizeFlag)
            {
                LayoutConnectionPoints();
                resizeFlag = false;
            }
        }

        private int titleCount = 1;

        public virtual Vector2 DrawNode(Vector2 position)
        {
            float height = 20f;
            LayoutLabel(
                new Rect(
                    position.x,
                    rect.y + rect.height / 2 - 10,
                    rect.width - LayoutOffset.x * 2,
                    height),
                title,
                titleStyle);
            position.y += height;
            if (resizeFlag)
            {
                LayoutConnectionPoints();
                resizeFlag = false;
            }

            return position;
        }

        #region Layout component methods
        private bool resizeFlag = false;
        private Rect stuffRect;
        protected Vector2 LayoutPosition;
        protected Vector2 LayoutOffset = new Vector2(10, 10);

        protected void ResizeNodeAdaptively(Rect position)
        {
            if (position.x + position.width > rect.x + rect.width - LayoutOffset.x)
            {
                rect.width = position.x + position.width - rect.x + LayoutOffset.x;
                resizeFlag = true;
            }
            if (position.y + position.height > rect.y + rect.height - LayoutOffset.y)
            {
                rect.height = position.y + position.height - rect.y + LayoutOffset.y;
                resizeFlag = true;
            }
        }

        private Vector2 pos;
        private float cursor, addHeight;

        protected void Layout()
        {
            addHeight = 20f;
            GUI.Label(new Rect(pos.x + cursor, pos.y, rect.width -LayoutOffset.x * 2 - cursor, 20), "Text");
            NewLine();
        }

        protected void Layout(float x, float y, float width, float height)
        {
            x = x < 0 ? 0 : x;
            height = height < 0 ? 0 : height;
            addHeight = addHeight < y + height ? y + height : addHeight;
            GUI.Label(new Rect(pos.x + cursor + x, pos.y + y, width, height), "Text");
            cursor += x + width;
        }

        protected void NewLine()
        {
            pos = new Vector2(rect.x + LayoutOffset.x, pos.y + LayoutOffset.y + addHeight);
            cursor = 0;
            addHeight = 0;
        }

        protected void LayoutLabel(Rect position, string text)
        {
            ResizeNodeAdaptively(position);
            GUI.Label(position, text);
        }

        protected void LayoutLabel(Rect position, string text, GUIStyle style)
        {
            ResizeNodeAdaptively(position);
            GUI.Label(position, text, style);
        }

        protected bool LayoutButton(Rect position, string text)
        {
            ResizeNodeAdaptively(position);
            return GUI.Button(position, text);
        }

        protected bool LayoutButton(Rect position, string text, GUIStyle style)
        {
            ResizeNodeAdaptively(position);
            return GUI.Button(position, text, style);
        }

        protected int LayoutPopup(Rect position, int selectedIndex, string[] options)
        {
            ResizeNodeAdaptively(position);
            return EditorGUI.Popup(position, selectedIndex, options);
        }

        protected int LayoutIntField(Rect position, int value)
        {
            ResizeNodeAdaptively(position);
            return EditorGUI.IntField(position, value);
        }

        protected float LayoutFloatField(Rect position, float value)
        {
            ResizeNodeAdaptively(position);
            return EditorGUI.FloatField(position, value);
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

        protected void LayoutConnectionPoints()
        {
            float heightOfPoint = 30f;
            float interval = 10f;
            int maxPointNum = Mathf.Max(inPoints.Count, outPoints.Count);
            if (maxPointNum > 0)
            {
                float totalHeight = maxPointNum * heightOfPoint + (maxPointNum + 1) * interval;
                if(totalHeight > rect.height)
                    rect.height = totalHeight + LayoutOffset.y * 2;

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