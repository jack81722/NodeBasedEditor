using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor.Component
{
    public abstract class NodeLayoutComponent
    {
        public abstract void Draw(ref object value, Rect rect);
    }

    public class LabelComponent : NodeLayoutComponent
    {
        private GUIStyle style;

        public LabelComponent(GUIStyle style = null)
        {
            this.style = style;
        }

        public override void Draw(ref object value, Rect rect)
        {
            if (style != null)
                GUI.Label(rect, (string)value, style);
            else
                GUI.Label(rect, (string)value);
        }
    }


}