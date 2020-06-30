using NodeEditor.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class FloatNode : Node
    {
        public float Value { get; protected set; }
        public float Min { get; protected set; }
        public float Max { get; protected set; }
        

        #region -- Constructors --
        public FloatNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle) 
        {
            title = "Float";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("Float", () => Value);
        }

        public FloatNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Float";
            ShowTitle = false;
            AddOutPoint<FloatConnectPoint>("Float", () => Value);
        }
        #endregion

        protected override void OnDraw(float zoom = 1)
        {   
            LayoutLabel(title, CommonStyle.BoldTitle);
            if (Max > Min)
            {
                FieldHeight += RowHeight;
                float width = rect.width - 20;
                Min = EditorGUI.FloatField(new Rect(rect.x + RowXOffset, rect.y + RowYOffset + FieldCount * RowHeight, 20, RowHeight), Min);
                Value = GUI.HorizontalSlider(new Rect(rect.x + RowXOffset + 25, rect.y + RowYOffset + FieldCount * RowHeight, width - 70, RowHeight), Value, Min, Max);
                Value = EditorGUI.FloatField(new Rect(rect.x + RowXOffset + width - 40, rect.y + RowYOffset + FieldCount * RowHeight, 20, RowHeight), Value);
                Max = EditorGUI.FloatField(new Rect(rect.x + RowXOffset + width - 20, rect.y + RowYOffset + FieldCount * RowHeight, 20, RowHeight), Max);
                FieldCount++;
                //Value = LayoutSlider(Value, Min, Max);
            }
            else
            {
                Value = LayoutFloatField(Value);
            }
        }

        protected override void AddGenericItem(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        }

        public override void OnDrawDetail()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Property:", GUILayout.Width(70));
            title = EditorGUILayout.TextField(title, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Type:", GUILayout.Width(70));
            var tempType = (ENodeType) EditorGUILayout.EnumPopup(NodeType, GUILayout.Width(150));
            if (tempType != ENodeType.Function)
                NodeType = tempType;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Value:", GUILayout.Width(70));
            Value = EditorGUILayout.FloatField(Value, GUILayout.Width(150));
            if(Max > Min)
            {
                Value = Mathf.Clamp(Value, Min, Max);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Min:", GUILayout.Width(40));
            Min = EditorGUILayout.FloatField(Min, GUILayout.Width(60));
            EditorGUILayout.LabelField("Max:", GUILayout.Width(40));
            Max = EditorGUILayout.FloatField(Max, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
        }


        #region -- Save/Load methods --
        public override void BinarySave(BinaryWriter writer)
        {
            base.BinarySave(writer);
            writer.Write((byte)NodeType);
            writer.Write(Value);
        }

        public override void BinaryLoad(BinaryReader reader)
        {
            base.BinaryLoad(reader);
            NodeType = (ENodeType)reader.ReadByte();
            Value = reader.ReadSingle();
        }
        #endregion
    }
}