// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UltEvents.Editor;
using Unity.Mathematics;

namespace com.zebugames.meantween.ult
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

        void OnSceneGUI()
        {
            MeanTween meanTween = (MeanTween)target;

            var lookRotation = Quaternion.LookRotation(Camera.current.transform.forward);
            float radius = 0.25f;

            if (meanTween.path)
            {
                Handles.color = new Color(0, 1, 0, 0.5f);
                if (meanTween.space == MeanBehaviour.SPACE.Local && meanTween.transform.parent != null)
                {
                    Vector3 localStartPoint = meanTween.transform.parent.TransformPoint(meanTween.startPoint);
                    Vector3 localStartControl = meanTween.transform.parent.TransformPoint(meanTween.startControlPoint);

                    Handles.DrawSolidDisc(localStartPoint, Camera.current.transform.forward, radius);
                    Handles.DrawSolidDisc(localStartControl, Camera.current.transform.forward, radius);

                    meanTween.startPoint = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localStartPoint, quaternion.identity));
                    meanTween.startControlPoint = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localStartControl, quaternion.identity));

                    Handles.color = new Color(1, 0, 1, 0.5f);
                    for (int i = 0; i < meanTween.pathPoints.Count; i++)
                    {
                        Vector3 localPoint = meanTween.transform.parent.TransformPoint(meanTween.pathPoints[i].point);
                        Vector3 localControl1 = meanTween.transform.parent.TransformPoint(meanTween.pathPoints[i].control1);
                        Vector3 localControl2 = meanTween.transform.parent.TransformPoint(meanTween.pathPoints[i].control2);

                        Vector3 newPos = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localPoint, quaternion.identity));
                        Vector3 newPos2 = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localControl1, quaternion.identity));
                        Vector3 newPos3 = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localControl2, quaternion.identity));

                        MeanBehaviour.BezierPoint bezierPoint1 = meanTween.pathPoints[i];
                        bezierPoint1.point = newPos;
                        bezierPoint1.control1 = newPos2;
                        bezierPoint1.control2 = newPos3;
                        meanTween.pathPoints[i] = bezierPoint1;

                        Handles.DrawLine(localPoint, localControl1);
                        Handles.DrawLine(localPoint, localControl2);

                        Handles.DrawSolidDisc(localPoint, Camera.current.transform.forward, radius);
                        Handles.DrawSolidDisc(localControl1, Camera.current.transform.forward, radius);
                        Handles.DrawSolidDisc(localControl2, Camera.current.transform.forward, radius);
                    }
                    Vector3 localEndPoint = meanTween.transform.parent.TransformPoint(meanTween.endPoint);
                    Vector3 localEndControl = meanTween.transform.parent.TransformPoint(meanTween.endControlPoint);

                    Handles.color = new Color(0, 1, 0, 0.5f);
                    Handles.DrawLine(localStartControl, localStartPoint);
                    Handles.color = new Color(1, 0, 0, 0.5f);
                    Handles.DrawLine(localEndControl, localEndPoint);
                    Handles.DrawSolidDisc(localEndPoint, Camera.current.transform.forward, radius);
                    Handles.DrawSolidDisc(localEndControl, Camera.current.transform.forward, radius);

                    meanTween.endPoint = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localEndPoint, quaternion.identity));
                    meanTween.endControlPoint = meanTween.transform.parent.InverseTransformPoint(Handles.PositionHandle(localEndControl, quaternion.identity));

                }
                else
                {
                    meanTween.startPoint = Handles.PositionHandle(meanTween.startPoint, quaternion.identity);
                    meanTween.startControlPoint = Handles.PositionHandle(meanTween.startControlPoint, quaternion.identity);

                    Handles.DrawSolidDisc(meanTween.startPoint, Camera.current.transform.forward, radius);
                    Handles.DrawSolidDisc(meanTween.startControlPoint, Camera.current.transform.forward, radius);

                    Handles.color = new Color(1, 0, 1, 0.5f);
                    for (int i = 0; i < meanTween.pathPoints.Count; i++)
                    {

                        Vector3 newPos = Handles.PositionHandle(meanTween.pathPoints[i].point, quaternion.identity);
                        Vector3 newPos2 = Handles.PositionHandle(meanTween.pathPoints[i].control1, quaternion.identity);
                        Vector3 newPos3 = Handles.PositionHandle(meanTween.pathPoints[i].control2, quaternion.identity);

                        MeanBehaviour.BezierPoint bezierPoint1 = meanTween.pathPoints[i];
                        bezierPoint1.point = newPos;
                        bezierPoint1.control1 = newPos2;
                        bezierPoint1.control2 = newPos3;
                        meanTween.pathPoints[i] = bezierPoint1;

                        Handles.DrawLine(meanTween.pathPoints[i].point, meanTween.pathPoints[i].control1);
                        Handles.DrawLine(meanTween.pathPoints[i].point, meanTween.pathPoints[i].control2);

                        Handles.DrawSolidDisc(meanTween.pathPoints[i].point, Camera.current.transform.forward, radius);
                        Handles.DrawSolidDisc(meanTween.pathPoints[i].control1, Camera.current.transform.forward, radius);
                        Handles.DrawSolidDisc(meanTween.pathPoints[i].control2, Camera.current.transform.forward, radius);


                    }
                    Handles.color = new Color(0, 1, 0, 0.5f);
                    Handles.DrawLine(meanTween.startControlPoint, meanTween.startPoint);
                    Handles.color = new Color(1, 0, 0, 0.5f);
                    Handles.DrawLine(meanTween.endControlPoint, meanTween.endPoint);

                    Handles.DrawSolidDisc(meanTween.endPoint, Camera.current.transform.forward, radius);
                    Handles.DrawSolidDisc(meanTween.endControlPoint, Camera.current.transform.forward, radius);

                    meanTween.endPoint = Handles.PositionHandle(meanTween.endPoint, quaternion.identity);
                    meanTween.endControlPoint = Handles.PositionHandle(meanTween.endControlPoint, quaternion.identity);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            componentsLookup.Clear();

            MeanTween meanTween = (MeanTween)target;

            Color defaultColor = GUI.color;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToTween"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnAwake"));

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

                bool spline = false;
                SerializedProperty splineProp = null;
                if (meanTween.tweenType == MeanBehaviour.TWEENTYPE.Move)
                {
                    splineProp = serializedObject.FindProperty("path");
                    spline = splineProp.GetValue<bool>();

                    EditorGUILayout.PropertyField(splineProp);
                }

                bool rotateAround = false;
                if (!spline)
                {
                    SerializedProperty additivieProp = serializedObject.FindProperty("additive");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("space"));
                    EditorGUILayout.PropertyField(additivieProp);

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
                    }

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
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("space"));

                    EditorGUILayout.LabelField("Path Settings", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("orientToPath"));
                    if (GUILayout.Button("Set to transform position", EditorStyles.miniButton))
                    {
                        if (meanTween.space == MeanBehaviour.SPACE.Local)
                        {
                            meanTween.startPoint = meanTween.transform.localPosition;
                            meanTween.startControlPoint = meanTween.transform.localPosition;
                        }
                        else
                        {
                            meanTween.startPoint = meanTween.transform.position;
                            meanTween.startControlPoint = meanTween.transform.position;
                        }

                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startPoint"));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startControlPoint"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pathPoints"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endPoint"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endControlPoint"));
                }
            }

            if (meanTween.path)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
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
                EditorUtility.SetDirty(target);
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