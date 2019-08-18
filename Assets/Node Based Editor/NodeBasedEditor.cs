using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class NodeBasedEditor : EditorWindow
{
    private IdentityPool nodeIdPool;
    private List<Node> nodes;
    private List<Connection> connections;

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private Vector2 offset;
    private Vector2 drag;

    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow()
    {
        NodeBasedEditor window = GetWindow<NodeBasedEditor>();
        window.titleContent = new GUIContent("Node Based Editor");
    }

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

    #region GUI methods
    private void OnGUI()
    {
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
        if(selectedInPoint != null && selectedOutPoint == null)
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

        if(selectedOutPoint != null && selectedInPoint == null)
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
        if(nodes != null)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnections()
    {
        if(connections != null)
        {
            for(int i = 0; i < connections.Count; i++)
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
                if(e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if(e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if(nodes != null)
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
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    #region OnClick events
    private void OnClickAddNode(Vector2 mousePosition)
    {
        Node newNode = new Node(
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
                OnClickRemoveConnectionPoint,
                OnClickRemoveNode);
        nodes.Add(newNode);
    }

    private void OnClickRemoveConnectionPoint(ConnectionPoint point)
    {
        if (point != null)
        {
            connections.RemoveAll(conn => conn.inPoint == point || conn.outPoint == point);
        }
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if(selectedOutPoint != null)
        {
            if(selectedOutPoint.node != selectedInPoint.node)
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

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
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

    private void OnClickRemoveNode(Node node)
    {
        if(connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();
            for(int i = 0; i < connections.Count; i++)
            {
                if(node.inPoints.Contains(connections[i].inPoint) || node.outPoints.Contains(connections[i].outPoint))
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
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }
    #endregion

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
        offset = Vector2.zero;
    }

    private void Clear()
    {   
        nodes.Clear();
        nodeIdPool.Reset();
        connections.Clear();
        BackToCenter();
    }

    public void OpenSaveDialog()
    {
        string path = EditorUtility.SaveFilePanel("Save node state", Application.dataPath, "NewNodeState", "nstat");
        if (!string.IsNullOrEmpty(path))
        {
            using (BinaryWriter writer =
                new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(0);
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
                Load(reader);
            }
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(nodes.Count);
        for(int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Save(writer);
        }
        writer.Write(connections.Count);
        for(int i = 0; i < connections.Count; i++)
        {
            connections[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        Clear();

        int nodesCount = reader.ReadInt32();
        for(int i = 0; i < nodesCount; i++)
        {
            nodes.Add(
                new Node(
                nodeStyle,
                selectedNodeStyle,
                inPointStyle,
                outPointStyle,
                OnClickInPoint,
                OnClickOutPoint,
                OnClickRemoveConnectionPoint,
                OnClickRemoveNode));
            nodes[i].Load(reader);
            nodeIdPool.SetUsedId(nodes[i].GetInstanceID());
        }
        int connCount = reader.ReadInt32();
        for(int i = 0; i < connCount; i++)
        {
            int outNodeId = reader.ReadInt32();
            int outPointId = reader.ReadInt32();
            Node outNode = nodes.Find(n => n.GetInstanceID() == outNodeId);
            int inNodeId = reader.ReadInt32();
            int inPointId = reader.ReadInt32();
            Node inNode = nodes.Find(n => n.GetInstanceID() == inNodeId);
            connections.Add(new Connection(inNode.inPoints[inPointId], outNode.outPoints[outPointId], OnClickRemoveConnection));
        }
    }
}
