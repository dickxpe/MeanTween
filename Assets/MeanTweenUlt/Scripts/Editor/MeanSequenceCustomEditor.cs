// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections.Generic;
using System.Linq;
using UltEvents.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.zebugames.meantween.ult
{

    [CustomEditor(typeof(MeanSequence))]
    public class MeanSequenceCustomEditor : Editor
    {

        SerializedProperty sequence;

        ReorderableList sequenceList;

        Dictionary<SequenceTween, CustomMeanTweenList> tweenLists = new Dictionary<SequenceTween, CustomMeanTweenList>();

        CustomMeanTweenList tweenlist;
        List<float> heights = new List<float>();
        SequenceTween sequenceTween;

        MeanSequence meanSequence;
        Color defaultColor;

        private void OnEnable()
        {
            defaultColor = GUI.color;
            tweenLists.Clear();
            sequence = serializedObject.FindProperty("sequence");
            sequenceList = new ReorderableList(serializedObject, sequence, true, true, true, true);
            sequenceList.drawElementCallback = DrawSequenceListItems;
            sequenceList.drawHeaderCallback = DrawSequenceHeader;
            sequenceList.onAddCallback = AddCallBack;
        }


        void DrawSequenceListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            sequenceTween = meanSequence.sequence[index];
            SerializedProperty element = sequenceList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty tweens = element.FindPropertyRelative("tweens");
            List<MeanBehaviour> tweenss = (List<MeanBehaviour>)tweens.GetValue();
            EditorGUI.BeginChangeCheck();
            SerializedProperty targetProp = element.FindPropertyRelative("targetGameObject");
            EditorGUI.PropertyField(
                          new Rect(rect.x + 0, rect.y + 5, 150, EditorGUIUtility.singleLineHeight),
                         targetProp,
                          GUIContent.none
                      );

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                sequenceTween.targetGameObject = targetProp.GetValue<GameObject>();
                sequenceTween.tweens.Clear();
                if (sequenceTween.targetGameObject != null)
                {
                    sequenceTween.tweens.Add(sequenceTween.targetGameObject.GetComponent<MeanBehaviour>());
                }
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }



            if (sequenceTween.targetGameObject != null)
            {

                if (sequenceTween.targetGameObject.GetComponents<MeanBehaviour>().Count() > 0)
                {
                    SerializedProperty simulProp = element.FindPropertyRelative("playSimultaneously");
                    simulProp.boolValue = EditorGUI.ToggleLeft(new Rect(rect.x + 0, rect.y + 10 + EditorGUIUtility.singleLineHeight, 150, EditorGUIUtility.singleLineHeight), "Play Simulataneously", simulProp.boolValue);
                    if (!tweenLists.ContainsKey(meanSequence.sequence[index]))
                    {

                        tweenlist = new CustomMeanTweenList(serializedObject, tweens, true, true, true, true);
                        tweenlist.elementHeight = EditorGUIUtility.singleLineHeight;
                        tweenlist.drawElementCallback = DrawTweenListItems;
                        tweenlist.drawHeaderCallback = DrawTweensHeader;
                        tweenlist.onAddCallback = AddTweenCallback;
                        tweenlist.DoLayoutList(rect);
                        tweenLists.Add(meanSequence.sequence[index], tweenlist);
                        DrawTweenListItems(rect, 0, true, true);
                    }
                    else
                    {
                        if (tweenLists.TryGetValue(meanSequence.sequence[index], out tweenlist))
                        {
                            tweenlist.DoLayoutList(rect);
                        }
                    }
                }
                else
                {
                    GUI.color = new Color(1, 0.5f, 0.5f);
                    EditorGUI.LabelField(new Rect(rect.x + 170, rect.y, rect.width, rect.height), "Selected gameObject has no MeanTweens");
                    GUI.color = defaultColor;
                }

            }
            else
            {
                EditorGUI.LabelField(new Rect(rect.x + 170, rect.y - 10, rect.width, rect.height), "Select a gameObject with MeanTweens");
            }
        }

        void DrawTweenListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            List<MeanBehaviour> components = sequenceTween.targetGameObject.GetComponents<MeanBehaviour>().Where(x => x.GetType().BaseType == typeof(MeanBehaviour)).ToList();
            string[] componentStrings = Array.ConvertAll(components.ToArray(), x => x.tweenName);

            for (int i = 0; i < componentStrings.Length; i++)
            {
                int charIndex = componentStrings[i].IndexOf('(');
                componentStrings[i] = componentStrings[i].Substring(charIndex + 1, componentStrings[i].Length - charIndex - 1) + ": " + components[i].tweenType.ToString();
                componentStrings[i] += " - " + components[i].loopType.ToString();
                if (components[i].loopType != MeanBehaviour.LOOPTYPE.Once)
                {
                    if (components[i].infiniteLoop)
                    {
                        componentStrings[i] += " ∞→1x";
                    }
                    else
                    {
                        componentStrings[i] += " " + components[i].loops + "x";
                    }
                }
                componentStrings[i] += " - " + components[i].duration + "s";
            }

            if (sequenceTween.tweens.Count > 0)
            {
                if (sequenceTween.tweens[index] == null)
                {
                    int selectedId = EditorGUI.Popup(rect, 0, componentStrings);
                    sequenceTween.tweens[index] = components[selectedId];
                }
                else
                {
                    int selectedId = EditorGUI.Popup(rect, components.IndexOf(sequenceTween.tweens[index]), componentStrings);
                    if (selectedId >= 0)
                    {
                        sequenceTween.tweens[index] = components[selectedId];
                    }
                }
            }
        }

        void DrawTweensHeader(Rect rect)
        {
            //draw empty header
        }

        void DrawSequenceHeader(Rect rect)
        {
            string name = "Sequence";
            EditorGUI.LabelField(rect, name);
        }

        void AddCallBack(ReorderableList list)
        {
            if (list.serializedProperty != null)
            {
                list.serializedProperty.arraySize++;
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
                element.FindPropertyRelative("targetGameObject").SetValue(null);
                element.FindPropertyRelative("tweens").ClearArray();
            }
        }

        void AddTweenCallback(ReorderableList list)
        {
            if (list.serializedProperty != null)
            {
                list.serializedProperty.arraySize++;
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
                element.SetValue(null);
            }
        }

        public override void OnInspectorGUI()
        {
            meanSequence = (MeanSequence)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnAwake"));
            if (meanSequence.sequence.Count > 0 && meanSequence.sequence[meanSequence.sequence.Count - 1].targetGameObject == null)
            {
                sequenceList.displayAdd = false;
            }
            else
            {
                sequenceList.displayAdd = true;
            }

            sequenceList.elementHeightCallback = (index) =>
            {
                Repaint();
                float height = 0;
                float count = meanSequence.sequence[index].tweens.Count;
                try
                {
                    if (meanSequence.sequence[index].targetGameObject == null)
                    {
                        height = EditorGUIUtility.singleLineHeight * 2 + 10;
                    }
                    else
                    {
                        if (count < 2)
                        {
                            count = 1;
                        }
                        if (meanSequence.sequence[index].targetGameObject.GetComponents<MeanBehaviour>().Count() == 0)
                        {
                            height = EditorGUIUtility.singleLineHeight + 5;
                        }
                        else
                        {
                            height = 4.5f * EditorGUIUtility.singleLineHeight + (count - 2) * (EditorGUIUtility.singleLineHeight + 2);

                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }

                return height;
            };

            sequenceList.DoLayoutList();

            meanSequence.showEvents = EditorGUILayout.Foldout(meanSequence.showEvents, "Events");
            if (meanSequence.showEvents)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onPlayNext"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onCompleted"));
            }

            if (EditorApplication.isPlaying)
            {
                GUI.color = Color.cyan;
                if (GUILayout.Button("Play Sequence", EditorStyles.miniButton))
                {
                    foreach (SequenceTween sequence in meanSequence.sequence)
                    {
                        if (sequence.tweens != null)
                        {
                            if (sequence.tweens.Count > 0)
                            {
                                sequence.tweens[0].CancelAll();
                            }
                        }
                    }
                    meanSequence.Play();
                }
                GUI.color = defaultColor;
            }



            serializedObject.ApplyModifiedProperties();
        }
    }
}