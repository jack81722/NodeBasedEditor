using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class IntConnectPoint : DigitConnectPoint
    {
        public IntConnectPoint(Node node, ConnectionPointType type, GUIStyle style, string title = "", int maxConnect = 1) : base(node, type, style, title, maxConnect) { }

        public override bool ValidateConnectPoint(ConnectionPoint point)
        {
            var t = point.GetType();
            var match = t == typeof(IntConnectPoint) || t == typeof(DigitConnectPoint) || t == typeof(FloatConnectPoint);
            var maxConnect = type == ConnectionPointType.In ? MaxIn : MaxOut;
            if (maxConnect < 0)
                return match;
            else
                return match && Connections.Count < maxConnect;
        }
    }
}