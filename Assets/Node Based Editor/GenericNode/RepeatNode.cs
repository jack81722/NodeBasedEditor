using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class RepeatNode : Node
    {
        public RepeatNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Repeat";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("invoke");
            AddInPoint<IntConnectPoint>("times");
            AddOutPoint<EventConnectPoint>("event", null, 1);
            AddOutPoint<IntConnectPoint>("i", () => 0);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }

        public RepeatNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Repeat";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("invoke");
            AddInPoint<IntConnectPoint>("times");
            AddOutPoint<EventConnectPoint>("event", null, 1);
            AddOutPoint<IntConnectPoint>("i", () => 0);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }

        
    }
}