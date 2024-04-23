using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.Events;
using System;
using UnityEditor.Callbacks;
using System.Linq;
using UltEvents;

namespace EventVisualizer.Base
{
	public static class EventsFinder
	{
		public static List<EventCall> FindAllEvents(GameObject[] roots, bool searchHierarchy = true)
		{
			RefreshTypesThatCanHoldUnityEvents();
			HashSet<EventCall> calls = new HashSet<EventCall>();
			foreach (var type in ComponentsThatCanHaveUnityEvent)
			{
				if (type.IsGenericTypeDefinition)
				{
					continue;
				}

				HashSet<UnityEngine.Object> selectedComponents = new HashSet<UnityEngine.Object>();
				if (roots != null && roots.Length > 0)
				{
					foreach (var root in roots)
					{
						if (root != null)
						{
							if (searchHierarchy)
							{
								selectedComponents.UnionWith(root.GetComponentsInChildren(type));
							}
							else
							{
								selectedComponents.Add(root.GetComponent(type));
							}
						}
					}
				}
				else
				{
					selectedComponents = new HashSet<UnityEngine.Object>(GameObject.FindObjectsOfType(type));
				}

				foreach (UnityEngine.Object caller in selectedComponents)
				{
					Component comp = caller as Component;
					if (comp != null)
					{
						ExtractDefaultEventTriggers(calls, comp);
						ExtractEvents(calls, comp);
					}
				}
			}
			return calls.ToList();
		}

		private static void ExtractEvents(HashSet<EventCall> calls, Component caller)
		{
			SerializedProperty iterator = new SerializedObject(caller).GetIterator();
			iterator.Next(true);
			RecursivelyExtractEvents(calls, caller, iterator, 0);
		}

		private static bool RecursivelyExtractEvents(HashSet<EventCall> calls, Component caller, SerializedProperty iterator, int level)
		{
			bool hasData = true;

			do
			{
				SerializedProperty persistentCalls;
				bool isUltEvent = false;

				if (iterator.type.Contains("Event"))
				{
					if ((int)iterator.FindPropertyRelative("_PersistentCalls.Array.size").boxedValue > 0)
					{
						persistentCalls = iterator.FindPropertyRelative("_PersistentCalls.Array");
						isUltEvent = true;
					}
					else
					{
						persistentCalls = null;
					}
				}
				else
				{
					persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
					isUltEvent = false;
				}

				bool isUnityEvent = persistentCalls != null;
				if (isUnityEvent && persistentCalls.arraySize > 0)
				{
					if (isUltEvent)
					{
						UltEventBase ultEvent = Puppy.EditorHelper.GetTargetObjectOfProperty(iterator) as UltEventBase;
						if (ultEvent != null)
						{
							AddEventCalls(calls, caller, ultEvent, iterator.displayName, iterator.propertyPath);
						}
					}
					else
					{
						UnityEventBase unityEvent = Puppy.EditorHelper.GetTargetObjectOfProperty(iterator) as UnityEventBase;
						if (unityEvent != null)
						{
							AddEventCalls(calls, caller, unityEvent, iterator.displayName, iterator.propertyPath);
						}
					}
				}
				hasData = iterator.Next(!isUnityEvent);
				if (hasData)
				{
					if (iterator.depth < level) return hasData;
					else if (iterator.depth > level) hasData = RecursivelyExtractEvents(calls, caller, iterator, iterator.depth);
				}
			}
			while (hasData);
			return false;
		}

		private static void ExtractDefaultEventTriggers(HashSet<EventCall> calls, Component caller)
		{
			EventTrigger eventTrigger = caller as EventTrigger;
			if (eventTrigger != null)
			{
				foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
				{
					string name = trigger.eventID.ToString();
					AddEventCalls(calls, caller, trigger.callback, name, name);
				}
			}
		}

		private static void AddEventCalls(HashSet<EventCall> calls, Component caller, UnityEventBase unityEvent, string eventShortName, string eventFullName)
		{
			for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
			{
				string methodName = unityEvent.GetPersistentMethodName(i);
				UnityEngine.Object receiver = unityEvent.GetPersistentTarget(i);

				if (receiver != null && methodName != null && methodName != "")
				{
					calls.Add(new EventCall(caller, receiver, eventShortName, eventFullName, methodName, unityEvent));
				}
			}
		}

		private static void AddEventCalls(HashSet<EventCall> calls, Component caller, UltEventBase unityEvent, string eventShortName, string eventFullName)
		{
			for (int i = 0; i < unityEvent.PersistentCallsList.Count(); i++)
			{
				string methodName = unityEvent.PersistentCallsList[i].MemberName;
				UnityEngine.Object receiver = unityEvent.PersistentCallsList[i].Target;

				if (receiver != null && methodName != null && methodName != "")
				{
					EventCall call = new EventCall(caller, receiver, eventShortName, eventFullName, methodName, unityEvent);
					calls.Add(call);
					calls.Contains(call);
				}
			}
		}

