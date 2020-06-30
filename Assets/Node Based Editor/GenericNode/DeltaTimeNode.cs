using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class DeltaTimeNode : Node
    {
        public DeltaTimeNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "DeltaTime";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("t", () => 0);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }

        public DeltaTimeNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "DeltaTime";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("t", () => 0);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }
    }
}