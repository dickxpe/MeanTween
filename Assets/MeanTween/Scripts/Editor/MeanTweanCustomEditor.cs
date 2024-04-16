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

                meanTween.selectedComponentId = EditorGUILayout.Popup("Component", meanTween.selectedComponentId, componentStrings);
                serializedObject.FindProperty("selectedComponentId").SetValue(meanTween.selectedComponentId);
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                Component component;

                List<string> publicFields = new List<string>();
                if (componentsLookup.TryGetValue(componentStrings[meanTween.selectedComponentId], out component))
                {
                    serializedObject.FindProperty("selectedComponent").SetValue(component);
                    meanTween.selectedComponent = component;
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
                    meanTween.selectedFieldId = EditorGUILayout.Popup("Property", meanTween.selectedFieldId, publicFields.ToArray());
                    serializedObject.FindProperty("selectedFieldId").SetValue(meanTween.selectedFieldId);
                    FieldInfo fieldInfoOut;
                    PropertyInfo propertyInfoOut;
                    SerializedProperty fromProp = serializedObject.FindProperty("from");
                    if (fieldsLookup.TryGetValue(publicFields[meanTween.selectedFieldId], out fieldInfoOut))
                    {
                        serializedObject.FindProperty("fromCheck").SetValue(true);
                        serializedObject.FindProperty("selectedFieldName").SetValue(fieldInfoOut.Name);

                        if (fieldInfoOut.FieldType == typeof(float))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
                            fromProp.SetValue(new Vector3((float)fieldInfoOut.GetValue(meanTween.selectedComponent), 0, 0));
                        }
                        else if (fieldInfoOut.FieldType == typeof(Vector3))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                            fromProp.SetValue((Vector3)fieldInfoOut.GetValue(meanTween.selectedComponent));
                        }
                        else if (fieldInfoOut.FieldType == typeof(Vector2))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"));
                            fromProp.SetValue((Vector2)fieldInfoOut.GetValue(meanTween.selectedComponent));
                        }
                    }
                    else if (propertiesLookup.TryGetValue(publicFields[meanTween.selectedFieldId], out propertyInfoOut))
                    {
                        serializedObject.FindProperty("fromCheck").SetValue(true);
                        serializedObject.FindProperty("selectedFieldName").SetValue(propertyInfoOut.Name);
                        if (propertyInfoOut.PropertyType == typeof(float))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
                            fromProp.SetValue(new Vector3((float)propertyInfoOut.GetValue(meanTween.selectedComponent), 0, 0));
                        }
                        else if (propertyInfoOut.PropertyType == typeof(Vector3))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                            fromProp.SetValue((Vector3)propertyInfoOut.GetValue(meanTween.selectedComponent));
                        }
                        else if (propertyInfoOut.PropertyType == typeof(Vector2))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"));
                            fromProp.SetValue((Vector2)propertyInfoOut.GetValue(meanTween.selectedComponent));
                        }
                    }
                }
                else
                {
                    GUI.enabled = false;
                    meanTween.selectedFieldId = EditorGUILayout.Popup("Property", 0, new string[] { "Component has no public fields or properties" });
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
                if (meanTween.loopType != MeanBehaviour.LOOPTYPE.Once && !meanTween.infiniteLoop)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onLoopsComplete"));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                changeUpdate = true;
                // EditorUtility.SetDirty(target);
            }
            else
            {
                changeUpdate = false;
            }

            serializedObject.ApplyModifiedProperties();


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