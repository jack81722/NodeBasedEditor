using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Connection
    {
        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;
        public Action<Connection> OnClickRemoveConnection;

        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            this.OnClickRemoveConnection = OnClickRemoveConnection;
        }

        public void Draw()
        {
            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + Vector2.left * 50f,
                outPoint.rect.center - Vector2.left * 50f,
                Color.white,
                null,
                2f);

            if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                inPoint.Disconnect(outPoint);
                outPoint.Disconnect(inPoint);
                OnClickRemoveConnection?.Invoke(this);
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(outPoint.node.GetInstanceID());
            writer.Write(outPoint.node.outPoints.IndexOf(outPoint));
            writer.Write(inPoint.node.GetInstanceID());
            writer.Write(inPoint.node.inPoints.IndexOf(inPoint));
        }


    }
}