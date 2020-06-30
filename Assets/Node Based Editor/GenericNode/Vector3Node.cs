using NodeEditor.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class Vector3Node : Node
    {
        public Vector3 Vector;
        private ConnectionPoint xPoint, yPoint, zPoint;

        public Vector3Node(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Vector3";
            ShowTitle = false;
            xPoint = AddInPoint<DigitConnectPoint>("X");
            yPoint = AddInPoint<DigitConnectPoint>("Y");
            zPoint = AddInPoint<DigitConnectPoint>("Z");
            AddOutPoint<Vector3ConnectPoint>("V", GetV);
            AddOutPoint<FloatConnectPoint>("X", GetX);
            AddOutPoint<FloatConnectPoint>("Y", GetY);
            AddOutPoint<FloatConnectPoint>("Z", GetZ);
            InPointAlignment = EConnectPointAlignment.Upper;
            OutPointAlignment = EConnectPointAlignment.Upper;
        }

        public Vector3Node(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Vector3";
            ShowTitle = false;
            xPoint = AddInPoint<DigitConnectPoint>("X");
            yPoint = AddInPoint<DigitConnectPoint>("Y");
            zPoint = AddInPoint<DigitConnectPoint>("Z");
            AddOutPoint<Vector3ConnectPoint>("V", GetV);
            AddOutPoint<FloatConnectPoint>("X", GetX);
            AddOutPoint<FloatConnectPoint>("Y", GetY);
            AddOutPoint<FloatConnectPoint>("Z", GetZ);
            InPointAlignment = EConnectPointAlignment.Upper;
            OutPointAlignment = EConnectPointAlignment.Upper;
        }

        protected override void OnDraw(float zoom)
        {
            row = 0;
            //base.OnDraw();
            //LayoutLabel(title, CommonStyle.BoldTitle);
            GUI.Label(new Rect(rect.x + RowXOffset * zoom, rect.y + scaleRect.height / 2 - RowHeight * 1.5f * zoom, scaleRect.width - 20 * zoom, RowHeight * zoom), title, CommonStyle.BoldTitle);
            //row++;

            if (NodeType == ENodeType.Property)
            {
                Column("X", ref Vector.x);
                Column("Y", ref Vector.y);
                Column("Z", ref Vector.z);
            }
            else
            {
                // x
                if (xPoint.HasConnected)
                {
                    GUI.enabled = false;
                    RefreshValue(xPoint, ref Vector.x);
                }
                Column("X", ref Vector.x);
                if (xPoint.HasConnected)
                    GUI.enabled = true;

                // y
                if (yPoint.HasConnected)
                {
                    GUI.enabled = false;
                    RefreshValue(yPoint, ref Vector.y);
                }
                Column("Y", ref Vector.y);
                if (yPoint.HasConnected)
                    GUI.enabled = true;

                // z
                if (zPoint.HasConnected)
                {
                    GUI.enabled = false;
                    RefreshValue(zPoint, ref Vector.z);
                }
                Column("Z", ref Vector.z);
                if (zPoint.HasConnected)
                    GUI.enabled = true;
            }
        }

        int row = 0;

        private void Column(string label, ref float value)
        {
            float labelWidth = 15;
            float fieldWidth = 40;
            float colWidth = (labelWidth + fieldWidth + 5) * zoom;
            GUI.Label(new Rect(rect.x + RowXOffset * zoom + colWidth * row, rect.y + scaleRect.height / 2, labelWidth * zoom, RowHeight * zoom), label, CommonStyle.BoldTitle);
            value = EditorGUI.FloatField(new Rect(rect.x + RowXOffset * zoom + labelWidth * zoom + colWidth * row, rect.y + rect.height / 2, fieldWidth * zoom, RowHeight * zoom), value);
            row++;
        }

        public override void OnDrawDetail()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Property:", GUILayout.Width(70));
            title = EditorGUILayout.TextField(title, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Type:", GUILayout.Width(70));
            var tempType = (ENodeType)EditorGUILayout.EnumPopup(NodeType, GUILayout.Width(150));
            if(tempType != NodeType)
            {
                NodeType = tempType;
                if (NodeType == ENodeType.Property)
                {
                    ClearInPoints();
                    xPoint = null;
                    yPoint = null;
                    zPoint = null;
                }
                else
                {
                    xPoint = AddInPoint<DigitConnectPoint>("X");
                    yPoint = AddInPoint<DigitConnectPoint>("Y");
                    zPoint = AddInPoint<DigitConnectPoint>("Z");
                }
            }
            EditorGUILayout.EndHorizontal();

            if (NodeType == ENodeType.Property)
                DrawProperty();
            else
                DrawConstant();
        }

        private void DrawProperty()
        {
            EditorGUILayout.LabelField("Vector:", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.Space();
            // x connect detection
            EditorGUILayout.LabelField("X", GUILayout.Width(10));
            Vector.x = EditorGUILayout.FloatField(Vector.x, GUILayout.Width(50));

            // y connect detection
            EditorGUILayout.LabelField("Y", GUILayout.Width(10));
            Vector.y = EditorGUILayout.FloatField(Vector.y, GUILayout.Width(50));

            // z connect detection
            EditorGUILayout.LabelField("Z", GUILayout.Width(10));
            Vector.z = EditorGUILayout.FloatField(Vector.z, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConstant()
        {
            EditorGUILayout.LabelField("Vector:", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.Space();
            // x connect detection
            if (xPoint.HasConnected)
            {
                GUI.enabled = false;
                RefreshValue(xPoint, ref Vector.x);
            }
            EditorGUILayout.LabelField("X", GUILayout.Width(10));
            Vector.x = EditorGUILayout.FloatField(Vector.x, GUILayout.Width(50));
            if (xPoint.HasConnected)
                GUI.enabled = true;

            // y connect detection
            if (yPoint.HasConnected)
            {
                GUI.enabled = false;
                RefreshValue(yPoint, ref Vector.y);
            }
            EditorGUILayout.LabelField("Y", GUILayout.Width(10));
            Vector.y = EditorGUILayout.FloatField(Vector.y, GUILayout.Width(50));
            if (yPoint.HasConnected)
                GUI.enabled = true;

            // z connect detection
            if (zPoint.HasConnected)
            {
                GUI.enabled = false;
                RefreshValue(zPoint, ref Vector.z);
            }
            EditorGUILayout.LabelField("Z", GUILayout.Width(10));
            Vector.z = EditorGUILayout.FloatField(Vector.z, GUILayout.Width(50));
            if (zPoint.HasConnected)
                GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshValue(ConnectionPoint inPoint, ref float axis)
        {
            var value = inPoint.GetFunc.Invoke();
            axis = Convert.ToSingle(value);
        }

        #region -- Get methods --
        public object GetV()
        {
            return Vector;
        }

        public object GetX()
        {
            return Vector.x;
        }

        public object GetY()
        {
            return Vector.y;
        }

        public object GetZ()
        {
            return Vector.z;
        }
        #endregion

        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write((byte)NodeType);
            writer.Write(Vector.x);
            writer.Write(Vector.y);
            writer.Write(Vector.z);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            NodeType = (ENodeType)reader.ReadByte();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            Vector = new Vector3(x, y, z);
        }
        #endregion
    }
}