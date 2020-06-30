using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class GravityNode : Node
    {
        public GravityNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Gravity";
            ShowTitle = false;
            AddOutPoint<Vector3ConnectPoint>("g", () => Physics.gravity);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }

        public GravityNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Gravity";
            ShowTitle = false;
            AddOutPoint<Vector3ConnectPoint>("g", () => Physics.gravity);

            style = defaultNodeStyle = CommonStyle.CyanNode;
            selectedNodeStyle = CommonStyle.SelectedCyanNode;
        }
    }
}