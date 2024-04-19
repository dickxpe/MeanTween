using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

using Graphs = UnityEditor.Graphs;

namespace EventVisualizer.Base
{
	// Specialized edge drawer class
	public class EdgeGUI : Graphs.IEdgeGUI
	{
		#region Public members

		public Graphs.GraphGUI host { get; set; }
		public List<int> edgeSelection { get; set; }

		public EdgeGUI()
		{
			edgeSelection = new List<int>();
		}

		#endregion

		#region IEdgeGUI implementation


		public void DoEdges()
		{
			// Draw edges on repaint.
			if (Event.current.type == EventType.Repaint)
			{
				int sameCount = 0;

				foreach (var edge in host.graph.edges)
				{
					if (edge == _moveEdge) continue;

					Vector2Int indexes = FindSlotIndexes(edge);
					DrawEdge(edge, indexes, ColorForIndex(edge.fromSlotName), ref sameCount);
				}
			}
		}
		private Vector2Int FindSlotIndexes(Edge edge)
		{
			Vector2Int indexes = Vector2Int.zero;

			int totalOutputs = 0;
			bool found = false;
			foreach (var slot in edge.fromSlot.node.outputSlots)
			{
				if (slot != edge.fromSlot && !found)
				{
					indexes.x++;
				}
				else
				{
					found = true;
				}
				totalOutputs++;
			}
			indexes.x = totalOutputs - indexes.x - 1;

			foreach (var slot in edge.toSlot.node.inputSlots)
			{
				if (slot != edge.toSlot)
				{
					indexes.y++;
				}
				else
				{
					break;
				}
			}

			return indexes;
		}

		public static Color ColorForIndex(string name)
		{
			int hash = Math.Abs(Animator.StringToHash(name));
			return Color.HSVToRGB((float)(hash / (double)int.MaxValue), 1f, 1f);
		}

		public void DoDraggedEdge()
		{

		}

		public void BeginSlotDragging(Graphs.Slot slot, bool allowStartDrag, bool allowEndDrag)
		{

		}

		public void SlotDragging(Graphs.Slot slot, bool allowEndDrag, bool allowMultiple)
		{

		}

		public void EndSlotDragging(Graphs.Slot slot, bool allowMultiple)
		{

		}


		public void EndDragging()
		{

		}

		public Graphs.Edge FindClosestEdge()
		{
			return null;
		}


		#endregion

		#region Private members

		Graphs.Edge _moveEdge;
		Graphs.Slot _dragSourceSlot;
		Graphs.Slot _dropTarget;

		#endregion

		#region Edge drawer

		public const float kEdgeWidth = 6;

		private static Node prevNode;

		static void DrawEdge(Edge edge, Vector2Int indexes, Color color, ref int sameCount)
		{

			var p1 = GetPositionAsFromSlot(edge.fromSlot, indexes.x);
			var p10 = GetPositionAsToSlot(edge.toSlot, indexes.y);
			if (edge.fromSlot.node != prevNode)
			{
				sameCount = 0;
			}
			if (edge.fromSlot.node == edge.toSlot.node)
			{
				Node node = edge.fromSlot.node;
				IEnumerable<Slot> slots = node.slots.Where(x => x.isOutputSlot == true) as List<Slot>;
				//	int index = outputSlots.IndexOf(edge.fromSlot);
				Vector2 nodePos = new Vector2(node.position.x, node.position.y);
				Vector2 nodeSize = new Vector2(node.position.width, node.position.height);

				float offset = 15 + sameCount * 15;

				float y1DistFromTop = Mathf.Abs(nodePos.y - p1.y);
				float y2DistFromTop = Mathf.Abs(nodePos.y - p10.y);
				var p2 = new Vector2(p1.x + offset, p1.y - y1DistFromTop / 2);
				var p3 = new Vector2(p1.x + offset, p1.y);
				var p4 = new Vector2(p1.x, p1.y - y1DistFromTop - offset);
				var p5 = new Vector2(p1.x + offset, p1.y - y1DistFromTop - offset);
				var p6 = new Vector2(p10.x, p1.y - y1DistFromTop - offset);
				var p7 = new Vector2(p10.x - offset, p10.y - y2DistFromTop / 2);
				var p8 = new Vector2(p10.x - offset, p1.y - y1DistFromTop - offset);
				var p9 = new Vector2(p10.x - offset, p10.y);
				DrawEdge(p1, p2, p3, p3, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
				DrawEdge(p2, p4, p5, p5, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
				DrawEdge(p4, p6, p4, p6, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
				DrawEdge(p6, p7, p8, p8, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
				DrawEdge(p7, p10, p9, p9, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
				sameCount++;
				prevNode = edge.fromSlot.node;
			}
			else
			{
				sameCount = 0;
				DrawEdge(p1, p10, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
			}

		}

		static void DrawEdge(Vector2 p1, Vector2 p2, Color color, List<float> triggers)
		{
			var l = Mathf.Min(Mathf.Abs(p1.y - p2.y), 50);
			Vector2 p3 = p1 + new Vector2(l, 0);
			Vector2 p4 = p2 - new Vector2(l, 0);
			DrawEdge(p1, p2, p3, p4, color, triggers);
		}

		static void DrawEdge(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color, List<float> triggers)
		{
			Color prevColor = Handles.color;
			Handles.color = color;
			var texture = (Texture2D)Graphs.Styles.selectedConnectionTexture.image;
			Handles.DrawBezier(p1, p2, p3, p4, color, texture, kEdgeWidth);



			foreach (var trigger in triggers)
			{
				Vector3 pos = CalculateBezierPoint(trigger, p1, p3, p4, p2);
				Handles.DrawSolidArc(pos, Vector3.back, pos + Vector3.up, 360, kEdgeWidth);

			}

			Handles.color = prevColor;
		}


		#endregion

		#region Utilities to access private members

		const float kEdgeBottomMargin = 4;
		const float kNodeTitleSpace = 36;
		const float kNodeEdgeSeparation = 12;

		static Vector2 GetPositionAsFromSlot(Slot slot, int index)
		{
			NodeGUI node = slot.node as NodeGUI;
			Vector2 pos = node.position.position;

			pos.y = node.position.yMax - kEdgeBottomMargin;
			pos.y -= kNodeEdgeSeparation * index;

			pos.x = node.position.xMax;

			return pos;
		}

		static Vector2 GetPositionAsToSlot(Slot slot, int index)
		{
			NodeGUI node = slot.node as NodeGUI;
			Vector2 pos = node.position.position;
			pos.y += kNodeTitleSpace;
			pos.y += kNodeEdgeSeparation * index;
			pos.x = node.position.x;

			return pos;
		}

		#endregion

		public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float u = 1.0f - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector3 p = uuu * p0;
			p += 3 * uu * t * p1;
			p += 3 * u * tt * p2;
			p += ttt * p3;

			return p;
		}
	}

}
