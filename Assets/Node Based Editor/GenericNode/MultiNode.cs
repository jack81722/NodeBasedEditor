using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class MultiNode : Node
    {
        public Type ValueType;
        public object Value = 0;
        private List<object> inValues = new List<object>();
        private List<ConnectionPoint> InPoints = new List<ConnectionPoint>();

        public MultiNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Multiple";
            ShowTitle = false;
            InPointAlignment = EConnectPointAlignment.Upper;
            OutPointAlignment = EConnectPointAlignment.Upper;
            DynamicAddInPoint();
            DynamicAddInPoint();
            UpdateOutPoints(typeof(int));
        }

        public MultiNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Multiple";
            ShowTitle = false;
            InPointAlignment = EConnectPointAlignment.Upper;
            OutPointAlignment = EConnectPointAlignment.Upper;
            DynamicAddInPoint();
            DynamicAddInPoint();
            UpdateOutPoints(typeof(int));
        }

        protected override void OnDraw(float zoom)
        {
            base.OnDraw(zoom);
        }

        public override void OnDrawDetail()
        {
            base.OnDrawDetail();
            for (int i = 0; i < inPoints.Count; i++)
            {
                var point = inPoints[i];
                Row(point, i);
            }

            RefreshTotal();
            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Total", GUILayout.Width(30));
            RowField(ValueType, ref Value);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        int inCount = 0;

        private void DynamicAddInPoint()
        {
            char title = (char)('A' + inCount++);
            ConnectionPoint inPoint;
            if(inCount - 1 > 0)
                inPoint = AddInPoint<FloatConnectPoint>(title.ToString());
            else
                inPoint = AddInPoint<DigitConnectPoint>(title.ToString());
            InPoints.Add(inPoint);
            inPoint.OnConnected += OnInPointConnected;
            inPoint.OnDisconnected += OnInPointDisconnected;

            inValues.Add(0);
            ModifyConnectionPointHeight();
        }

        private void DynamicRemoveInPoint()
        {
            inPoints.RemoveAt(inPoints.Count - 1);
            inValues.RemoveAt(inValues.Count - 1);
            inCount--;
        }

        private void OnInPointConnected(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            if (inPoints[inPoints.Count - 1] == inPoint)
            {
                DynamicAddInPoint();
            }
            var newValueType = UpdateValueType(inPoints);
            UpdateOutPoints(newValueType);
            RefreshTotal();
            ModifyConnectionPointHeight();
        }

        private void OnInPointDisconnected(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            while (inPoints.Count > 2 && !inPoints[inPoints.Count - 2].HasConnected)
            {
                DynamicRemoveInPoint();
            }
            var newValueType = UpdateValueType(inPoints);
            UpdateOutPoints(newValueType);
            RefreshTotal();
            ModifyConnectionPointHeight();
        }

        private void RefreshTotal()
        {
            Value = 0;
            if (inPoints.Count > 0 && inPoints[0].HasConnected)
            {
                Value = DigitConvert.CastTo(inPoints[0].Value, ValueType);
            }
            else
            {
                Value = DigitConvert.CastTo(inValues[0], ValueType);
            }

            for (int i = 1; i < inPoints.Count; i++)
            {   
                var point = inPoints[i];
                if (point.HasConnected)
                {
                    Value = DigitConvert.Multi(Value, point.Value, ValueType);
                }
                else
                {
                    Value = DigitConvert.Multi(Value, inValues[i], ValueType);
                }
            }
        }

        private Type UpdateValueType(List<ConnectionPoint> points)
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
                    case 1:
                        type = DigitConvert.FLOAT;
                        break;
                    default:
                        type = DigitConvert.INT;
                        break;
                }
                return type;
            }
        }

        private void UpdateOutPoints(Type newValueType)
        {
            if (ValueType != newValueType)
            {
                ClearOutPoints();
                if (newValueType == DigitConvert.V4)
                {
                    ValueType = DigitConvert.V4;
                    AddOutPoint<DigitConnectPoint>("Vector4", () => Value.CastTo(ValueType));
                    AddOutPoint<DigitConnectPoint>("X", () => Value.CastToVector4().x);
                    AddOutPoint<DigitConnectPoint>("Y", () => Value.CastToVector4().y);
                    AddOutPoint<DigitConnectPoint>("Z", () => Value.CastToVector4().z);
                    AddOutPoint<DigitConnectPoint>("W", () => Value.CastToVector4().w);
                }
                else if (newValueType == DigitConvert.V3)
                {
                    ValueType = DigitConvert.V3;
                    AddOutPoint<DigitConnectPoint>("Vector3", () => Value.CastTo(ValueType));
                    AddOutPoint<DigitConnectPoint>("X", () => Value.CastToVector3().x);
                    AddOutPoint<DigitConnectPoint>("Y", () => Value.CastToVector3().y);
                    AddOutPoint<DigitConnectPoint>("Z", () => Value.CastToVector3().z);
                }
                else if (newValueType == DigitConvert.V2)
                {
                    ValueType = DigitConvert.V2;
                    AddOutPoint<DigitConnectPoint>("Vector2", () => Value.CastTo(ValueType));
                    AddOutPoint<DigitConnectPoint>("X", () => Value.CastToVector2().x);
                    AddOutPoint<DigitConnectPoint>("Y", () => Value.CastToVector2().y);
                }
                else if (newValueType == DigitConvert.FLOAT)
                {
                    ValueType = DigitConvert.FLOAT;
                    AddOutPoint<DigitConnectPoint>("Float", () => Value.CastTo(ValueType));
                }
                else
                {
                    ValueType = DigitConvert.INT;
                    AddOutPoint<DigitConnectPoint>("Integer", () => Value.CastTo(ValueType));
                }
            }
        }

        private void Row(ConnectionPoint inPoint, int index)
        {
            if (inPoint.HasConnected)
            {
                GUI.enabled = false;
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                EditorGUILayout.LabelField(inPoint.title, GUILayout.Width(10));
                RowField(ValueType, inPoint);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                EditorGUILayout.LabelField(inPoint.title, GUILayout.Width(10));
                var obj = inValues[index];
                RowField(typeof(float), ref obj);
                inValues[index] = obj;
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
                value = EditorGUILayout.Vector2Field("", value.CastToVector2(), GUILayout.Width(200));
            }
            else if (type == DigitConvert.V3)
            {
                value = EditorGUILayout.Vector3Field("", value.CastToVector3(), GUILayout.Width(200));
            }
            else
            {
                value = EditorGUILayout.Vector4Field("", value.CastToVector4(), GUILayout.Width(200));
            }

        }

        private void RowField(Type type, ConnectionPoint inPoint)
        {
            var value = inPoint.Value;
            if (type == DigitConvert.INT)
            {
                EditorGUILayout.IntField((int)value, GUILayout.Width(50));
            }
            else if (type == DigitConvert.FLOAT)
            {
                EditorGUILayout.FloatField(Convert.ToSingle(value), GUILayout.Width(50));
            }
            else if (type == DigitConvert.V2)
            {
                EditorGUILayout.Vector2Field("", value.CastToVector2(), GUILayout.Width(200));
            }
            else if (type == DigitConvert.V3)
            {
                EditorGUILayout.Vector3Field("", value.CastToVector3(), GUILayout.Width(200));
            }
            else
            {
                EditorGUILayout.Vector4Field("", value.CastToVector4(), GUILayout.Width(200));
            }
        }
    }
}