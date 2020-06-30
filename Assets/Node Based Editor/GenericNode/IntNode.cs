using NodeEditor.Style;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Generic
{
    public class IntNode : Node
    {
        public int Value { get; protected set; }

        public IntNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Integer";
            ShowTitle = false;
            AddOutPoint<IntConnectPoint>("Integer", () => Value);
        }

        public IntNode(int id, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle) : base(id, position, nodeStyle, selectedStyle, inPointStyle, outPointStyle)
        {
            title = "Integer";
            ShowTitle = false;
            AddOutPoint<IntConnectPoint>("Integer", () => Value);
        }

        protected override void OnDraw(float zoom = 1)
        {            
            LayoutLabel(title, CommonStyle.BoldTitle);
            Value = LayoutIntField(Value);
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
            if (tempType != ENodeType.Function)
                NodeType = tempType;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            EditorGUILayout.LabelField("Value:", GUILayout.Width(70));
            Value = EditorGUILayout.IntField(Value, GUILayout.Width(150));
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
            Value = reader.ReadInt32();
        }
        #endregion
    }
}