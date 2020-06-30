using NodeEditor.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Connection : IBinarySaveable
    {
        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;
        public Action<Connection> OnRemoveConnection;

        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
        }

        public void Draw()
        {
            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + Vector2.left * 50f,
                outPoint.rect.center - Vector2.left * 50f,
                Color.yellow,
                null,
                5f);

            if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                OnRemoveConnection?.Invoke(this);
            }
        }

        public void BinarySave(BinaryWriter writer)
        {
            writer.Write(outPoint.node.GetInstanceID());
            writer.Write(outPoint.node.outPoints.IndexOf(outPoint));
            writer.Write(inPoint.node.GetInstanceID());
            writer.Write(inPoint.node.inPoints.IndexOf(inPoint));
            //Debug.Log($"Save connection : ({outPoint.node.GetInstanceID()}, {outPoint.node.outPoints.IndexOf(outPoint)}) - ({inPoint.node.GetInstanceID()},{inPoint.node.inPoints.IndexOf(inPoint)})");
        }
    }
}