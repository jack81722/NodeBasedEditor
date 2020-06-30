using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using NodeEditor.FileIO;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;
using NodeEditor.Generic;
using NodeEditor.Style;
using System.Text;
using UnityEngine.UI;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow, IBinarySaveable, IBinaryLoadable
    {
        protected IdentityPool nodeIdPool;
        protected List<Node> nodes;
        protected List<Connection> connections;

        protected float Zoom = 1;
        protected float ZoomRate = 0.05f;
        protected float MaxZoom = 2;
        protected float MinZoom = 1;

        #region -- Styles --
        protected GUIStyle nodeStyle;
        protected GUIStyle selectedNodeStyle;
        protected GUIStyle inPointStyle;
        protected GUIStyle outPointStyle;
        #endregion

        #region -- Constant --
        protected const float ToolBarHeight = 16;
        protected const float LeftColWidth = 250;
        #endregion

        #region -- SelectBox -- 
        private bool selectBox;
        private Vector2 selectBoxPos;
        private Vector2 selectBoxSize;
        private Rect SelectBoxRect
        {
            get
            {   
                var pos = selectBoxPos;
                var size = selectBoxSize;
                if (size.x < 0)
                {
                    size.x = Mathf.Abs(size.x);
                    pos.x -= size.x;
                }
                if (size.y < 0)
                {
                    size.y = Mathf.Abs(size.y);
                    pos.y -= size.y;
                }
                return new Rect(pos, size);
            }
        }
        
        #endregion

        #region -- Subwindow --
        protected Rect SubwindowRect;
        #endregion

        //protected Node SelectedNode;
        protected HashSet<Node> SelectedNodes;
        private Node copiedNode;
        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;

        protected bool clickOnNode;
        protected Vector2 offset;
        protected Vector2 drag;

        protected bool DragNodes;

        protected string SavePath = string.Empty;

        #region -- Menu item --
        [MenuItem("Window/Node Based Editor")]
        private static void OpenWindow()
        {
            NodeBasedEditor window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent("Node Based Editor");
        }
        #endregion

        #region -- Enable methods (Initialize) --
        private void OnEnable()
        {
            nodeIdPool = new IdentityPool();
            nodes = new List<Node>();
            connections = new List<Connection>();
            SelectedNodes = new HashSet<Node>();

            // initialize node style
            nodeStyle = CommonStyle.BlueNode;

            // initialize selected node style
            selectedNodeStyle = CommonStyle.SelectedBlueNode;

            // initialize input point style
            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            // initialize output point style
            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);

            // zoom
            Zoom = 1;
            ZoomRate = 0.01f;
            MaxZoom = 1f;
            MinZoom = 1f;

            // initialize subwindow 
            SubwindowRect = new Rect(0, ToolBarHeight, LeftColWidth, position.height - ToolBarHeight);

            OnInitialized();
        }
        #endregion

        #region -- GUI methods --
        private void OnGUI()
        {
            try
            {
                DrawGrid(20, 0.2f, Zoom, Color.gray);
                DrawGrid(100, 0.4f, Zoom, Color.gray);

                DrawNodes();
                DrawConnections();

                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                DrawToolbar();
                GUILayout.EndHorizontal();

                ProcessSubwindow();

                ProcessNodeEvents(Event.current);
                ProcessEvents(Event.current);

                DrawSelectBox();
                DrawConnectionLine(Event.current);

                if (GUI.changed)
                    Repaint();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Clear();
                OnInitialized();
            }
            finally
            {
                clickOnNode = false;
            }
        }

        public void ProcessSubwindow()
        {
            SubwindowRect.height = position.height - ToolBarHeight;
            GUI.Box(SubwindowRect, "");
            ProcessDetail();
        }

        

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public void ProcessDetail()
        {
            if(SelectedNodes.Count <= 0)
            {
                // do nothing ...
            }
            else if(SelectedNodes.Count == 1)
            {
                // show specific detail of node
                SelectedNodes.First().OnDrawDetail();
            }
            else
            {
                // show multi selected nodes list ...
                EditorGUILayout.LabelField("Selected:");
                foreach(var node in SelectedNodes)
                {
                    EditorGUILayout.LabelField(node.title);
                }
            }
        }

        protected virtual void DrawSelectBox()
        {
            if (selectBox)
            {   
                GUI.Box(SelectBoxRect, string.Empty, CommonStyle.SelectBoxStyle);
            }
        }

        protected virtual void DrawToolbar()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Position:" + offset.ToString(), EditorStyles.label);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                OpenSaveDialog();
            }
            if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                OpenLoadDialog();
            }
            if (GUILayout.Button("Center", EditorStyles.toolbarButton))
            {
                BackToCenter();
            }
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                Clear();
                OnInitialized();
            }
            if(EditorGUILayout.DropdownButton(new GUIContent("Menu"), FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                GenericMenu genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("New"), false, NewFile);
                genericMenu.AddSeparator("");
                genericMenu.AddItem(new GUIContent("Save"), false, QuickSave);
                genericMenu.AddItem(new GUIContent("Save as ..."), false, OpenSaveDialog);
                genericMenu.AddItem(new GUIContent("Load"), false, OpenLoadDialog);
                genericMenu.AddSeparator("");
                genericMenu.AddItem(new GUIContent("About"), false, About);
                genericMenu.DropDown(new Rect(Screen.width - 48, -83, 50, 100));
            }
        }

        protected void DrawGrid(float gridSpacing, float gridOpacity, float zoom, Color gridColor)
        {
            gridSpacing *= zoom;
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing) + 1;
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                if (gridSpacing * i + newOffset.x > LeftColWidth)
                    Handles.DrawLine(new Vector3(gridSpacing * i + newOffset.x, ToolBarHeight, 0), new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                if (gridSpacing * j + newOffset.y > ToolBarHeight)
                    Handles.DrawLine(new Vector3(/*-gridSpacing + */LeftColWidth, gridSpacing * j + newOffset.y, 0) /*+ newOffset*/, new Vector3(position.width, gridSpacing * j + newOffset.y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        protected void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null && selectedOutPoint == null)
            {
                Handles.DrawBezier(
                    selectedInPoint.rect.center,
                    e.mousePosition,
                    selectedInPoint.rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50,
                    Color.white,
                    null,
                    5f);

                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null)
            {
                Handles.DrawBezier(
                    selectedOutPoint.rect.center,
                    e.mousePosition,
                    selectedOutPoint.rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    5f);

                GUI.changed = true;
            }


            if (e.keyCode == KeyCode.Escape)
            {
                selectedOutPoint = null;
                selectedInPoint = null;
            }
            if (e.isMouse && e.button == 0)
            {
                if (selectedOutPoint != null && selectedInPoint == null && !selectedOutPoint.rect.Contains(e.mousePosition) ||
                    selectedInPoint != null && selectedOutPoint == null && !selectedInPoint.rect.Contains(e.mousePosition))
                {
                    selectedOutPoint = null;
                    selectedInPoint = null;
                }
            }
        }

        protected void DrawNodes()
        {
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Draw(Zoom);
                }
            }
        }

        protected void DrawConnections()
        {
            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    connections[i].Draw();
                }
            }
        }
        #endregion

        /// <summary>
        /// Drag editor
        /// </summary>
        /// <param name="delta"></param>
        private void OnDrag(Vector2 delta)
        {
            drag = delta;
            foreach (var node in nodes)
            {
                node.Move(delta);
            }
            GUI.changed = true;
        }

        protected void MoveTo(Vector2 position)
        {
            var delta = position - offset;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Move(delta);
            }
            offset += delta;
        }

        protected void BackToCenter()
        {
            MoveTo(Vector2.zero);
        }

        protected IEnumerable<T> GetNodes<T>() where T : Node
        {
            return nodes.Where(node => node.GetType() == typeof(T)).Select(node => (T)node);
        }

        

        private void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1 && selectedInPoint == null && selectedOutPoint == null)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    if(e.button == 0 && !SubwindowRect.Contains(e.mousePosition))
                    {
                        if (!clickOnNode)
                        {
                            if(!e.control)
                                ClearSelectedNodes();
                            selectBoxPos = e.mousePosition;
                            selectBoxSize = Vector2.zero;
                            if (!DragNodes)
                                selectBox = true;
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (!SubwindowRect.Contains(e.mousePosition))
                    {
                        if (e.button == 0)
                        {
                            if (selectBox)
                            {
                                selectBoxSize += e.delta;
                                e.Use();
                            }
                        }

                        if (e.button == 2)
                        {
                            OnDrag(e.delta);
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if(e.button == 0)
                    {
                        selectBox = false;
                        DragNodes = false;
                        SelectBox(e.control);
                        e.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete)
                    {
                        foreach(var node in SelectedNodes)
                        {
                            OnClickRemoveNode(node);
                        }
                        SelectedNodes.Clear();
                        e.Use();
                    }
                    
                    break;

                case EventType.ValidateCommand:
                    if (e.commandName == "Copy" || e.commandName == "Paste")
                    {
                        e.Use();
                    }
                    break;

                case EventType.ExecuteCommand:
                    //if (e.commandName == "Copy")
                    //{
                    //    OnClickCopyNode(SelectedNode);
                    //}
                    //else if (e.commandName == "Paste")
                    //{
                    //    PasteNode(new Vector2(50, 50));
                    //}
                    break;

                case EventType.ScrollWheel:
                    Zoom = Zoom - e.delta.y * ZoomRate;
                    Zoom = Mathf.Clamp(Zoom, MinZoom, MaxZoom);
                    e.Use();
                    break;
            }
        }

        private void SelectBox(bool ctrl)
        {
            var selecteds = nodes.FindAll(node => SelectBoxRect.Overlaps(node.rect));
            if (ctrl)
            {
                foreach (var node in selecteds)
                    node.Select(false);
                SelectedNodes.ExceptWith(selecteds);
            }
            else
            { 
                SelectedNodes.UnionWith(selecteds);
                foreach (var selected in selecteds)
                    selected.Select(true);
            }
            selectBoxSize = Vector2.zero;
        }

        private void ProcessNodeEvents(Event e)
        {
            if (nodes != null)
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    bool guiChanged = nodes[i].ProcessEvents(e);
                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            AddContextMenu(genericMenu, mousePosition);
            genericMenu.ShowAsContext();
        }

        protected virtual void AddContextMenu(GenericMenu menu, Vector2 position)
        {
            menu.AddItem(new GUIContent("Generic/Int"), false, () => CreateNode<IntNode>(position));
            menu.AddItem(new GUIContent("Generic/Float"), false, () => CreateNode<FloatNode>(position));
            menu.AddItem(new GUIContent("Generic/Vector2"), false, () => CreateNode<Vector2Node>(position));
            menu.AddItem(new GUIContent("Generic/Vector3"), false, () => CreateNode<Vector3Node>(position));
            menu.AddItem(new GUIContent("Generic/Vector4"), false, () => CreateNode<Vector4Node>(position));
            menu.AddItem(new GUIContent("Operator/Plus"), false, () => CreateNode<PlusNode>(position));
            menu.AddItem(new GUIContent("Operator/Minus"), false, () => CreateNode<MinusNode>(position));
            menu.AddItem(new GUIContent("Operator/Multi"), false, () => CreateNode<MultiNode>(position));
            menu.AddItem(new GUIContent("Operator/Divide"), false, () => CreateNode<DivideNode>(position));
            menu.AddItem(new GUIContent("Operator/Pow"), false, () => CreateNode<PowerNode>(position));
            menu.AddItem(new GUIContent("Operator/Sqrt"), false, () => CreateNode<SqrtNode>(position));
            menu.AddItem(new GUIContent("Operator/Sin"), false, () => CreateNode<SinNode>(position));
            menu.AddItem(new GUIContent("Operator/Cos"), false, () => CreateNode<CosNode>(position));
            menu.AddItem(new GUIContent("Operator/Tan"), false, () => CreateNode<TanNode>(position));
            menu.AddItem(new GUIContent("Operator/Clamp"), false, () => CreateNode<ClampNode>(position));
            menu.AddItem(new GUIContent("Operator/Distance"), false, () => CreateNode<DistanceNode>(position));
            menu.AddItem(new GUIContent("Operator/Normalize"), false, () => CreateNode<NormalizeNode>(position));
            menu.AddItem(new GUIContent("Constant/Pi"), false, () => CreateNode<PiNode>(position));
            menu.AddItem(new GUIContent("System/DeltaTime"), false, () => CreateNode<DeltaTimeNode>(position));
            menu.AddItem(new GUIContent("System/Gravity"), false, () => CreateNode<GravityNode>(position));

            menu.AddItem(new GUIContent("Event/Parallel"), false, () => CreateNode<ParallelNode>(position));
            menu.AddItem(new GUIContent("Event/Sequence"), false, () => CreateNode<SequenceNode>(position));
            menu.AddItem(new GUIContent("Event/Select"), false, () => CreateNode<SelectNode>(position));
            menu.AddItem(new GUIContent("Event/Invert"), false, () => CreateNode<InvertNode>(position));
            menu.AddItem(new GUIContent("Event/Repeat"), false, () => CreateNode<RepeatNode>(position));

            //menu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(position));
            menu.AddItem(new GUIContent("Paste"), false, () => PasteNode(position));
        }

        public void ClearSelectedNodes()
        {
            foreach (var node in SelectedNodes)
                node.Select(false);
            SelectedNodes.Clear();
        }

        #region -- Create node methods --
        protected Node CreateNode(string nodeType, Vector2 position)
        {
            Type type = Type.GetType(nodeType);
            if (type == null)
                throw new ArgumentException($"Missing node type ({nodeType})");
            return CreateNode(type, position);
        }

        protected Node CreateNode(Type type, Vector2 position)
        {
            if (!type.IsSubclassOf(typeof(Node)))
                throw new ArgumentException("Type must be subclass of node.");
            Node node = (Node)Activator.CreateInstance(type,
                    nodeIdPool.NewID(),
                    position,
                    nodeStyle,
                    selectedNodeStyle,
                    inPointStyle,
                    outPointStyle);
            // assign method into events
            node.OnSelectNode = OnClickSelectNode;
            node.OnCancelSelectNode = OnClickCancelSelectNode;
            node.OnCopyNode = OnClickCopyNode;
            node.OnClickInPoint += OnClickInPoint;
            node.OnClickOutPoint += OnClickOutPoint;
            node.OnRemoveConnectionPoint = OnClickRemoveConnectionPoint;
            node.OnRemoveNode = OnClickRemoveNode;
            node.OnDrag += OnNodeDrag;

            nodes.Add(node);

            return node;
        }

        protected Node CreateNode<T>(Vector2 position) where T : Node
        {
            return CreateNode(typeof(T), position);
        }
        #endregion

        #region -- OnClick events --
        private void OnClickAddNode(Vector2 mousePosition)
        {
            CreateNode<Node>(mousePosition);
        }

        protected void OnClickRemoveConnectionPoint(ConnectionPoint point)
        {
            if (point != null)
            {
                connections.RemoveAll(conn => conn.inPoint == point || conn.outPoint == point);
            }
        }

        protected void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;
            if (selectedOutPoint != null)
            {
                // filt loop
                if (Node.LoopCheck(selectedOutPoint.node, selectedInPoint.node) && inPoint.ValidateConnectPoint(selectedOutPoint) && selectedOutPoint.ValidateConnectPoint(selectedInPoint))
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        protected void OnClickOutPoint(ConnectionPoint outPoint)
        {
            selectedOutPoint = outPoint;
            if (selectedInPoint != null)
            {
                // filt loop
                if (Node.LoopCheck(selectedOutPoint.node, selectedInPoint.node) && selectedInPoint.ValidateConnectPoint(outPoint) && selectedOutPoint.ValidateConnectPoint(selectedInPoint))
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        protected void OnClickRemoveNode(Node node)
        {
            if (connections != null)
            {
                foreach(var point in node.inPoints)
                {
                    for (int i = 0; i < point.Connections.Count; i++)
                    {
                        var conn = point.Connections.Last();
                        OnRemoveConnection(conn);
                    }
                    OnClickRemoveConnectionPoint(point);
                }
                foreach (var point in node.outPoints)
                {
                    for (int i = 0; i < point.Connections.Count; i++)
                    {
                        var conn = point.Connections.Last();
                        OnRemoveConnection(conn);
                    }
                    OnClickRemoveConnectionPoint(point);
                }
            }
            //nodeIdPool.RecycleID(node.GetInstanceID());
            nodes.Remove(node);
        }

        protected void OnNodeDrag(Node node, Vector2 delta)
        {
            if (SelectedNodes.All(n => n.EnableDrag))
            {
                foreach (var n in SelectedNodes)
                {
                    n.Move(delta);
                }
            }
        }

        private void OnRemoveConnection(Connection connection)
        {
            connection.inPoint.RemoveConnection(connection);
            connection.outPoint.RemoveConnection(connection);
            connections.Remove(connection);
        }

        protected virtual void OnClickSelectNode(Node node, bool control)
        {
            selectBox = false;
            DragNodes = true;
            clickOnNode = true;
            if (control)
            {
                if (SelectedNodes.Contains(node))
                {
                    SelectedNodes.Remove(node);
                    node.Select(false);
                }
                else
                {
                    SelectedNodes.Add(node);
                    node.Select(true);
                }
            }
            else
            {   
                SelectedNodes.Add(node);
                node.Select(true);
            }
        }

        protected virtual void OnClickCancelSelectNode(Node node)
        {   
            if (!SubwindowRect.Contains(Event.current.mousePosition))
            {
                SelectedNodes.Remove(node);
                node.Select(false);
            }
        }

        protected virtual void OnClickCopyNode(Node node)
        {
            copiedNode = node;
        }
        #endregion

        private void PasteNode(Vector2 position)
        {
            if (copiedNode != null)
            {
                CreateNode(copiedNode.GetType(), position);
            }
        }

        #region -- Connection methods -- 
        private void CreateConnection()
        {
            CreateConnection(selectedInPoint, selectedOutPoint);
        }

        private void CreateConnection(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            var conn = new Connection(inPoint, outPoint);
            if (!inPoint.ContainsConnection(conn))
            {
                inPoint.AddConnection(conn);
                outPoint.AddConnection(conn);
                connections.Add(conn);
                conn.OnRemoveConnection += OnRemoveConnection;
            }
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }
        #endregion


        private void Clear()
        {
            nodes.Clear();
            nodeIdPool.Reset();
            connections.Clear();
            BackToCenter();
            selectedInPoint = null;
            selectedOutPoint = null;
            SelectedNodes.Clear();
            //SelectedNode = null;
        }

        protected virtual void OnInitialized()
        {

        }

        #region -- File methods --
        public void NewFile()
        {
            SavePath = string.Empty;
            Clear();
            OnInitialized();
        }

        public void OpenSaveDialog()
        {
            string path = EditorUtility.SaveFilePanel("Save node state", Application.dataPath, "NewNodeState", "nstat");
            if (!string.IsNullOrEmpty(path))
            {
                SavePath = path;
                using (BinaryWriter writer =
                    new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    writer.Write(0);
                    BinarySave(writer);
                }
            }
        }

        public void QuickSave()
        {
            if (File.Exists(SavePath))
            {
                using (BinaryWriter writer =
                                    new BinaryWriter(File.Open(SavePath, FileMode.Create)))
                {
                    writer.Write(0);
                    BinarySave(writer);
                }
            }
            else
            {
                OpenSaveDialog();
            }
        }

        public void OpenLoadDialog()
        {
            string path = EditorUtility.OpenFilePanel("Load node state", Application.dataPath, "nstat");
            if (!string.IsNullOrEmpty(path))
            {
                SavePath = path;
                using (BinaryReader reader =
                    new BinaryReader(File.OpenRead(path)))
                {
                    int header = reader.ReadInt32();
                    BinaryLoad(reader);
                }
            }
        }

        public virtual void BinarySave(BinaryWriter writer)
        {
            writer.Write(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                writer.Write(nodes[i].GetType().AssemblyQualifiedName);
                nodes[i].BinarySave(writer);
            }
            writer.Write(connections.Count);
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].BinarySave(writer);
            }
        }

        public virtual void BinaryLoad(BinaryReader reader)
        {
            Clear();

            int nodesCount = reader.ReadInt32();
            for (int i = 0; i < nodesCount; i++)
            {
                string nodeTypeName = reader.ReadString();
                Type nodeType = Type.GetType(nodeTypeName);
                if (nodeType == null)
                {
                    Debug.Log($"Missing node type ({nodeTypeName})");
                }
                var node = (Node)Activator.CreateInstance(nodeType,
                    nodeStyle,
                    selectedNodeStyle,
                    inPointStyle,
                    outPointStyle);

                // assign methods into events
                node.OnSelectNode = OnClickSelectNode;
                node.OnCancelSelectNode = OnClickCancelSelectNode;
                node.OnCopyNode = OnClickCopyNode;
                node.OnClickInPoint += OnClickInPoint;
                node.OnClickOutPoint += OnClickOutPoint;
                node.OnRemoveConnectionPoint = OnClickRemoveConnectionPoint;
                node.OnRemoveNode = OnClickRemoveNode;

                // add node
                nodes.Add(node);
                nodes[i].BinaryLoad(reader);
                nodeIdPool.SetUsedId(nodes[i].GetInstanceID());
            }
            int connCount = reader.ReadInt32();
            for (int i = 0; i < connCount; i++)
            {
                int outNodeId = reader.ReadInt32();
                int outPointId = reader.ReadInt32();
                Node outNode = nodes.Find(n => n.GetInstanceID() == outNodeId);
                int inNodeId = reader.ReadInt32();
                int inPointId = reader.ReadInt32();
                Node inNode = nodes.Find(n => n.GetInstanceID() == inNodeId);
                ConnectionPoint inPoint = inNode.inPoints[inPointId];
                ConnectionPoint outPoint = outNode.outPoints[outPointId];
                CreateConnection(inPoint, outPoint);
            }
        }
        #endregion

        #region -- About methods -- 
        protected virtual void About()
        {
            string path = "Assets\\Node Based Editor\\About\\license.txt";
            string msg = File.ReadAllText(path);
            EditorUtility.DisplayDialog("About", msg, "Ok");
        }
        #endregion
    }



}