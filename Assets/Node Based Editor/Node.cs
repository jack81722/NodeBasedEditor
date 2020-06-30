using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using NodeEditor.FileIO;
using System.Reflection;
using NodeEditor.Style;
using System.Linq;
using NodeEditor.Component;

namespace NodeEditor
{
    public class Node : IBinarySaveable, IBinaryLoadable
    {
        #region -- Default values --
        protected readonly Rect DefaultRect = new Rect(0, 0, 200, 50);
        protected const float UpperBound = 10f;
        protected const float LowerBound = 10f;
        #endregion

        #region -- Node state fields --
        protected int nodeId;
        public Rect rect;
        public string title;
        protected bool ShowTitle = true;
        public Rect scaleRect { get => new Rect(rect.x, rect.y, rect.width * zoom, rect.height * zoom); }
        protected float zoom = 1;
        public ENodeType NodeType { get; protected set; } = ENodeType.Constant;
        public bool isDragged;
        public bool isSelected;
        #endregion

        #region -- Input/Output fields --
        public List<ConnectionPoint> inPoints;
        protected GUIStyle inPointStyle;
        private event Action<ConnectionPoint> onClickInPoint;
        public event Action<ConnectionPoint> OnClickInPoint
        {
            add
            {
                onClickInPoint += value;
                foreach (var p in inPoints)
                {
                    p.OnConnectPoint += value;
                }
            }
            remove
            {
                onClickInPoint -= value;
                foreach (var p in inPoints)
                {
                    p.OnConnectPoint -= value;
                }
            }
        }
        public EConnectPointAlignment InPointAlignment = EConnectPointAlignment.Average;

        public List<ConnectionPoint> outPoints;
        protected GUIStyle outPointStyle;
        private event Action<ConnectionPoint> onClickOutPoint;
        public event Action<ConnectionPoint> OnClickOutPoint
        {
            add
            {
                onClickOutPoint += value;
                foreach (var p in outPoints)
                {
                    p.OnConnectPoint += value;
                }
            }
            remove
            {
                onClickOutPoint -= value;
                foreach (var p in outPoints)
                {
                    p.OnConnectPoint -= value;
                }
            }
        }
        public EConnectPointAlignment OutPointAlignment = EConnectPointAlignment.Average;
        #endregion

        #region -- Events --
        public Action<Node> OnCancelSelectNode, OnCopyNode;
        public Action<Node, bool> OnSelectNode;
        public Action<Node, Vector2> OnDrag;
        public Action<ConnectionPoint> OnRemoveConnectionPoint;
        public Action<Node> OnRemoveNode;
        #endregion

        #region -- Styles --
        public GUIStyle style;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;
        #endregion

        public bool EnableDrag { get; protected set; } = true;
        public bool EnableCopy { get; protected set; } = true;

        #region -- Constructors --
        public Node(
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle)
        {
            rect = DefaultRect;

            style = nodeStyle;
            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;

            inPoints = new List<ConnectionPoint>();
            this.inPointStyle = inPointStyle;

            outPoints = new List<ConnectionPoint>();
            this.outPointStyle = outPointStyle;

        }

        public Node(
            int id,
            Vector2 position,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle)
        {
            nodeId = id;
            rect = new Rect(position.x, position.y, DefaultRect.width, DefaultRect.height);

            style = nodeStyle;
            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;

            inPoints = new List<ConnectionPoint>();
            this.inPointStyle = inPointStyle;

            outPoints = new List<ConnectionPoint>();
            this.outPointStyle = outPointStyle;
        }
        #endregion

        public void Move(Vector2 delta)
        {
            rect.position += delta;
            //OnDrag?.Invoke(this, delta);
        }

        protected void Drag(Vector2 delta)
        {
            OnDrag?.Invoke(this, delta);
        }

        public void Draw(float zoom = 1)
        {
            this.zoom = zoom;
            for (int i = 0; i < inPoints.Count; i++)
            {
                inPoints[i].Draw(zoom);
            }
            for (int i = 0; i < outPoints.Count; i++)
            {
                outPoints[i].Draw(zoom);
            }
            GUI.Box(scaleRect, ShowTitle ? title : string.Empty, style);
            BeginLayout();
            OnDraw(zoom);
            ApplyNodeLayout();
        }

        protected virtual void OnDraw(float zoom)
        {
            GUI.Label(new Rect(rect.x, rect.y + scaleRect.height / 2 - 10 * zoom, scaleRect.width, 20 * zoom), string.IsNullOrEmpty(title) ? nodeId.ToString() : title, CommonStyle.BoldTitle);
        }

        public void Select(bool select)
        {
            if (select)
                style = selectedNodeStyle;
            else
                style = defaultNodeStyle;
        }

