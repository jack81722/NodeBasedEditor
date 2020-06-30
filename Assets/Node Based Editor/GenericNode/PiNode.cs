using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class PiNode : Node
    {
        public PiNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Pi";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("pi", () => Mathf.PI);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }

        public PiNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Pi";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("pi", () => Mathf.PI);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }
    }
}