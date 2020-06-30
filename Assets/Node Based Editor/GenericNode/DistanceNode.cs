using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class DistanceNode : Node
    {
        public float Length;
        private Vector3 from, to;
        private ConnectionPoint fromPoint, toPoint;

        public DistanceNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Distance";
            ShowTitle = false;
            fromPoint = AddInPoint<DigitConnectPoint>("From");
            toPoint = AddInPoint<DigitConnectPoint>("To");
            AddOutPoint<FloatConnectPoint>("Length", () => Length);
        }

        public DistanceNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Distance";
            ShowTitle = false;
            fromPoint = AddInPoint<DigitConnectPoint>("From");
            toPoint = AddInPoint<DigitConnectPoint>("To");
            AddOutPoint<FloatConnectPoint>("Length", () => Length);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();

            if (fromPoint.HasConnected)
            {
                GUI.enabled = false;
                from = fromPoint.Value.CastToVector3();
            }
            DrawVector3("From:", ref from);
            if (fromPoint.HasConnected)
                GUI.enabled = true;

            if (toPoint.HasConnected)
            {
                GUI.enabled = false;
                to = toPoint.Value.CastToVector3();
            }
            DrawVector3("To:", ref to);
            if (toPoint.HasConnected)
                GUI.enabled = true;

            GUI.enabled = false;
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            GUILayout.Label("Length:", GUILayout.Width(50));
            Length = Vector3.Distance(from, to);
            EditorGUILayout.FloatField(Length, GUILayout.Width(140));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void DrawVector3(string label, ref Vector3 v)
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.Space();
            // x connect detection
            EditorGUILayout.LabelField("X", GUILayout.Width(10));
            v.x = EditorGUILayout.FloatField(v.x, GUILayout.Width(50));

            // y connect detection
            EditorGUILayout.LabelField("Y", GUILayout.Width(10));
            v.y = EditorGUILayout.FloatField(v.y, GUILayout.Width(50));

            // z connect detection
            EditorGUILayout.LabelField("Z", GUILayout.Width(10));
            v.z = EditorGUILayout.FloatField(v.z, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }


        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write(from.x);
            writer.Write(from.y);
            writer.Write(from.z);
            writer.Write(to.x);
            writer.Write(to.y);
            writer.Write(to.z);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            from.x = reader.ReadSingle();
            from.y = reader.ReadSingle();
            from.z = reader.ReadSingle();
            to.x = reader.ReadSingle();
            to.y = reader.ReadSingle();
            to.z = reader.ReadSingle();
        }
        #endregion
    }
}