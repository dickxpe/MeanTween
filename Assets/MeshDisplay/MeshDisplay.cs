using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

/// <summary>
/// Morten Nobel-Jørgensen
/// https://github.com/mortennobel/ProceduralMesh/blob/master/MeshDisplay.cs
/// Utility class that let you see normals and tangent vectors for a mesh.
/// This is really useful when debugging mesh appearance
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class MeshDisplay : EditorWindow
{
	[SerializeField]
	public bool showNormals = true;
	[SerializeField]
	public Color normalColor = Color.red;
	[SerializeField]
	public float normalLength = .2f;

	[SerializeField]
	public bool showTangents = false;
	[SerializeField]
	public Color tangentColor = Color.blue;
	[SerializeField]
	public float tangentLength = .2f;

	[SerializeField]
	List<MeshFilter> meshes = new List<MeshFilter>();

	static MeshDisplay instance;

	[MenuItem("Window/Analysis/Mesh Display", false, 0)]

	static void DisplayMesh(MenuCommand command)
	{
		EditorWindow.GetWindow(typeof(MeshDisplay));
	}

	protected void OnEnable()
	{
		instance = this;
		SceneView.duringSceneGui += OnSceneGUI;

		string data = EditorPrefs.GetString("MeshDisplaySettings", JsonUtility.ToJson(this, false));
		JsonUtility.FromJsonOverwrite(data, this);
	}

	protected void OnDisable()
	{
		string data = JsonUtility.ToJson(this, false);
		EditorPrefs.SetString("MeshDisplaySettings", data);
		showNormals = false;
		showTangents = false;
	}

	void OnGUI()
	{
		ScriptableObject target = this;
		SerializedObject serializedObject = new SerializedObject(target);
		serializedObject.Update();
		SerializedProperty normalProp = serializedObject.FindProperty("showNormals");
		EditorGUILayout.PropertyField(normalProp);
		if (normalProp.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("normalColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("normalLength"));
		}
		SerializedProperty tangentProp = serializedObject.FindProperty("showTangents");
		EditorGUILayout.PropertyField(tangentProp);
		if (tangentProp.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("tangentColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("tangentLength"));
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("meshes"));
		serializedObject.ApplyModifiedProperties();
		HandleUtility.Repaint();
	}

	void OnSceneGUI(SceneView sceneView)
	{
		foreach (MeshFilter meshFilter in instance.meshes)
		{
			if (meshFilter == null)
			{
				return;
			}
			Mesh mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{

				return;
			}

			bool doShowNormals = instance.showNormals && mesh.normals.Length == mesh.vertices.Length;
			bool doShowTangents = instance.showTangents && mesh.tangents.Length == mesh.vertices.Length;

			foreach (int idx in mesh.triangles)
			{
				Vector3 vertex = meshFilter.transform.TransformPoint(mesh.vertices[idx]);

				if (doShowNormals)
				{
					Vector3 normal = meshFilter.transform.TransformDirection(mesh.normals[idx]);
					Handles.color = instance.normalColor;
					Handles.DrawLine(vertex, vertex + normal * instance.normalLength);
				}
				if (doShowTangents)
				{
					Vector3 tangent = meshFilter.transform.TransformDirection(mesh.tangents[idx]);
					Handles.color = instance.tangentColor;
					Handles.DrawLine(vertex, vertex + tangent * instance.tangentLength);
				}
			}
		}
	}
}