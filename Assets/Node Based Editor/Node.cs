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
            try
            {
                DrawNode();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                ClearDirectLayout();
            }
        }
        
        public virtual void DrawNode()
        {
            LayoutLabel(title, titleStyle);
            //LayoutButton("Button", null);
            //LayoutPopup(0, new string[] { "1", "2", "3" }, null);
            //BeginHorizontal();
            //LayoutLabel("Int:");
            //LayoutIntField(5, null);
            //EndHorizontal();
            //BeginHorizontal();
            //LayoutLabel("Float:");
            //LayoutFloatField(0.5f, null);
            //EndHorizontal();
        }

        #region Layout component methods
        protected Vector2 LayoutPosition;
        protected Vector2 LayoutOffset = new Vector2(10, 10);
        private float currentHeight;

        private enum LayoutDirect { Horizontal, Vertical }
        private Stack<Tuple<LayoutDirect, List<Action<float, float>>>> directLayout = new Stack<Tuple<LayoutDirect, List<Action<float, float>>>>();
        private Stack<Tuple<LayoutDirect, float>> directSize = new Stack<Tuple<LayoutDirect, float>>();

        protected void BeginHorizontal(float height = 20f)
        {  
            directLayout.Push(new Tuple<LayoutDirect, List<Action<float, float>>>(LayoutDirect.Horizontal, new List<Action<float, float>>()));
            directSize.Push(new Tuple<LayoutDirect, float>(LayoutDirect.Horizontal, height));
        }

        protected void EndHorizontal()
        {
            if (directLayout.Count > 0 && directLayout.Peek().Item1 == LayoutDirect.Horizontal)
            {   
                var list = directLayout.Pop().Item2;
                float width = (rect.x + rect.width - LayoutOffset.x - LayoutPosition.x) / list.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    float x = LayoutPosition.x + width * i;
                    list[i].Invoke(x, width);
                }
                currentHeight = directSize.Pop().Item2;
                NewLine();
            }
        }

        private void ClearDirectLayout()
        {
            directLayout.Clear();
            directSize.Clear();
        }

        #region vertical
        //protected void BeginVertical(float width = 50f)
        //{
        //    horizontalLayout.Push(new Tuple<LayoutDirect, List<Action<float, float>>>(LayoutDirect.Vertical, new List<Action<float, float>>()));
        //    horizontalHeight.Push(new Tuple<LayoutDirect, float>(LayoutDirect.Vertical, width));
        //}

        //protected void EndVertical()
        //{
        //    if (horizontalLayout.Count > 0 && horizontalLayout.Peek().Item1 == LayoutDirect.Vertical)
        //    {
        //        var list = horizontalLayout.Pop().Item2;
        //        float height = (rect.height - LayoutOffset.y * 2) / list.Count;
        //        for(int i = 0; i < list.Count; i++)
        //        {
        //            float y = LayoutPosition.y + height * i;
        //            list[i].Invoke(y, height);
        //        }
        //        horizontalHeight.Pop();
        //        NewLine();
        //    }
        //}
        #endregion

        protected void LayoutLabel(string text, float width, float height)
        {
            GUI.Label(new Rect(LayoutPosition, new Vector2(width, height)), text);
            LayoutPosition.x += width + 3f;
            currentHeight = currentHeight < height ? height : currentHeight;
        }

        protected void LayoutLabel(string text)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    GUI.Label(GetDirectRect(arg1, arg2, size, direct), text);
                });
            }
            else
            {
                currentHeight = 20f;
                GUI.Label(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), text);
                NewLine();
            }
        }

        protected void LayoutLabel(string text, GUIStyle style)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    GUI.Label(GetDirectRect(arg1, arg2, size, direct), text, style);
                });
            }
            else
            {
                currentHeight = 20f;
                GUI.Label(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), text, style);
                NewLine();
            }
        }

        protected void LayoutButton(string text, float width, float height, Action action)
        {
            if(GUI.Button(new Rect(LayoutPosition, new Vector2(width, height)), text))
            {
                action?.Invoke();
            }
            LayoutPosition.x += width + 3f;
            currentHeight = currentHeight < height ? height : currentHeight;
        }

        protected void LayoutButton(string text, Action action)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    if (GUI.Button(GetDirectRect(arg1, arg2, size, direct), text))
                    {
                        action?.Invoke();
                    }
                });
            }
            else
            {
                currentHeight = 20f;
                if(GUI.Button(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), text))
                    action?.Invoke();
                NewLine();
            }
        }

        protected void LayoutButton(string text, Action action, GUIStyle style)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    if (GUI.Button(GetDirectRect(arg1, arg2, size, direct), text, style))
                    {
                        action?.Invoke();
                    }
                });
            }
            else
            {
                currentHeight = 20f;
                if (GUI.Button(new Rect(LayoutPosition.x, LayoutPosition.y, rect.width - LayoutOffset.x * 2, 20), text, style))
                    action?.Invoke();
                NewLine();
            }
        }

        protected void LayoutPopup(int selectedIndex, string[] options, float width, float height, Action<int> onSelect)
        {   
            int index = EditorGUI.Popup(new Rect(LayoutPosition.x, LayoutPosition.y, width, height), selectedIndex, options);
            onSelect?.Invoke(index);
            LayoutPosition.x += width + 3f;
            currentHeight = currentHeight < height ? height : currentHeight;
        }

        protected void LayoutPopup(int selectedIndex, string[] options, Action<int> onSelect)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    int index = EditorGUI.Popup(GetDirectRect(arg1, arg2, size, direct), selectedIndex, options);
                    onSelect?.Invoke(index);
                });
            }
            else
            {
                currentHeight = 20f;
                int index = EditorGUI.Popup(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), selectedIndex, options);
                onSelect?.Invoke(index);
                NewLine();
            }
        }

        protected void LayoutPopup(int selectedIndex, string[] options, GUIStyle style, Action<int> onSelect)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    int index = EditorGUI.Popup(GetDirectRect(arg1, arg2, size, direct), selectedIndex, options, style);
                    onSelect?.Invoke(index);
                });
            }
            else
            {
                currentHeight = 20f;
                int index = EditorGUI.Popup(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), selectedIndex, options, style);
                onSelect?.Invoke(index);
                NewLine();
            }
        }

        protected void LayoutIntField(int value, float width, float height, Action<int> onChanged)
        {
            int v = EditorGUI.IntField(new Rect(LayoutPosition, new Vector2(width, height)), value);
            onChanged?.Invoke(v);
            LayoutPosition.x += width + 3f;
            currentHeight = currentHeight < height ? height : currentHeight;
        }

        protected void LayoutIntField(int value, Action<int> onChanged)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    int v = EditorGUI.IntField(GetDirectRect(arg1, arg2, size, direct), value);
                    onChanged?.Invoke(v);
                });
            }
            else
            {
                currentHeight = 20f;
                int v = EditorGUI.IntField(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), value);
                onChanged?.Invoke(v);
                NewLine();
            }
        }

        protected void LayoutIntField(int value, GUIStyle style, Action<int> onChanged)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    int v = EditorGUI.IntField(GetDirectRect(arg1, arg2, size, direct), value, style);
                    onChanged?.Invoke(v);
                });
            }
            else
            {
                currentHeight = 20f;
                int v = EditorGUI.IntField(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), value, style);
                onChanged?.Invoke(v);
                NewLine();
            }
        }

        protected void LayoutFloatField(float value, float width, float height, Action<float> onChanged)
        {
            float v = EditorGUI.FloatField(new Rect(LayoutPosition, new Vector2(width, height)), value);
            onChanged?.Invoke(v);
            LayoutPosition.x += width + 3f;
            currentHeight = currentHeight < height ? height : currentHeight;
        }

        protected void LayoutFloatField(float value, Action<float> onChanged)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    float v = EditorGUI.FloatField(GetDirectRect(arg1, arg2, size, direct), value);
                    onChanged?.Invoke(v);
                });
            }
            else
            {
                currentHeight = 20f;
                float v = EditorGUI.FloatField(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), value);
                onChanged?.Invoke(v);
                NewLine();
            }
        }

        protected void LayoutFloatField(float value, GUIStyle style, Action<float> onChanged)
        {
            if (directLayout.Count > 0)
            {
                float size = directSize.Peek().Item2;
                LayoutDirect direct = directLayout.Peek().Item1;
                directLayout.Peek().Item2.Add((arg1, arg2) =>
                {
                    float v = EditorGUI.FloatField(GetDirectRect(arg1, arg2, size, direct), value, style);
                    onChanged?.Invoke(v);
                });
            }
            else
            {
                currentHeight = 20f;
                float v = EditorGUI.FloatField(new Rect(LayoutPosition.x, LayoutPosition.y, rect.x + rect.width - LayoutOffset.x - LayoutPosition.x, 20), value, style);
                onChanged?.Invoke(v);
                NewLine();
            }
        }

        private Rect GetDirectRect(float arg1, float arg2, float size, LayoutDirect direct)
        {
            if(direct == LayoutDirect.Horizontal)
                return new Rect(arg1, LayoutPosition.y, arg2, size);
            else
                return new Rect(LayoutPosition.x, arg1, size, arg2);
        }

        protected void NewLine()
        {
            LayoutPosition.x = rect.x + LayoutOffset.x;
            LayoutPosition.y = LayoutPosition.y + currentHeight;
            float totalSize = LayoutPosition.y - rect.y + LayoutOffset.y * 2;
            if ((totalSize > rect.height && totalSize > connectionPointHeight) || (connectionPointHeight < totalSize && totalSize < rect.height))
            {
                rect.height = totalSize;
                LayoutConnectionPoints();
            }
            
            currentHeight = 0;
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

        public virtual void OnCreateConnectionPoint(ConnectionPoint point)
        {

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
                OnCreateConnectionPoint(point);
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
                OnCreateConnectionPoint(point);
                inPoints.Add(point);
            }

            LayoutConnectionPoints();
        }

        private float connectionPointHeight;
        protected void LayoutConnectionPoints()
        {
            float heightOfPoint = 25f;
            float interval = 5f;
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