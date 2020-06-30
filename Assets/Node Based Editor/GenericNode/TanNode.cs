using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class TanNode : Node
    {
        private float value;
        private float inValue;
        private ConnectionPoint inPoint;

        public TanNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Tan";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("t");
            AddOutPoint<FloatConnectPoint>("tan", GetValue);
        }

        public TanNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Tan";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("t");
            AddOutPoint<FloatConnectPoint>("tan", GetValue);
        }

        public override void OnDrawDetail()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(80));
            if (inPoint.HasConnected)
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField("t:", GUILayout.Width(10));
                inValue = EditorGUILayout.FloatField((float)inPoint.Value, GUILayout.Width(70));
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.LabelField("t:", GUILayout.Width(10));
                inValue = EditorGUILayout.FloatField(inValue, GUILayout.Width(70));
            }
            GUILayout.EndHorizontal();

            GUI.enabled = false;
            GUILayout.BeginHorizontal(GUILayout.Width(120));
            EditorGUILayout.LabelField("Tan(t):", GUILayout.Width(50));
            EditorGUILayout.FloatField(Mathf.Tan(inValue), GUILayout.Width(70));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private object GetValue()
        {
            return value;
        }
    }
}
