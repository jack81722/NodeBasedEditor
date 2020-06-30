using NodeEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class PowerNode : Node
    {
        private float Value, v, p;
        private ConnectionPoint vPoint, pPoint;

        public PowerNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Pow";
            ShowTitle = false;
            vPoint = AddInPoint<FloatConnectPoint>("v");
            pPoint = AddInPoint<FloatConnectPoint>("p");
            AddOutPoint<FloatConnectPoint>("pow", ()=> Value);
        }

        public PowerNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Pow";
            ShowTitle = false;
            vPoint = AddInPoint<FloatConnectPoint>("v");
            pPoint = AddInPoint<FloatConnectPoint>("p");
            AddOutPoint<FloatConnectPoint>("pow", () => Value);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();
            v = (float)Row(vPoint, v);
            p = (float)Row(pPoint, p);

            GUI.enabled = false;
            GUILayout.BeginHorizontal(GUILayout.Width(100));
            EditorGUILayout.LabelField("Pow:", GUILayout.Width(30));
            Value = EditorGUILayout.FloatField(Mathf.Pow(v, p), GUILayout.Width(70));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private object Row(ConnectionPoint inPoint, object value)
        {
            if (inPoint.HasConnected)
            {
                GUI.enabled = false;
                GUILayout.BeginHorizontal(GUILayout.Width(80));
                EditorGUILayout.LabelField($"{inPoint.title}:", GUILayout.Width(10));
                value = EditorGUILayout.FloatField(Convert.ToSingle(inPoint.Value), GUILayout.Width(70));
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.Width(80));
                EditorGUILayout.LabelField($"{inPoint.title}:", GUILayout.Width(10));
                value = EditorGUILayout.FloatField(Convert.ToSingle(value), GUILayout.Width(70));
                GUILayout.EndHorizontal();
            }
            return value;
        }

        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write(v);
            writer.Write(p);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            v = reader.ReadSingle();
            p = reader.ReadSingle();
        }
        #endregion
    }
}