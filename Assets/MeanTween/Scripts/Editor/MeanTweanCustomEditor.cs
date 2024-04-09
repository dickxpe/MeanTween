// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.zebugames.meantween.unity
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeanTween))]
    [InitializeOnLoad]
    public class MeanTweenCustomEditor : Editor
    {

        Dictionary<string, Component> componentsLookup = new Dictionary<string, Component>();
        Dictionary<string, FieldInfo> fieldsLookup = new Dictionary<string, FieldInfo>();

        Dictionary<string, PropertyInfo> propertiesLookup = new Dictionary<string, PropertyInfo>();
        bool changeUpdate = false;

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            componentsLookup.Clear();
            MeanTween meanTween = (MeanTween)target;
            Color defaultColor = GUI.color;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToTween"));

            EditorGUI.BeginChangeCheck();
            SerializedProperty tweenTypeProp = serializedObject.FindProperty("tweenType");
            GUI.color = Color.cyan;
            EditorGUILayout.PropertyField(tweenTypeProp);
            GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("easeType"));
            GUI.color = defaultColor;
            if (meanTween.tweenType == MeanTween.TWEENTYPE.SpriteColor)
            {
                if (changeUpdate)
                {
                    SpriteRenderer ren = meanTween.objectToTween.GetComponent<SpriteRenderer>();
                    if (ren == null)
                    {
                        Debug.LogWarning("MeanTween Sprite Color: No SpriteRenderer on Gameobject " + meanTween.objectToTween.name);
                    }
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("color"));
            }
            else if (meanTween.tweenType == MeanTween.TWEENTYPE.SpriteAlpha)
            {
                if (changeUpdate)
                {
                    SpriteRenderer ren = meanTween.objectToTween.GetComponent<SpriteRenderer>();
                    if (ren == null)
                    {
                        Debug.LogWarning("MeanTween Sprite Alpha: No SpriteRenderer on Gameobject " + meanTween.objectToTween.name);
                    }
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("alpha"));
            }
            else if (meanTween.tweenType == MeanTween.TWEENTYPE.ComponentFieldValue)
            {
                List<Component> components = meanTween.objectToTween.GetComponents<Component>().Where(x => x.GetType() != typeof(MeanTween)).ToList();

                string[] componentStrings = Array.ConvertAll(components.ToArray(), x => x.ToString());

                List<string> duplicates = new List<string>();

                for (int i = componentStrings.Length - 1; i >= 0; i--)
                {
                    int count = componentStrings.Where(x => x.Equals(componentStrings[i])).Count();
                    if (count > 1)
                    {
                        if (!duplicates.Contains(componentStrings[i]))
                        {
                            duplicates.Add(componentStrings[i]);
                        }
                    }
                    if (duplicates.Contains(componentStrings[i]))
                    {
                        componentStrings[i] = componentStrings[i] + " " + count;


                    }
                    componentsLookup.Add(componentStrings[i], components[i]);
                }

                meanTween.selectedComponent = EditorGUILayout.Popup("Component", meanTween.selectedComponent, componentStrings);

                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                Component component;

                List<string> publicFields = new List<string>();
                if (componentsLookup.TryGetValue(componentStrings[meanTween.selectedComponent], out component))
                {
                    meanTween.component = component;
                    FieldInfo[] fields = component.GetType().GetFields(flags);
                    foreach (FieldInfo fieldInfo in fields)
                    {
                        if (fieldInfo.FieldType == typeof(float))
                        {
                            publicFields.Add(fieldInfo.Name + " (float)");
                            if (!fieldsLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                fieldsLookup.Add(publicFields[publicFields.Count - 1], fieldInfo);
                            }
                        }
                        else if (fieldInfo.FieldType == typeof(Vector3))
                        {
                            publicFields.Add(fieldInfo.Name + " (Vector3)");
                            if (!fieldsLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                fieldsLookup.Add(publicFields[publicFields.Count - 1], fieldInfo);
                            }
                        }
                        else if (fieldInfo.FieldType == typeof(Vector2))
                        {
                            publicFields.Add(fieldInfo.Name + " (Vector2)");
                            if (!fieldsLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                fieldsLookup.Add(publicFields[publicFields.Count - 1], fieldInfo);
                            }
                        }
                    }
                    PropertyInfo[] properties = component.GetType().GetProperties(flags | BindingFlags.SetProperty);
                    foreach (PropertyInfo propInfo in properties)
                    {
                        if (propInfo.PropertyType == typeof(float))
                        {
                            publicFields.Add(propInfo.Name + " (float)");
                            if (!propertiesLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                propertiesLookup.Add(publicFields[publicFields.Count - 1], propInfo);
                            }
                        }
                        else if (propInfo.PropertyType == typeof(Vector3))
                        {
                            publicFields.Add(propInfo.Name + " (Vector3)");
                            if (!propertiesLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                propertiesLookup.Add(publicFields[publicFields.Count - 1], propInfo);
                            }
                        }

                        else if (propInfo.PropertyType == typeof(Vector2))
                        {
                            publicFields.Add(propInfo.Name + " (Vector2)");
                            if (!propertiesLookup.ContainsKey(publicFields[publicFields.Count - 1]))
                            {
                                propertiesLookup.Add(publicFields[publicFields.Count - 1], propInfo);
                            }
                        }
                    }
                }

                if (publicFields.Count > 0)
                {
                    meanTween.selectedField = EditorGUILayout.Popup("Property", meanTween.selectedField, publicFields.ToArray());

                    FieldInfo fieldInfoOut;
                    PropertyInfo propertyInfoOut;

                    if (fieldsLookup.TryGetValue(publicFields[meanTween.selectedField], out fieldInfoOut))
                    {
                        meanTween.fieldInfo = fieldInfoOut;

                        if (fieldInfoOut.FieldType == typeof(float))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
                        }
                        else if (fieldInfoOut.FieldType == typeof(Vector3))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                        }
                        else if (fieldInfoOut.FieldType == typeof(Vector2))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"));
                        }
                    }
                    else if (propertiesLookup.TryGetValue(publicFields[meanTween.selectedField], out propertyInfoOut))
                    {
                        meanTween.propertyInfo = propertyInfoOut;

                        if (propertyInfoOut.PropertyType == typeof(float))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
                        }
                        else if (propertyInfoOut.PropertyType == typeof(Vector3))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                        }
                        else if (propertyInfoOut.PropertyType == typeof(Vector2))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"));
                        }
                    }
                }
                else
                {
                    GUI.enabled = false;
                    meanTween.selectedField = EditorGUILayout.Popup("Property", 0, new string[] { "Component has no public fields or properties" });
                    GUI.enabled = true;
                }

            }
            else
            {
                bool rotateAround = false;
                SerializedProperty additivieProp = serializedObject.FindProperty("additive");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("space"));
                if (meanTween.tweenType == MeanTween.TWEENTYPE.Rotate)
                {
                    SerializedProperty rotateAroundProp = serializedObject.FindProperty("rotateAroundAxis");
                    rotateAround = meanTween.rotateAroundAxis;
                    EditorGUILayout.PropertyField(rotateAroundProp);
                    if (rotateAround)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("axis"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("degrees"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(additivieProp);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(additivieProp);
                }

                bool spline = false;
                //TODO: Implement splines
                /*
                if (tweenType == MeanTween.TWEENTYPE.Move)
                {
                    SerializedProperty splineProp = serializedObject.FindProperty("spline");
                    spline = splineProp.GetValue<bool>();
                    EditorGUILayout.PropertyField(splineProp);
                }
                */
                if (!spline)
                {
                    if (!rotateAround)
                    {
                        if (meanTween.additive)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"), new GUIContent("Addition"));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("splinePositions"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnAwake"));
            SerializedProperty loopProp = serializedObject.FindProperty("loopType");
            EditorGUILayout.PropertyField(loopProp);
            if (meanTween.loopType != MeanTween.LOOPTYPE.Once)
            {
                SerializedProperty infiniteProp = serializedObject.FindProperty("infiniteLoop");
                EditorGUILayout.PropertyField(infiniteProp);

                if (!meanTween.infiniteLoop)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("loops"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreTimeScale"));

            meanTween.showEvents = EditorGUILayout.Foldout(meanTween.showEvents, "Events");
            if (meanTween.showEvents)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onUpdate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onLoopsComplete"));
            }
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                changeUpdate = true;
            }
            else
            {
                changeUpdate = false;
            }
            if (EditorApplication.isPlaying)
            {
                GUI.color = Color.cyan;
                if (GUILayout.Button("Play Tween", EditorStyles.miniButton))
                {
                    meanTween.CancelAll();
                    meanTween.Animate();
                }
                GUI.color = default;
            }
        }
    }
}