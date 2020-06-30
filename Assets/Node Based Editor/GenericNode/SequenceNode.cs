using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class SequenceNode : Node
    {
        private const int minOutPoint = 1;

        public SequenceNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Sequence";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            DynamicAddOutPoint();

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }

        public SequenceNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Sequence";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            DynamicAddOutPoint();

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }
    
        private void DynamicAddOutPoint()
        {
            var outPoint = AddOutPoint<EventConnectPoint>("", null, 1);
            outPoint.OnConnected += OnPointConnected;
            outPoint.OnDisconnected += OnPointDisconnected;
        }

        private void DynamicRemoveOutPoint()
        {
            outPoints.RemoveAt(outPoints.Count - 1);
        }

        private void OnPointConnected(ConnectionPoint i, ConnectionPoint o)
        {
            if(outPoints[outPoints.Count - 1] == o)
            {
                DynamicAddOutPoint();
            }

            ModifyConnectionPointHeight();
        }

        private void OnPointDisconnected(ConnectionPoint i, ConnectionPoint o)
        {
            while (outPoints.Count > minOutPoint && !outPoints[outPoints.Count - 2].HasConnected)
            {
                DynamicRemoveOutPoint();
            }
            ModifyConnectionPointHeight();
        }
    }
}