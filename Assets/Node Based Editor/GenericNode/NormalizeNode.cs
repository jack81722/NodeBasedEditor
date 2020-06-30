using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class NormalizeNode : Node
    {
        private ConnectionPoint inPoint;
        private Type valueType = typeof(Vector2);
        private object v = Vector2.zero;
        private object normalized;

        public NormalizeNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Normalize";
            ShowTitle = false;
            inPoint = AddInPoint<DigitConnectPoint>("v");
            inPoint.OnConnected += OnConnectInPoint;
            AddOutPoint<DigitConnectPoint>("normalized", () => normalized);
        }

        public NormalizeNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Normalize";
            ShowTitle = false;
            inPoint = AddInPoint<DigitConnectPoint>("v");
            inPoint.OnConnected += OnConnectInPoint;
            AddOutPoint<DigitConnectPoint>("normalized", () => normalized);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();
            Row(inPoint, ref v);

            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("normalized:", GUILayout.Width(50));
            object nor = DigitConvert.Normalize(v, valueType);
            RowField(valueType, ref nor);
            normalized = nor;
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void OnConnectInPoint(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            valueType = UpdateValueType(inPoint);
        }

        private Type UpdateValueType(params ConnectionPoint[] points)
        {
            int max = 0;
            foreach (var p in points)
            {
                if (p.HasConnected && p.Value != null)
                {
                    int weight = calWeight(p.Value.GetType());
                    if (max < weight)
                        max = weight;
                }
            }
            return getType(max);

            int calWeight(Type type)
            {
                if (type == DigitConvert.INT)
                    return 0;
                else if (type == DigitConvert.FLOAT)
                    return 1;
                else if (type == DigitConvert.V2)
                    return 2;
                else if (type == DigitConvert.V3)
                    return 3;
                else
                    return 4;
            }

            Type getType(int weight)
            {
                Type type;
                switch (weight)
                {
                    case 4:
                        type = DigitConvert.V4;
                        break;
                    case 3:
                        type = DigitConvert.V3;
                        break;
                    case 2:
                        type = DigitConvert.V2;
                        break;
                    default:
                        type = DigitConvert.V2;
                        break;
                }
                return type;
            }
        }

        private void Row(ConnectionPoint inPoint, ref object value)
        {
            if (inPoint.HasConnected)
            {
                GUI.enabled = false;
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                EditorGUILayout.LabelField(inPoint.title, GUILayout.Width(10));
                RowField(valueType, inPoint, ref value);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                EditorGUILayout.LabelField(inPoint.title, GUILayout.Width(10));
                RowField(valueType, ref value);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RowField(Type type, ref object value)
        {
            if (type == DigitConvert.INT)
            {
                value = EditorGUILayout.IntField(value.CastToInt(), GUILayout.Width(50));
            }
            else if (type == DigitConvert.FLOAT)
            {
                value = EditorGUILayout.FloatField(value.CastToFloat(), GUILayout.Width(50));
            }
            else if (type == DigitConvert.V2)
            {
                value = EditorGUILayout.Vector2Field("", value.CastToVector2(), GUILayout.Width(180));
            }
            else if (type == DigitConvert.V3)
            {
                value = EditorGUILayout.Vector3Field("", value.CastToVector3(), GUILayout.Width(180));
            }
            else
            {
                value = EditorGUILayout.Vector4Field("", value.CastToVector4(), GUILayout.Width(180));
            }

        }

        private void RowField(Type type, ConnectionPoint inPoint, ref object value)
        {
            var temp = inPoint.Value;
            if (type == DigitConvert.INT)
            {
                value = EditorGUILayout.IntField(Convert.ToInt32(temp), GUILayout.Width(50));
            }
            else if (type == DigitConvert.FLOAT)
            {
                value = EditorGUILayout.FloatField(Convert.ToSingle(temp), GUILayout.Width(50));
            }
            else if (type == DigitConvert.V2)
            {
                value = EditorGUILayout.Vector2Field("", temp.CastToVector2(), GUILayout.Width(200));
            }
            else if (type == DigitConvert.V3)
            {
                value = EditorGUILayout.Vector3Field("", temp.CastToVector3(), GUILayout.Width(200));
            }
            else
            {
                value = EditorGUILayout.Vector4Field("", temp.CastToVector4(), GUILayout.Width(200));
            }

        }
    }
}