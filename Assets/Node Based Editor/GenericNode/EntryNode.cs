using NodeEditor.Style;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class EntryNode : Node
    {
        public EntryNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Entry";
            ShowTitle = false;
            AddOutPoint<EventConnectPoint>("event", null, 1);

            EnableDrag = false;
            EnableCopy = false;

            style = defaultNodeStyle = CommonStyle.GreenNode;
            selectedNodeStyle = CommonStyle.SelectedGreenNode;
        }

        public EntryNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Entry";
            ShowTitle = false;
            AddOutPoint<EventConnectPoint>("event", null, 1);

            EnableDrag = false;
            EnableCopy = false;

            style = defaultNodeStyle = CommonStyle.GreenNode;
            selectedNodeStyle = CommonStyle.SelectedGreenNode;
        }

    }
}