		public static bool NeedsGraphRefresh = false;

		private static HashSet<Type> ComponentsThatCanHaveUnityEvent = new HashSet<Type>();
		private static Dictionary<Type, bool> TmpSearchedTypesUnityEvents = new Dictionary<Type, bool>();
		private static Dictionary<Type, bool> TmpSearchedTypesUltEvents = new Dictionary<Type, bool>();

		[DidReloadScripts, InitializeOnLoadMethod]
		static void RefreshTypesThatCanHoldUnityEvents()
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();

#if NET_4_6
			var objects = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
				.SelectMany(a => a.GetTypes())
				.Where(t => typeof(Component).IsAssignableFrom(t));
#else
			var objects = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => typeof(Component).IsAssignableFrom(t));
#endif

			foreach (var obj in objects)
			{
				if (RecursivelySearchFieldsForUnityEvents<UnityEventBase>(obj))
				{
					ComponentsThatCanHaveUnityEvent.Add(obj);
				}

				if (RecursivelySearchFieldsForUltEvents<UltEventBase>(obj))
				{
					ComponentsThatCanHaveUnityEvent.Add(obj);
				}
			}
			TmpSearchedTypesUnityEvents.Clear();
			TmpSearchedTypesUltEvents.Clear();

			//			Debug.Log("UnityEventVisualizer Updated Components that can have UnityEvents (" + ComponentsThatCanHaveUnityEvent.Count + "). Milliseconds: " + sw.Elapsed.TotalMilliseconds);
		}

		/// <summary>
		/// Search for types that have a field or property of type <typeparamref name="T"/> or can hold an object that can.
		/// </summary>
		/// <typeparam name="T">Needle</typeparam>
		/// <param name="type">Haystack</param>
		/// <returns>Can contain some object <typeparamref name="T"/></returns>
		static bool RecursivelySearchFieldsForUnityEvents<T>(Type type)
		{



			bool wanted;
			if (TmpSearchedTypesUnityEvents.TryGetValue(type, out wanted)) return wanted;
			TmpSearchedTypesUnityEvents.Add(type, false);

			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var fType in type.GetFields(flags).Where(f => !f.FieldType.IsPrimitive).Select(f => f.FieldType).Concat(type.GetProperties(flags).Select(p => p.PropertyType)))
			{
				if (typeof(T).IsAssignableFrom(fType))
				{
					return TmpSearchedTypesUnityEvents[type] |= true;
				}
				else if (typeof(UnityEngine.Object).IsAssignableFrom(fType))
				{
					continue;
				}
				else if (!TmpSearchedTypesUnityEvents.TryGetValue(fType, out wanted))
				{
					if (RecursivelySearchFieldsForUnityEvents<T>(fType))
					{
						return TmpSearchedTypesUnityEvents[type] |= true;
					}
				}
				else if (wanted)
				{
					return TmpSearchedTypesUnityEvents[type] |= true;
				}
			}

			if (type.IsArray)
			{
				if (RecursivelySearchFieldsForUnityEvents<T>(type.GetElementType()))
				{
					return TmpSearchedTypesUnityEvents[type] |= true;
				}
			}

			return false;
		}

		static bool RecursivelySearchFieldsForUltEvents<T>(Type type)
		{



			bool wanted;
			if (TmpSearchedTypesUltEvents.TryGetValue(type, out wanted)) return wanted;
			TmpSearchedTypesUltEvents.Add(type, false);

			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var fType in type.GetFields(flags).Where(f => !f.FieldType.IsPrimitive).Select(f => f.FieldType).Concat(type.GetProperties(flags).Select(p => p.PropertyType)))
			{
				if (typeof(T).IsAssignableFrom(fType))
				{
					return TmpSearchedTypesUltEvents[type] |= true;
				}
				else if (typeof(UnityEngine.Object).IsAssignableFrom(fType))
				{
					continue;
				}
				else if (!TmpSearchedTypesUltEvents.TryGetValue(fType, out wanted))
				{
					if (RecursivelySearchFieldsForUltEvents<T>(fType))
					{
						return TmpSearchedTypesUltEvents[type] |= true;
					}
				}
				else if (wanted)
				{
					return TmpSearchedTypesUltEvents[type] |= true;
				}
			}

			if (type.IsArray)
			{
				if (RecursivelySearchFieldsForUltEvents<T>(type.GetElementType()))
				{
					return TmpSearchedTypesUltEvents[type] |= true;
				}
			}

			return false;
		}
	}
}