        public virtual bool ProcessEvents(Event e)
        {
            ProcessConnectionPointEvents(e);
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            if(EnableDrag)
                                isDragged = true;
                            //GUI.changed = true;
                            OnSelectNode?.Invoke(this, e.control);
                        }
                        //else if(!e.control)
                        //{
                        //    //GUI.changed = true;
                        //    OnCancelSelectNode(this);
                        //}
                    }

                    if (e.button == 1 && rect.Contains(e.mousePosition))
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

        #region -- Connect point methods --
        public ConnectionPoint AddOutPoint(string title, Func<object> getValue, int maxConnect = -1)
        {
            ConnectionPoint point = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, title, maxConnect);
            point.OnConnectPoint += onClickOutPoint;
            point.OnRemoveConnectionPoint += RemoveConnectionPoint;
            point.GetFunc = getValue;
            point.offset.x = /*scaleRect.width*/ -8f;
            outPoints.Add(point);

            ModifyConnectionPointHeight();
            return point;
        }

        public ConnectionPoint AddOutPoint<T>(string title, Func<object> getValue, int maxConnect = -1) where T : ConnectionPoint
        {
            ConnectionPoint point = (ConnectionPoint)Activator.CreateInstance(typeof(T), this, ConnectionPointType.Out, outPointStyle, title, maxConnect);
            point.OnConnectPoint += onClickOutPoint;
            point.OnRemoveConnectionPoint += RemoveConnectionPoint;
            point.GetFunc = getValue;
            point.offset.x = /*scaleRect.width*/ -8f;
            outPoints.Add(point);

            ModifyConnectionPointHeight();
            return point;
        }

        public ConnectionPoint AddInPoint(string title = "", int maxConnect = 1)
        {
            ConnectionPoint point = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, title, maxConnect);
            point.OnConnectPoint += onClickInPoint;
            point.OnRemoveConnectionPoint += RemoveConnectionPoint;
            point.offset.x = /*-point.rect.width*/ +8f;
            inPoints.Add(point);

            ModifyConnectionPointHeight();
            return point;
        }

        public ConnectionPoint AddInPoint<T>(string title = "", int maxConnect = 1) where T : ConnectionPoint
        {
            ConnectionPoint point = (ConnectionPoint)Activator.CreateInstance(typeof(T), this, ConnectionPointType.In, inPointStyle, title, maxConnect);
            point.OnConnectPoint += onClickInPoint;
            point.OnRemoveConnectionPoint += RemoveConnectionPoint;
            point.offset.x = /*-point.rect.width*/ +8f;
            inPoints.Add(point);

            ModifyConnectionPointHeight();
            return point;
        }

        protected void ClearInPoints()
        {
            foreach (var point in inPoints)
            {
                point.Clear();
                OnRemoveConnectionPoint?.Invoke(point);
            }
            inPoints.Clear();
            ModifyConnectionPointHeight();
        }

        protected void ClearOutPoints()
        {
            foreach (var point in outPoints)
            {
                point.Clear();
                OnRemoveConnectionPoint?.Invoke(point);
            }
            outPoints.Clear();
            ModifyConnectionPointHeight();
        }

        protected void RemoveConnectionPoint(ConnectionPoint point)
        {
            if (point.type == ConnectionPointType.In)
                inPoints.Remove(point);
            else
                outPoints.Remove(point);
            OnRemoveConnectionPoint?.Invoke(point);
            ModifyConnectionPointHeight();
        }
        #endregion

        protected virtual void AddGenericItem(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            menu.AddItem(new GUIContent("Add In Point"), false, () => AddInPoint());
            menu.AddItem(new GUIContent("Add Out Point"), false, () => AddOutPoint(string.Empty, null));

            menu.AddItem(new GUIContent("Copy"), false, () => OnCopyNode(this));
        }

        protected void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }

        public int GetInstanceID()
        {
            return nodeId;
        }

        public virtual void OnDrawDetail()
        {
            EditorGUILayout.LabelField(title, CommonStyle.DetailTitle);
        }

        public static bool LoopCheck(Node a, Node b)
        {
            // a --> b
            var root = a;
            Stack<Node> stack = new Stack<Node>();
            stack.Push(b);
            List<Node> list = new List<Node>();

            Node current;
            while (stack.Count > 0)
            {
                current = stack.Pop();
                if (current == root)
                    return false;
                list.Add(current);

                foreach (var next in current.GetOutNodes())
                {
                    stack.Push(next);
                }
            }
            return true;
        }

        public IEnumerable<Node> GetOutNodes()
        {
            List<Node> outs = new List<Node>();
            foreach (var point in outPoints)
            {
                if (point.HasConnected)
                {
                    outs.AddRange(point.Connections.Select(conn => conn.inPoint.node));
                }
            }
            return outs;
        }

        #region -- Binary Save/Load methods --
        public virtual void BinarySave(BinaryWriter writer)
        {
            writer.Write(nodeId);
            writer.Write(rect.x);
            writer.Write(rect.y);
            writer.Write(rect.width);
            writer.Write(rect.height);
            //writer.Write(inPoints.Count);
            //writer.Write(outPoints.Count);
        }

        public virtual void BinaryLoad(BinaryReader reader)
        {
            nodeId = reader.ReadInt32();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float width = reader.ReadSingle();
            float height = reader.ReadSingle();
            rect = new Rect(x, y, width, height);

            //int inPointsCount = reader.ReadInt32();
            //for (int i = 0; i < inPointsCount; i++)
            //{
            //    AddInPoint();
            //}
            //int outPointsCount = reader.ReadInt32();
            //for (int i = 0; i < outPointsCount; i++)
            //{
            //    AddOutPoint();
            //}
        }
        #endregion

        #region -- Layout methods -- 
        private void ApplyNodeLayout()
        {
            var layoutChanged = LastFieldHeight != FieldHeight;
            if (layoutChanged || ConnectPointChanged)
            {
                LastFieldHeight = FieldHeight;
                var fieldHeight = DefaultRect.height * zoom - UpperBound * zoom - LowerBound * zoom + FieldHeight * zoom;
                rect.height = Mathf.Max(fieldHeight, ConnectHeight);

                LayoutConnectionPointList(inPoints, InPointAlignment);
                LayoutConnectionPointList(outPoints, OutPointAlignment);
            }
        }

        #region -- Connect point layout --
        protected bool ConnectPointChanged = false;
        protected float ConnectHeight;
        protected const float PointHeight = 20;
        protected const float PointInterval = 5f;

        protected void ModifyConnectionPointHeight()
        {
            int maxPointNum = Mathf.Max(inPoints.Count, outPoints.Count);
            if (maxPointNum > 0)
            {
                float totalHeight = (maxPointNum * PointHeight + (maxPointNum/* - 1*/) * PointInterval + UpperBound + LowerBound) * zoom;
                ConnectHeight = totalHeight;
            }
            else
            {
                ConnectHeight = DefaultRect.height * zoom;
            }
            ConnectPointChanged = true;
        }

        private void LayoutConnectionPointList(List<ConnectionPoint> list, EConnectPointAlignment alignment)
        {
            switch (alignment)
            {
                case EConnectPointAlignment.Upper:
                    LayoutUpperConnectionPointList(list);
                    break;
                case EConnectPointAlignment.Lower:
                    LayoutLowerConnectionPointList(list);
                    break;
                case EConnectPointAlignment.Middle:
                    LayoutMiddleConnectionPointList(list);
                    break;
                case EConnectPointAlignment.Average:
                    LayoutAverageConnectionPointList(list, rect.height);
                    break;
            }
        }

        private void LayoutUpperConnectionPointList(List<ConnectionPoint> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].offset.y = (i * PointHeight + (i + 1) * PointInterval + UpperBound) * zoom;
            }
        }

        private void LayoutMiddleConnectionPointList(List<ConnectionPoint> list)
        {
            float middle = scaleRect.height / 2;
            float start = middle - list.Count * PointHeight / 2 * zoom - (list.Count + 1 * PointInterval) / 2 * zoom;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].offset.y = start + (i * PointHeight + (i + 1) * PointInterval) * zoom;
            }
        }

        private void LayoutLowerConnectionPointList(List<ConnectionPoint> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[list.Count - 1 - i].offset.y = scaleRect.height - LowerBound * zoom - (i + 1) * PointHeight * zoom - (i - 1) * PointInterval * zoom;
            }
        }

        private void LayoutAverageConnectionPointList(List<ConnectionPoint> list, float totalHeight)
        {
            float divPoint = 1f / (list.Count + 1);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].offset.y = totalHeight * divPoint * (i + 1) - list[i].rect.height * 0.5f * zoom;
            }
        }
        #endregion


        #region -- Field layout --
        protected const float RowHeight = 20;
        protected const float RowXOffset = 10;
        protected const float RowYOffset = 10;

        protected int FieldCount;
        protected float FieldHeight;
        private float LastFieldHeight;

        protected void BeginLayout()
        {
            FieldCount = 0;
            FieldHeight = 0;
        }

        protected Rect LayoutRect()
        {
            return new Rect(rect.x + RowXOffset * zoom, rect.y + RowYOffset * zoom + FieldCount * RowHeight * zoom, scaleRect.width - 20 * zoom, RowHeight * zoom);
        }

        protected Rect LayoutRect(ENodeFieldAlignment alignment, int index, int count)
        {   
            float width = scaleRect.width - 20;
            float height = RowHeight;
            float leftX = rect.x + RowXOffset;
            float centerX = rect.x + rect.width * 0.5f - width * 0.5f;
            float rightX = rect.x + rect.width - RowXOffset - width;
            switch (alignment)
            {
                case ENodeFieldAlignment.UpperLeft:
                    return new Rect(leftX, rect.y + RowYOffset + index * RowHeight, width, height);
                case ENodeFieldAlignment.UpperCenter:
                    return new Rect(centerX, rect.y + RowYOffset + index * RowHeight, width, height);
                case ENodeFieldAlignment.UpperRight:
                    return new Rect(rightX, rect.y + RowYOffset + index * RowHeight, width, height);
                case ENodeFieldAlignment.LowerLeft:
                    return new Rect(leftX, rect.y + rect.height - RowYOffset - RowHeight * count + index * RowHeight, width, height);
                case ENodeFieldAlignment.LowerCenter:
                    return new Rect(centerX, rect.y + rect.height - RowYOffset - RowHeight * count + index * RowHeight, width, height);
                case ENodeFieldAlignment.LowerRight:
                    return new Rect(rightX, rect.y + rect.height - RowYOffset - RowHeight * count + index * RowHeight, width, height);
                case ENodeFieldAlignment.MiddleLeft:
                    return new Rect(leftX, rect.y + rect.height * 0.5f - count * RowHeight * 0.5f + index * RowHeight, width, height);
                case ENodeFieldAlignment.MiddleCenter:
                    return new Rect(leftX, rect.y + rect.height * 0.5f - count * RowHeight * 0.5f + index * RowHeight, width, height);
                case ENodeFieldAlignment.MiddleRight:
                    return new Rect(leftX, rect.y + rect.height * 0.5f - count * RowHeight * 0.5f + index * RowHeight, width, height);
                default:
                    return new Rect(leftX, rect.y + RowYOffset + index * RowHeight, width, height);
            }
        }

        protected void LayoutLabel(string label)
        {
            FieldHeight += RowHeight * zoom;
            GUI.Label(LayoutRect(), label);
            FieldCount++;
        }

        protected void LayoutLabel(string label, GUIStyle style)
        {
            FieldHeight += RowHeight * zoom;
            GUI.Label(LayoutRect(), label, style);
            FieldCount++;
        }

        protected string LayoutTextField(string text)
        {
            FieldHeight += RowHeight * zoom;
            text = GUI.TextField(LayoutRect(), text);
            FieldCount++;
            return text;
        }

        protected string LayoutTextField(string text, GUIStyle style)
        {
            FieldHeight += RowHeight * zoom;
            text = EditorGUI.TextField(LayoutRect(), text, style);
            FieldCount++;
            return text;
        }

        protected int LayoutIntField(int value)
        {
            FieldHeight += RowHeight * zoom;
            value = EditorGUI.IntField(LayoutRect(), value);
            FieldCount++;
            return value;
        }

        protected int LayoutIntField(int value, GUIStyle style)
        {
            FieldHeight += RowHeight * zoom;
            value = EditorGUI.IntField(LayoutRect(), value, style);
            FieldCount++;
            return value;
        }

        protected float LayoutFloatField(float value)
        {
            FieldHeight += RowHeight * zoom;
            value = EditorGUI.FloatField(LayoutRect(), value);
            FieldCount++;
            return value;
        }

        protected float LayoutFloatField(float value, GUIStyle style)
        {
            FieldHeight += RowHeight * zoom;
            value = EditorGUI.FloatField(LayoutRect(), value, style);
            FieldCount++;
            return value;
        }

        protected float LayoutSlider(float value, float min, float max)
        {
            FieldHeight += RowHeight * zoom;
            value = GUI.HorizontalSlider(LayoutRect(), value, min, max);
            FieldCount++;
            return value;
        }

        protected float LayoutSlider(float value, float min, float max, GUIStyle sliderStyle, GUIStyle thumbStyle)
        {
            FieldHeight += RowHeight * zoom;
            value = GUI.HorizontalSlider(LayoutRect(), value, min, max, sliderStyle, thumbStyle);
            FieldCount++;
            return value;
        }

        protected Vector3 LayoutVector3Field(Vector3 value)
        {
            FieldHeight += RowHeight * zoom;
            value = EditorGUI.Vector3Field(LayoutRect(), "Vector3", value);
            FieldCount++;
            return value;
        }

        protected Enum LayoutEnum(Enum e)
        {
            FieldHeight += RowHeight * zoom;
            e = EditorGUI.EnumPopup(LayoutRect(), e);
            FieldCount++;
            return e;
        }

        #endregion

        #endregion
    }

    public enum EConnectPointAlignment
    {
        Upper,
        Middle,
        Lower,
        Average
    }

    public enum ENodeFieldAlignment
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }
}