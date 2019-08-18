using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConnectionPoint
{
    public Rect rect;
    public Vector2 offset;
    public ConnectionPointType type;
    public Node node;
    public GUIStyle style;
    public Action<ConnectionPoint> OnClickConnectionPoint;
    public Action<ConnectionPoint> OnRemoveConnectionPoint;

    public ConnectionPoint (
        Node node, 
        ConnectionPointType type, 
        GUIStyle style, 
        Action<ConnectionPoint> OnClickConnectionPoint,
        Action<ConnectionPoint> OnRemoveConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        this.OnRemoveConnectionPoint = OnRemoveConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        OnClickConnectionPoint?.Invoke(this);
                        e.Use();
                    }
                }
                if (e.button == 1)
                {
                    if(rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                }
                break;
        }
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        AddGenericItem(genericMenu);
        genericMenu.ShowAsContext();
    }

    protected virtual void AddGenericItem(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Remove connection point"), false, OnClickRemoveConnectionPoint);
    }

    private void OnClickRemoveConnectionPoint()
    {
        OnRemoveConnectionPoint?.Invoke(this);
    }

    public void Draw()
    {
        rect.x = node.rect.x + offset.x;
        rect.y = node.rect.y + offset.y;
        GUI.Box(rect, "", style);
    }

}



public enum ConnectionPointType { In, Out }
