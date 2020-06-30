using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class InvertNode : Node
    {
        public InvertNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Invert";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            AddOutPoint<EventConnectPoint>("", null, 1);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }

        public InvertNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Invert";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            AddOutPoint<EventConnectPoint>("", null, 1);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }
    }
}