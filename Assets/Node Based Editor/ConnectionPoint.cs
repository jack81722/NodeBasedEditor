using NodeEditor.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class ConnectionPoint
    {
        public string title = string.Empty;
        public Rect rect;
        public Vector2 offset;
        public ConnectionPointType type;
        public Node node { get; }
        public GUIStyle style;
        public Action<ConnectionPoint> OnConnectPoint;
        public Action<ConnectionPoint> OnRemoveConnectionPoint;
        public Action<ConnectionPoint, ConnectionPoint> OnConnected;
        public Action<ConnectionPoint, ConnectionPoint> OnDisconnected;

        public int MaxOut = -1;
        public int MaxIn = 1;
        public HashSet<Connection> Connections { get; protected set; } = new HashSet<Connection>();
        public ConnectionPoint Opponent
        {
            get
            {
                var conn = Connections.First();
                if (type == ConnectionPointType.In)
                    return conn.outPoint;
                else
                    return conn.inPoint;
            }
        }
        public Func<object> GetFunc;

        public bool HasConnected { get => Connections.Count > 0; }
        public object Value
        {
            get
            {
                if (GetFunc == null)
                    return null;
                return GetFunc.Invoke();
            }
        }

        public ConnectionPoint(
            Node node,
            ConnectionPointType type,
            GUIStyle style,
            string title = "",
            int maxConnect = 1)
        {
            this.title = title;

            this.node = node;
            this.type = type;
            this.style = style;
            rect = new Rect(0, 0, 10f, 20f);

            if (type == ConnectionPointType.In)
                MaxIn = maxConnect;
            else if (type == ConnectionPointType.Out)
                MaxOut = maxConnect;
        }

        public virtual void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            OnConnectPoint?.Invoke(this);
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

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            AddGenericItem(genericMenu);
            genericMenu.ShowAsContext();
        }

        public virtual bool ValidateConnectPoint(ConnectionPoint point)
        {
            var maxConnect = type == ConnectionPointType.In ? MaxIn : MaxOut;
            if (maxConnect < 0)
                return true;
            else
                return Connections.Count < maxConnect;
        }

        public void AddConnection(Connection connection)
        {
            Connections.Add(connection);
            if (type == ConnectionPointType.In)
                GetFunc = connection.outPoint.GetFunc;
            OnConnected?.Invoke(connection.inPoint, connection.outPoint);
        }

        public void RemoveConnection(Connection connection)
        {
            Connections.Remove(connection);
            OnDisconnected?.Invoke(connection.inPoint, connection.outPoint);
            if (type == ConnectionPointType.In)
                GetFunc = null;
        }

        public bool ContainsConnection(Connection connection)
        {
            return Connections.FirstOrDefault(conn => conn.inPoint == connection.inPoint && conn.outPoint == connection.outPoint) != null;
        }

        protected virtual void AddGenericItem(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Remove connection point"), false, RemoveConnectionPoint);
        }

        public void RemoveConnectionPoint()
        {
            OnRemoveConnectionPoint?.Invoke(this);
        }

        public void Draw(float zoom = 1)
        {
            if (type == ConnectionPointType.In)
            {
                rect.x = node.scaleRect.x + offset.x * zoom - rect.width * zoom;
                rect.y = node.scaleRect.y + offset.y;
            }
            else
            {
                rect.x = node.scaleRect.x + offset.x * zoom + node.scaleRect.width;
                rect.y = node.scaleRect.y + offset.y;
            }

            var scaleWidth = rect.width * zoom;
            var scaleHeight = rect.height * zoom;
            GUI.Box(new Rect(rect.x, rect.y, scaleWidth, scaleHeight), "", style);
            if (!string.IsNullOrEmpty(title))
            {
                if (type == ConnectionPointType.In)
                    GUI.Label(new Rect(rect.x + scaleWidth - 60 * zoom, rect.y - 2 * zoom, 45 * zoom, 20 * zoom), title, CommonStyle.InPointTitle);
                else if (type == ConnectionPointType.Out)
                    GUI.Label(new Rect(rect.x + scaleWidth + 5 * zoom, rect.y - 2 * zoom, 45 * zoom, 20 * zoom), title, CommonStyle.OutPointTitle);
            }
        }

        public void Clear()
        {
            if (type == ConnectionPointType.In)
            {
                foreach (var conn in Connections)
                {
                    conn.outPoint.RemoveConnection(conn);
                }
            }
            else
            {
                foreach (var conn in Connections)
                {
                    conn.inPoint.RemoveConnection(conn);
                }
            }
        }

    }

    public enum ConnectionPointType { In, Out }
}