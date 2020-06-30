using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class Vector2ConnectPoint : DigitConnectPoint
    {
        public Vector2ConnectPoint(Node node, ConnectionPointType type, GUIStyle style, string title = "", int maxConnect = 1) : base(node, type, style, title, maxConnect)
        {
        }

        public override bool ValidateConnectPoint(ConnectionPoint point)
        {
            var result = base.ValidateConnectPoint(point);
            return result;
        }
    }
}