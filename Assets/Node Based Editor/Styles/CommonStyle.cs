using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NodeEditor.Style
{
    public class CommonStyle
    {
        private static GUIStyle boldTitle;
        public static GUIStyle BoldTitle
        {
            get
            {
                if (boldTitle == null)
                {
                    boldTitle = new GUIStyle();
                    boldTitle.alignment = TextAnchor.MiddleCenter;
                    boldTitle.fontStyle = FontStyle.Bold;
                    boldTitle.normal.textColor = Color.white;
                }
                return boldTitle;
            }
        }

        private static GUIStyle detailTitle;
        public static GUIStyle DetailTitle
        {
            get
            {
                if (detailTitle == null)
                {
                    detailTitle = new GUIStyle();
                    detailTitle.alignment = TextAnchor.MiddleLeft;
                    detailTitle.fontStyle = FontStyle.Bold;
                }
                return detailTitle;
            }
        }

        private static GUIStyle inPointTitle;
        public static GUIStyle InPointTitle
        {
            get
            {
                if (inPointTitle == null)
                {
                    inPointTitle = new GUIStyle();
                    inPointTitle.alignment = TextAnchor.MiddleRight;
                }
                return inPointTitle;
            }
        }

        private static GUIStyle outPointTitle;
        public static GUIStyle OutPointTitle
        {
            get
            {
                if (outPointTitle == null)
                {
                    outPointTitle = new GUIStyle();
                    outPointTitle.alignment = TextAnchor.MiddleLeft;
                }
                return outPointTitle;
            }
        }

        private static GUIStyle blueNode;

        public static GUIStyle BlueNode
        {
            get
            {
                if (blueNode == null)
                {
                    blueNode = new GUIStyle();
                    blueNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                    blueNode.border = new RectOffset(12, 12, 12, 12);
                }
                return blueNode;
            }
        }

        private static GUIStyle selectedBlueNode;

        public static GUIStyle SelectedBlueNode
        {
            get
            {
                if (selectedBlueNode == null)
                {
                    selectedBlueNode = new GUIStyle();
                    selectedBlueNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
                    selectedBlueNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedBlueNode;
            }
        }

        private static GUIStyle greenNode;

        public static GUIStyle GreenNode
        {
            get
            {
                if (greenNode == null)
                {
                    greenNode = new GUIStyle();
                    greenNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
                    greenNode.border = new RectOffset(12, 12, 12, 12);
                }
                return greenNode;
            }
        }

        private static GUIStyle selectedGreenNode;

        public static GUIStyle SelectedGreenNode
        {
            get
            {
                if (selectedGreenNode == null)
                {
                    selectedGreenNode = new GUIStyle();
                    selectedGreenNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
                    selectedGreenNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedGreenNode;
            }
        }

        private static GUIStyle yellowNode;

        public static GUIStyle YellowNode
        {
            get
            {
                if (yellowNode == null)
                {
                    yellowNode = new GUIStyle();
                    yellowNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
                    yellowNode.border = new RectOffset(12, 12, 12, 12);
                }
                return yellowNode;
            }
        }

        private static GUIStyle selectedYellowNode;

        public static GUIStyle SelectedYellowNode
        {
            get
            {
                if (selectedYellowNode == null)
                {
                    selectedYellowNode = new GUIStyle();
                    selectedYellowNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4 on.png") as Texture2D;
                    selectedYellowNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedYellowNode;
            }
        }

        private static GUIStyle orangeNode;

        public static GUIStyle OrangeNode
        {
            get
            {
                if (orangeNode == null)
                {
                    orangeNode = new GUIStyle();
                    orangeNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                    orangeNode.border = new RectOffset(12, 12, 12, 12);
                }
                return orangeNode;
            }
        }

        private static GUIStyle selectedOrangeNode;

        public static GUIStyle SelectedOrangeNode
        {
            get
            {
                if (selectedOrangeNode == null)
                {
                    selectedOrangeNode = new GUIStyle();
                    selectedOrangeNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5 on.png") as Texture2D;
                    selectedOrangeNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedOrangeNode;
            }
        }

        private static GUIStyle redNode;

        public static GUIStyle RedNode
        {
            get
            {
                if (redNode == null)
                {
                    redNode = new GUIStyle();
                    redNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
                    redNode.border = new RectOffset(12, 12, 12, 12);
                }
                return redNode;
            }
        }

        private static GUIStyle selectedRedNode;

        public static GUIStyle SelectedRedNode
        {
            get
            {
                if (selectedRedNode == null)
                {
                    selectedRedNode = new GUIStyle();
                    selectedRedNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
                    selectedRedNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedRedNode;
            }
        }

        private static GUIStyle cyanNode;

        public static GUIStyle CyanNode
        {
            get
            {
                if (cyanNode == null)
                {
                    cyanNode = new GUIStyle();
                    cyanNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
                    cyanNode.border = new RectOffset(12, 12, 12, 12);
                }
                return cyanNode;
            }
        }

        private static GUIStyle selectedCyanNode;

        public static GUIStyle SelectedCyanNode
        {
            get
            {
                if (selectedCyanNode == null)
                {
                    selectedCyanNode = new GUIStyle();
                    selectedCyanNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D;
                    selectedCyanNode.border = new RectOffset(12, 12, 12, 12);
                }
                return selectedCyanNode;
            }
        }

        private static GUIStyle selectBoxStyle;
        public static GUIStyle SelectBoxStyle
        {
            get
            {
                if (selectBoxStyle == null)
                {
                    Texture2D tex = new Texture2D(2, 2);
                    for (int x = 0; x < tex.width; x++)
                    {
                        for (int y = 0; y < tex.height; y++)
                        {
                            tex.SetPixel(x, y, new Color(0.4f, 0.4f, 1f, 0.3f));
                        }
                    }
                    tex.Apply();
                    selectBoxStyle = new GUIStyle(GUI.skin.box);
                    selectBoxStyle.normal.background = tex;
                }
                return selectBoxStyle;
            }
        }
    }
}