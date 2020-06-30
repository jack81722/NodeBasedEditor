using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class ParallelNode : Node
    {
        public ParallelNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Parallel";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            AddOutPoint<EventConnectPoint>("", null);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }

        public ParallelNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Parallel";
            ShowTitle = false;
            AddInPoint<EventConnectPoint>("");
            AddOutPoint<EventConnectPoint>("", null);

            style = defaultNodeStyle = CommonStyle.OrangeNode;
            selectedNodeStyle = CommonStyle.SelectedOrangeNode;
        }
    }
}