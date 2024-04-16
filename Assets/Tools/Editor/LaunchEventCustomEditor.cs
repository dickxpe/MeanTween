// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UltEvents.Editor;

namespace com.zebugames.meantween.unity
{

    [CustomEditor(typeof(LaunchEvent))]
    [InitializeOnLoad]
    public class LaunchEventCustomEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LaunchEvent launchEvent = (LaunchEvent)target;
            if (EditorApplication.isPlaying)

            {
                GUI.color = Color.cyan;
                if (GUILayout.Button("LAUNCH", EditorStyles.miniButton))
                {
                    launchEvent.Launch();
                }
                GUI.color = default;
            }
        }
    }
}