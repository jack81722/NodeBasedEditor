using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class ConnectionPoint
    {
        public Rect rect;
        public Vector2 offset;
        public ConnectionPointType type;
        public Node node;
        public GUIStyle style;
        public int MaxConnectionNum = -1;
        public List<ConnectionPoint> opposites { get; protected set; }
        public Func<ConnectionPoint, ConnectionPoint> OnClickConnectionPoint;
        public Action<ConnectionPoint, ConnectionPoint> OnConnect;
        public Action<ConnectionPoint, ConnectionPoint> OnDisconnect;
        public Func<ConnectionPoint, bool> OnRemoveConnectionPoint;

        public ConnectionPoint(
            Node node,
            ConnectionPointType type,
            GUIStyle style,
            Func<ConnectionPoint, ConnectionPoint> OnClickConnectionPoint,
            Action<ConnectionPoint, ConnectionPoint> OnConnect,
            Action<ConnectionPoint, ConnectionPoint> OnDisconnect,
            Func<ConnectionPoint, bool> OnRemoveConnectionPoint)
        {
            this.node = node;
            this.type = type;
            this.style = style;
            this.OnClickConnectionPoint = OnClickConnectionPoint;
            this.OnConnect = OnConnect;
            this.OnDisconnect = OnDisconnect;
            this.OnRemoveConnectionPoint = OnRemoveConnectionPoint;
            rect = new Rect(0, 0, 10f, 20f);
            opposites = new List<ConnectionPoint>();
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
                            var opposite = OnClickConnectionPoint?.Invoke(this);
                            if (opposite != null)
                            {
                                if (CanConnect())
                                {
                                    Connect(opposite);
                                    OnConnect(this, opposite);
                                    opposite.Connect(this);
                                    opposite.OnConnect(opposite, this);
                                }
                            }
                            e.Use();
                        }
                    }
                    if (e.button == 1)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    break;
            }
        }

        public bool CanConnect()
        {
            return MaxConnectionNum < 0 || opposites.Count < MaxConnectionNum;
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
            bool remove = OnRemoveConnectionPoint != null ? OnRemoveConnectionPoint.Invoke(this) : true;
            if (remove && opposites.Count > 0)
            {
                for (int i = 0; i < opposites.Count; i++)
                {
                    opposites[i].Disconnect(this);
                }
                opposites.Clear();
            }
        }

        public void Draw()
        {
            rect.x = node.rect.x + offset.x;
            rect.y = node.rect.y + offset.y;
            GUI.Box(rect, "", style);
        }

        public void Disconnect(ConnectionPoint opposite)
        {
            opposites.Remove(opposite);
            OnDisconnect(this, opposite);
        }

        public void Connect(ConnectionPoint opposite)
        {
            if (opposite != this && !opposites.Contains(opposite))
                opposites.Add(opposite);
        }

        public void Clear()
        {
            for(int i = 0; i< opposites.Count; i++)
            {
                opposites[i].Disconnect(this);
            }
            opposites.Clear();
        }
    }

    public enum ConnectionPointType { In, Out }
}