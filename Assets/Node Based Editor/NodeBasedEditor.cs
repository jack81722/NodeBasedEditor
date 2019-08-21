using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        protected IdentityPool nodeIdPool;
        protected List<Node> nodes;
        protected List<Connection> connections;

        protected GUIStyle nodeStyle;
        protected GUIStyle selectedNodeStyle;
        protected GUIStyle inPointStyle;
        protected GUIStyle outPointStyle;

        protected Node selectedNode;
        protected Node copiedNode;
        protected bool clickConnectionPoint;
        protected ConnectionPoint selectedInPoint;
        protected ConnectionPoint selectedOutPoint;

        protected Vector2 offset;
        protected Vector2 drag;

        //[MenuItem("Window/Node Based Editor")]
        //private static void OpenWindow()
        //{
        //    NodeBasedEditor window = GetWindow<NodeBasedEditor>();
        //    window.titleContent = new GUIContent("Node Based Editor");
        //}

        private void OnEnable()
        {
            nodeIdPool = new IdentityPool();
            nodes = new List<Node>();
            connections = new List<Connection>();

            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);
        }

        protected virtual void OnCreateNode(Node node)
        {
            // add events
            node.onSelectNode = OnClickSelectNode;
            node.onCancelSelectNode = OnClickCancelSelectNode;
            node.onCopyNode = OnClickCopyNode;
            node.onRemoveNode = OnClickRemoveNode;
        }

        #region GUI methods
        private void OnGUI()
        {
            clickConnectionPoint = false;
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolbar();
            GUILayout.EndHorizontal();

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed)
                Repaint();
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
            if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
            {
                BackToCenter();
            }
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                Clear();
            }
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            float tb = 16f;
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing) + 1;
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i + newOffset.x, tb, 0), new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                if (gridSpacing * j + newOffset.y > tb)
                    Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j + newOffset.y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawConnectionLine(Event e)
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
                    2f);

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
                    2f);

                GUI.changed = true;
            }
        }

        private void DrawNodes()
        {
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Draw();
                }
            }
        }

        private void DrawConnections()
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

        private void OnDrag(Vector2 delta)
        {
            drag = delta;
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if(e.button == 0)
                    {
                        if (!clickConnectionPoint)
                        {
                            selectedInPoint = null;
                            selectedOutPoint = null;
                        }
                    }
                    if (e.button == 1)
                    {   
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;

                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete)
                    {
                        OnClickRemoveNode(selectedNode);
                        selectedNode = null;
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
                    if (e.commandName == "Copy")
                    {
                        OnClickCopyNode(selectedNode);
                    }
                    else if (e.commandName == "Paste")
                    {
                        PasteNode(new Vector2(50, 50));
                    }
                    break;
            }
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
            AddContextMenuItem(genericMenu, mousePosition);
            genericMenu.ShowAsContext();
        }

        protected virtual void AddContextMenuItem(GenericMenu menu, Vector2 mousePosition)
        {
            menu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
            menu.AddItem(new GUIContent("Paste"), false, () => PasteNode(mousePosition));
        }

        #region OnClick events
        protected void OnClickAddNode(Vector2 mousePosition)
        {
            Node node = CreateNode(
                    nodeIdPool.NewID(),
                    mousePosition,
                    200,
                    50,
                    nodeStyle,
                    selectedNodeStyle,
                    inPointStyle,
                    outPointStyle,
                    OnClickInPoint,
                    OnClickOutPoint,
                    OnClickRemoveConnectionPoint);
            OnCreateNode(node);
            nodes.Add(node);
        }

        protected void OnClickRemoveConnectionPoint(ConnectionPoint point)
        {
            if (point != null)
            {
                connections.RemoveAll(conn => conn.inPoint == point || conn.outPoint == point);
            }
        }

        protected ConnectionPoint OnClickInPoint(ConnectionPoint inPoint)
        {
            ConnectionPoint opposite = null;
            if (inPoint.CanConnect())
            {
                selectedInPoint = inPoint;
                clickConnectionPoint = true;
                if (selectedOutPoint != null)
                {
                    if (selectedOutPoint.node != selectedInPoint.node)
                    {
                        opposite = selectedOutPoint;
                        CreateConnection();
                        ClearConnectionSelection();
                    }
                    else
                    {
                        ClearConnectionSelection();
                    }
                }
            }
            return opposite;
        }

        protected ConnectionPoint OnClickOutPoint(ConnectionPoint outPoint)
        {
            ConnectionPoint opposite = null;
            if (outPoint.CanConnect())
            {
                selectedOutPoint = outPoint;
                clickConnectionPoint = true;
                if (selectedInPoint != null)
                {
                    if (selectedOutPoint.node != selectedInPoint.node)
                    {
                        opposite = selectedInPoint;
                        CreateConnection();
                        ClearConnectionSelection();
                    }
                    else
                    {
                        ClearConnectionSelection();
                    }
                }
            }
            return opposite;
        }

        protected void OnClickRemoveNode(Node node)
        {
            if (connections != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();
                for (int i = 0; i < connections.Count; i++)
                {
                    if (node.inPoints.Contains(connections[i].inPoint) || node.outPoints.Contains(connections[i].outPoint))
                    {   
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    connections.Remove(connectionsToRemove[i]);
                }

                connectionsToRemove = null;
            }
            //nodeIdPool.RecycleID(node.GetInstanceID());
            nodes.Remove(node);
            foreach(var point in node.inPoints)
            {
                point.Clear();
            }
            foreach(var point in node.outPoints)
            {
                point.Clear();
            }

        }

        protected void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }

        protected void OnClickSelectNode(Node node)
        {
            selectedNode = node;
        }

        protected void OnClickCancelSelectNode(Node node)
        {
            selectedNode = null;
        }

        protected void OnClickCopyNode(Node node)
        {
            copiedNode = node;
        }
        #endregion

        protected virtual void PasteNode(Vector2 position)
        {
            if (copiedNode != null)
            {
                Node node = CreateNode(
                    nodeIdPool.NewID(),
                    position,
                    200,
                    50,
                    nodeStyle,
                    selectedNodeStyle,
                    inPointStyle,
                    outPointStyle,
                    OnClickInPoint,
                    OnClickOutPoint,
                    OnClickRemoveConnectionPoint);
                OnCreateNode(node);
                node.title = copiedNode.title;
                node.AddInPoint(copiedNode.inPoints.Count);
                node.AddOutPoint(copiedNode.outPoints.Count);
                nodes.Add(node);
            }
        }

        private void CreateConnection()
        {
            connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private void BackToCenter()
        {
            Vector2 back = -offset;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(back);
            }
            offset = Vector2.zero;
        }

        private void Clear()
        {
            nodes.Clear();
            nodeIdPool.Reset();
            connections.Clear();
            BackToCenter();
        }

        #region Save/Load methods
        public void OpenSaveDialog()
        {
            string path = EditorUtility.SaveFilePanel("Save node state", Application.dataPath, "NewNodeState", "nstat");
            if (!string.IsNullOrEmpty(path))
            {
                using (BinaryWriter writer =
                    new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    writer.Write(1);
                    Save(writer);
                }
            }
        }

        public void OpenLoadDialog()
        {
            string path = EditorUtility.OpenFilePanel("Load node state", Application.dataPath, "nstat");
            if (!string.IsNullOrEmpty(path))
            {
                using (BinaryReader reader =
                    new BinaryReader(File.OpenRead(path)))
                {
                    int header = reader.ReadInt32();
                    Load(reader, header);
                }
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Save(writer);
            }
            writer.Write(connections.Count);
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Save(writer);
            }
        }

        public void Load(BinaryReader reader, int header)
        {
            Clear();

            int nodesCount = reader.ReadInt32();
            for (int i = 0; i < nodesCount; i++)
            {
                Node node = CreateNode(
                    nodeStyle,
                    selectedNodeStyle,
                    inPointStyle,
                    outPointStyle,
                    OnClickInPoint,
                    OnClickOutPoint,
                    OnClickRemoveConnectionPoint);
                OnCreateNode(node);
                // register
                nodes.Add(node);
                nodes[i].Load(reader, header);
                nodeIdPool.SetUsedId(nodes[i].GetInstanceID());
            }
            int connCount = reader.ReadInt32();
            for (int i = 0; i < connCount; i++)
            {
                int outNodeId = reader.ReadInt32();
                int outPointId = reader.ReadInt32();
                Node outNode = nodes.Find(n => n.GetInstanceID() == outNodeId);
                ConnectionPoint outPoint = outNode.outPoints[outPointId];
                int inNodeId = reader.ReadInt32();
                int inPointId = reader.ReadInt32();
                Node inNode = nodes.Find(n => n.GetInstanceID() == inNodeId);
                ConnectionPoint inPoint = inNode.inPoints[inPointId];
                connections.Add(new Connection(inNode.inPoints[inPointId], outNode.outPoints[outPointId], OnClickRemoveConnection));
                inPoint.Connect(outPoint);
                outPoint.Connect(inPoint);
            }
        }
        #endregion

        public virtual Node CreateNode(
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        {
            return new Node(
                nodeStyle,
                selectedStyle,
                inPointStyle,
                outPointStyle,
                onClickInPoint,
                onClickOutPoint,
                onRemoveConnectionPoint);
        }

        public virtual Node CreateNode(
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
            return new Node(
                id,
                position, width,
                height,
                nodeStyle,
                selectedStyle,
                inPointStyle,
                outPointStyle,
                onClickInPoint,
                onClickOutPoint,
                onRemoveConnectionPoint);
        }
    }
}