using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class QuaternionConnectPoint : ConnectionPoint
    {
        public QuaternionConnectPoint(Node node, ConnectionPointType type, GUIStyle style, string title = "", int maxConnect = 1) : base(node, type, style, title, maxConnect)
        {
        }

        public override bool ValidateConnectPoint(ConnectionPoint point)
        {
            return base.ValidateConnectPoint(point) && point.GetType() == typeof(QuaternionConnectPoint);
        }
    }
}