using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class Node
{
    private int nodeId;
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;
    private Action<Node> onSelectNode, onCancelSelectNode, onCopyNode;

    public List<ConnectionPoint> inPoints;
    protected GUIStyle inPointStyle;
    protected Action<ConnectionPoint> onClickInPoint;

    public List<ConnectionPoint> outPoints;
    protected GUIStyle outPointStyle;
    protected Action<ConnectionPoint> onClickOutPoint;

    protected Action<ConnectionPoint> onRemoveConnectionPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<Node> OnRemoveNode;

    public Node(
        GUIStyle nodeStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle,
        GUIStyle outPointStyle,
        Action<Node> onSelectNode,
        Action<Node> onCancelSelectNode,
        Action<Node> onCopyNode,
        Action<ConnectionPoint> onClickInPoint,
        Action<ConnectionPoint> onClickOutPoint,
        Action<ConnectionPoint> onRemoveConnectionPoint,
        Action<Node> onClickRemoveNode)
    {
        style = nodeStyle;

        this.onSelectNode = onSelectNode;
        this.onCancelSelectNode = onCancelSelectNode;
        this.onCopyNode = onCopyNode;

        inPoints = new List<ConnectionPoint>();
        this.inPointStyle = inPointStyle;
        this.onClickInPoint = onClickInPoint;

        outPoints = new List<ConnectionPoint>();
        this.outPointStyle = outPointStyle;
        this.onClickOutPoint = onClickOutPoint;

        this.onRemoveConnectionPoint = onRemoveConnectionPoint;

        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;

        OnRemoveNode = onClickRemoveNode;
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
        Action<Node> onSelectNode,
        Action<Node> onCancelSelectNode,
        Action<Node> onCopyNode,
        Action<ConnectionPoint> onClickInPoint,
        Action<ConnectionPoint> onClickOutPoint,
        Action<ConnectionPoint> onRemoveConnectionPoint,
        Action<Node> onClickRemoveNode)
    {
        nodeId = id;
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;

        this.onSelectNode = onSelectNode;
        this.onCancelSelectNode = onCancelSelectNode;
        this.onCopyNode = onCopyNode;

        inPoints = new List<ConnectionPoint>();
        this.inPointStyle = inPointStyle;
        this.onClickInPoint = onClickInPoint;

        outPoints = new List<ConnectionPoint>();
        this.outPointStyle = outPointStyle;
        this.onClickOutPoint = onClickOutPoint;

        this.onRemoveConnectionPoint = onRemoveConnectionPoint;

        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;

        OnRemoveNode = onClickRemoveNode;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {   
        
        for(int i = 0; i < inPoints.Count; i++)
        {
            inPoints[i].Draw();
        }
        for(int i = 0; i < outPoints.Count; i++)
        {
            outPoints[i].Draw();
        }
        GUI.Box(rect, title, style);
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(rect.x, rect.y + rect.height / 2 - 10, rect.width, 20), nodeId.ToString(), titleStyle);
    }

    public bool ProcessEvents(Event e)
    {
        ProcessConnectionPointEvents(e);
        switch (e.type)
        {
            case EventType.MouseDown:
                if(e.button == 0)
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
                        if(isSelected)
                            onCancelSelectNode(this);
                        isSelected = false;
                        style = defaultNodeStyle;
                        
                    }
                }
                
                if(e.button == 1 && isSelected)
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                isDragged = false;
                break;
            case EventType.MouseDrag:
                if(e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    public void AddOutPoint(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            ConnectionPoint point = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint, OnRemoveConnectionPoint);
            point.offset.x = rect.width - 8f;
            outPoints.Add(point);
        }

        LayoutConnectionPoints();
    }

    public void AddInPoint(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            ConnectionPoint point = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint, OnRemoveConnectionPoint);
            point.offset.x = -point.rect.width + 8f;
            inPoints.Add(point);
        }

        LayoutConnectionPoints();
    }

    private void LayoutConnectionPoints()
    {
        float heightOfPoint = 30f;
        float interval = 10f;
        int maxPointNum = Mathf.Max(inPoints.Count, outPoints.Count);
        if (maxPointNum > 0)
        {
            float totalHeight = maxPointNum * heightOfPoint + (maxPointNum + 1) * interval;
            rect.height = totalHeight;

            LayoutConnectionPointList(inPoints, totalHeight);
            LayoutConnectionPointList(outPoints, totalHeight);
        }
    }

    private void LayoutConnectionPointList(List<ConnectionPoint> list, float totalHeight)
    {
        float divPoint = 1f / (list.Count + 1);
        for (int i = 0; i < list.Count; i++)
        {
            list[i].offset.y = rect.height * divPoint * (i + 1) - list[i].rect.height * 0.5f ;
        }
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
        for(int i = 0; i < list.Count; i++)
        {
            list[i].ProcessEvents(e);
        }
    }

    private void OnRemoveConnectionPoint(ConnectionPoint point)
    {
        if (point.type == ConnectionPointType.In)
            inPoints.Remove(point);
        else
            outPoints.Remove(point);
        onRemoveConnectionPoint?.Invoke(point);
        LayoutConnectionPoints();
    }

    protected virtual void AddGenericItem(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        menu.AddItem(new GUIContent("Add In Point"), false, () => AddInPoint());
        menu.AddItem(new GUIContent("Add Out Point"), false, () => AddOutPoint());
        
        menu.AddItem(new GUIContent("Copy"), false, () => onCopyNode(this));
    }

    private void OnClickRemoveNode()
    {
        OnRemoveNode?.Invoke(this);
    }

    public int GetInstanceID()
    {
        return nodeId;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(nodeId);
        writer.Write(rect.x);
        writer.Write(rect.y);
        writer.Write(rect.width);
        writer.Write(rect.height);
        writer.Write(inPoints.Count);
        writer.Write(outPoints.Count);
    }

    public void Load(BinaryReader reader)
    {
        nodeId = reader.ReadInt32();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float width = reader.ReadSingle();
        float height = reader.ReadSingle();
        rect = new Rect(x, y, width, height);

        int inPointsCount = reader.ReadInt32();
        for(int i = 0; i < inPointsCount; i++)
        {
            AddInPoint();
        }
        int outPointsCount = reader.ReadInt32();
        for(int i = 0; i < outPointsCount; i++)
        {
            AddOutPoint();
        }
    }
}
