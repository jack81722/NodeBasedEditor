using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace NodeEditor.Generic
{
    public class SqrtNode : Node
    {
        private float value;
        private float input;
        private ConnectionPoint inPoint;

        public SqrtNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Sqrt";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("v");
            AddOutPoint<FloatConnectPoint>("sqrt", () => value);
        }

        public SqrtNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Sqrt";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("v");
            AddOutPoint<FloatConnectPoint>("sqrt", () => value);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();

            if(inPoint.HasConnected)
            {
                GUI.enabled = false;
                GUILayout.BeginHorizontal(GUILayout.Width(80));
                EditorGUILayout.LabelField("v", GUILayout.Width(10));
                input = EditorGUILayout.FloatField(Convert.ToSingle(inPoint.Value), GUILayout.Width(70));
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.Width(80));
                EditorGUILayout.LabelField("v", GUILayout.Width(10));
                input = EditorGUILayout.FloatField(input, GUILayout.Width(70));
                GUILayout.EndHorizontal();
            }

            GUI.enabled = false;
            GUILayout.BeginHorizontal(GUILayout.Width(120));
            EditorGUILayout.LabelField("Sqrt(v):", GUILayout.Width(50));
            value = EditorGUILayout.FloatField(Mathf.Sqrt(input), GUILayout.Width(70));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write(input);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            input = reader.ReadSingle();
        }
        #endregion
    }
}