using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using HarmonyLib;
using ICSharpCode.NRefactory.Ast;
using needle.EditorPatching;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Needle.Demystify
{
	public class Patch_ConsoleWindowListView : EditorPatchProvider
	{
		public override string Description => "Custom Console List View";

		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new ListViewPatch());
			// patches.Add(new EnumeratorPatch());
			// patches.Add(new ListViewStatePatch());
		}

		// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/GUI/ListViewGUILayout.cs#L183


		private class ListViewStatePatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = typeof(GettingLogEntriesScope).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[]{typeof(ListViewState)}, null);
				Debug.Log(method);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static void Postfix(ListViewState listView)
			{
				listView.totalRows = Mathf.Min(2, listView.totalRows);
			}
		}
		
		private class EnumeratorPatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = typeof(ListViewShared.ListViewElementsEnumerator).GetMethod("MoveNext");
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static int count;

			private static void Prefix(int ___xPos)
			{
				if (___xPos <= -1)
					count = 0;
			}
			
			// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/GUI/ListViewShared.cs#L411
			private static void Postfix(ListViewShared.ListViewElementsEnumerator __instance, ref bool __result, ref ListViewElement ___element, ListViewShared.InternalLayoutedListViewState ___ilvState)
			{
				if (__result)
				{
					if (___element.row == 0) count = 0;
					
					if (___element.row % 2 == 0)
					{
						count += 1;
						// if(___ilvState != null && ___ilvState.@group != null)
						// 	___ilvState.@group.AddY(-___ilvState.rectHeight);
						__instance.MoveNext(); 
					}
					else
					{
						var el = ___element.position;
						el.y -= el.height * count;
						// ___element.row -= count;
						// ___element.row = Mathf.Max(___element.row, 0);
						___element.position = el;
					}
				}
			}
		}

		private class ListViewPatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = Patch_Console.ConsoleWindowType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static IEnumerable<CodeInstruction> Transpiler(MethodBase method, ILGenerator il, IEnumerable<CodeInstruction> instructions)
			{ 
				
				// var skipLabel = il.DefineLabel();

				var arr = instructions.ToArray();
				// CodeInstruction enumeratorInstruction = null;
				
				var loadListViewElementIndex = -1;
				for (var index = 0; index < arr.Length; index++)
				{
					var inst = arr[index];
					// if(index > 350 && index < 500)
					// 	Debug.Log("<color=grey>" + index + ": " + inst + "</color>");
					// if (inst.opcode == OpCodes.Unbox_Any && inst.operand != null && inst.ToString() == "UnityEditor.ListViewElement")
					// {
					// 	Debug.Log("LIST VIEW ELEMENT " + inst.operand); 
					// }
					
					// if (enumeratorInstruction == null && inst.operand != null && inst.opcode == OpCodes.Callvirt && inst.operand is MethodInfo m)
					// {
					// 	if (m.DeclaringType?.Name.EndsWith("ListViewElementsEnumerator") ?? false)
					// 	{
					// 		enumeratorInstruction = inst;
					// 		enumeratorInstruction.labels.Add(continueLabel);
					// 		Debug.Log("ENUMERATOR " + m.FullDescription());
					// 	}
					// }


					// if (inst.IsStloc() && inst.operand != null && inst.operand is LocalBuilder loc && loc.LocalType == typeof(ListViewElement))
					// {
					// 	yield return inst;
					// 	Debug.Log("STORING " + inst + ", " + loc.LocalIndex);
					// 	yield return new CodeInstruction(OpCodes.Ldloc, loc.LocalIndex);
					// 	yield return CodeInstruction.Call(typeof(ListViewPatch), nameof(OnDrawElement), new[] {typeof(ListViewElement)});
					// 	yield return new CodeInstruction(OpCodes.Brfalse, skipLabel);
					// 	arr[653].labels.Add(skipLabel);
					// 	continue;
					// }


					// get local index for current list view element
					if (loadListViewElementIndex == -1 || inst.IsStloc() && inst.operand is LocalBuilder)
					{
						var loc = inst.operand as LocalBuilder;
						if(loc?.LocalType == typeof(ListViewElement))
							loadListViewElementIndex = loc.LocalIndex;
					}
					
					if (inst.opcode == OpCodes.Call && inst.operand is MethodInfo m)
					{
						if (m.DeclaringType == typeof(LogEntries) && m.Name == "GetLinesAndModeFromEntryInternal")
						{ 
							yield return inst;
							// load text is one element before
							var ldStr = arr[index - 1]; 
							yield return new CodeInstruction(OpCodes.Ldloc, loadListViewElementIndex);
							yield return ldStr;
							yield return CodeInstruction.Call(typeof(ConsoleListView), nameof(ConsoleListView.ModifyText));
							continue; 
						}
					}

					yield return inst; 
				}
			}

			// private static bool OnDrawElement(ListViewElement element)
			// {
			// 	if (Event.current.type == EventType.Repaint)
			// 	{
			// 		int mode = 0;
			// 		string text = null;
			// 		LogEntries.GetLinesAndModeFromEntryInternal(element.row, 1, ref mode, ref text);
			// 		var rect = element.position;
			// 		rect.x = rect.width - 100;
			// 		GUI.Label(rect, "TEST");
			// 	}
			//
			// 	return true;
			// 	// Debug.Log(element.row);
			// 	// return element.row % 2 == 0;
			// }

			// private static Vector2 scroll;
			// private static GUIStyle Box = "CN Box";
			//
			// // https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L475
			// private static bool Prefix_Disabled(ConsoleWindow __instance, ListViewState ___m_ListView)
			// {
			// 	var e = Event.current;
			// 	var m_ListView = ___m_ListView;
			// 	int id = GUIUtility.GetControlID(0);
			// 	using (new GettingLogEntriesScope(m_ListView))
			// 	{
			// 		int selectedRow = -1;
			// 		bool openSelectedItem = false;
			// 		bool collapsed = false;// HasFlag(ConsoleFlags.Collapse);
			// 		var multiSelection = 0;// ListViewOptions.wantsRowMultiSelection;
			// 		var tempContent = new GUIContent();
			// 		foreach (ListViewElement el in ListViewGUI.ListView(m_ListView, Box))
			// 		{
			// 			
			// 			if (e.type == EventType.Repaint)
			// 			{
			// 				int mode = 0;
			// 				string text = null;
			// 				LogEntries.GetLinesAndModeFromEntryInternal(el.row, 1, ref mode, ref text);
			// 				
			// 				
			// 				tempContent.text = text;
			// 				GUIStyle errorModeStyle =ConsoleWindow.GetStyleForErrorMode(mode, false, true);
			// 				var textRect = el.position;
			// 				// textRect.x += offset;
			// 				
			// 				errorModeStyle.Draw(textRect, tempContent, id, m_ListView.row == el.row);
			// 			}
			//
			// 		}
			// 	}
			// 	return false;
			// }

			// private static class LogHelper
			// {
			// 	public static string GetLines(string message)
			// 	{
			// 		var line = message.IndexOf("\n");
			// 		var sub = message.Substring(0, line);
			// 		return sub;
			// 	}
			// }
		}
	}
}