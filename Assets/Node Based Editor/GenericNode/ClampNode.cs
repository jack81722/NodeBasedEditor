using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class ClampNode : Node
    {
        private float input, min, max = 1, output;
        private ConnectionPoint inPoint, minPoint, maxPoint;

        public ClampNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Clamp";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("v");
            minPoint = AddInPoint<FloatConnectPoint>("min");
            maxPoint = AddInPoint<FloatConnectPoint>("max");
            AddOutPoint<FloatConnectPoint>("clamp", () => output);
        }

        public ClampNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Clamp";
            ShowTitle = false;
            inPoint = AddInPoint<FloatConnectPoint>("v");
            minPoint = AddInPoint<FloatConnectPoint>("min");
            maxPoint = AddInPoint<FloatConnectPoint>("max");
            AddOutPoint<FloatConnectPoint>("clamp", () => output);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();
            input = (float)Row(inPoint, input);
            min = (float)Row(minPoint, min);
            max = (float)Row(maxPoint, max);

            GUI.enabled = false;
            GUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Clamp(v, min, max):", GUILayout.Width(120));
            output = EditorGUILayout.FloatField(Mathf.Clamp(input, min, max), GUILayout.Width(80));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private object Row(ConnectionPoint inPoint, object value)
        {
            if (inPoint.HasConnected)
            {
                GUI.enabled = false;
                GUILayout.BeginHorizontal(GUILayout.Width(90));
                EditorGUILayout.LabelField($"{inPoint.title}:", GUILayout.Width(30));
                value = EditorGUILayout.FloatField(Convert.ToSingle(inPoint.Value), GUILayout.Width(60));
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.Width(90));
                EditorGUILayout.LabelField($"{inPoint.title}:", GUILayout.Width(30));
                value = EditorGUILayout.FloatField(Convert.ToSingle(value), GUILayout.Width(60));
                GUILayout.EndHorizontal();
            }
            return value;
        }

        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write(input);
            writer.Write(min);
            writer.Write(max);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            input = reader.ReadSingle();
            min = reader.ReadSingle();
            max = reader.ReadSingle();
        }
        #endregion
    }
}