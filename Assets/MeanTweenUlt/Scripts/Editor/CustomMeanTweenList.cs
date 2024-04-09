// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.zebugames.meantween.ult
{

    public class CustomMeanTweenList : ReorderableList
    {
        Rect infinityRect = new Rect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public CustomMeanTweenList(IList elements, Type elementType) : base(elements, elementType) { }
        public CustomMeanTweenList(IList elements, Type elementType, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(elements, elementType, draggable, displayHeader, displayAddButton, displayRemoveButton) { }
        public CustomMeanTweenList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements) { }
        public CustomMeanTweenList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton) { }

        public void DoLayoutList(Rect parent)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo methodInfo;
            object[] parameters = new object[0];

            float h = EditorGUIUtility.singleLineHeight * 2;
            if (count > 1)
            {
                h += (EditorGUIUtility.singleLineHeight + 2) * (count - 1);
            }

            Rect rect = GUILayoutUtility.GetRect(0f, 0, GUILayout.ExpandWidth(expand: true));
            rect.y = parent.y;
            rect.x = parent.x + 170;
            rect.height = 0;
            rect.width = parent.width - 170;

            Rect rect2 = GUILayoutUtility.GetRect(10f, 0, GUILayout.ExpandWidth(expand: true));
            rect2.y = parent.y + 2;
            rect2.x = parent.x + 170;
            rect2.height = h - 5;
            rect2.width = parent.width - 170;

            Rect rect3 = GUILayoutUtility.GetRect(4f, 0, GUILayout.ExpandWidth(expand: true));
            rect3.y = parent.y + footerHeight + h - 5 - EditorGUIUtility.singleLineHeight;
            rect3.x = parent.x + parent.width - 250; ;
            rect3.width = 250;

            GUILayout.BeginVertical();
            methodInfo = base.GetType().BaseType.GetMethod("DoListHeader", flags);
            parameters = new object[] { rect };
            methodInfo.Invoke(this, parameters);
            methodInfo = base.GetType().BaseType.GetMethod("DoListElements", flags);
            parameters = new object[] { rect2, infinityRect };
            methodInfo.Invoke(this, parameters);
            methodInfo = base.GetType().BaseType.GetMethod("DoListFooter", flags);
            parameters = new object[] { rect3 };
            methodInfo.Invoke(this, parameters);
            GUILayout.EndVertical();
        }

    }




}
