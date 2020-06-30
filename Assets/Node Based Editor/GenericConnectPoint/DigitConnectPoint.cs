using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class DigitConnectPoint : ConnectionPoint
    {
        public EDigitType DigitType { get; }

        public DigitConnectPoint(Node node, ConnectionPointType type, GUIStyle style, string title = "", int maxConnect = 1) : base(node, type, style, title, maxConnect) { }

        public override bool ValidateConnectPoint(ConnectionPoint point)
        {
            var type = point.GetType();
            return base.ValidateConnectPoint(point) && (type.IsSubclassOf(typeof(DigitConnectPoint)) || type == typeof(DigitConnectPoint));
        }
    }

    public enum EDigitType
    {
        Int,
        Float,
        Vector2,
        Vector3,
        Vector4
    }
}