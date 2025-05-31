using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;


// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Needle.Console
{
		internal class Patch_ConsoleWindowListView : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				// For the Prefix on SplitterGUILayout.BeginVerticalSplit
				var splitterGUILayoutType = typeof(Editor).Assembly.GetType("UnityEditor.SplitterGUILayout");
				if (splitterGUILayoutType != null)
				{
					var splitterStateType = typeof(Editor).Assembly.GetType("UnityEditor.SplitterState");
					if (splitterStateType != null)
					{
						// Targetting public static void BeginVerticalSplit(SplitterState state, params GUILayoutOption[] options)
						var beginVerticalSplitMethod = AccessTools.Method(splitterGUILayoutType, "BeginVerticalSplit", new[] { splitterStateType, typeof(GUILayoutOption[]) });

						if (beginVerticalSplitMethod != null)
						{
							yield return beginVerticalSplitMethod;
						}
						else
						{
							UnityEngine.Debug.LogError("Needle Console: Could not find SplitterGUILayout.BeginVerticalSplit(SplitterState, GUILayoutOption[]) method to patch for DrawListPrefix.");
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Needle Console: Could not find UnityEditor.SplitterState type.");
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Needle Console: Could not find UnityEditor.SplitterGUILayout type.");
				}

				// For Postfix on GUILayout.FlexibleSpace
				var guilayoutType = typeof(UnityEngine.GUILayout);
				var flexibleSpaceMethod = AccessTools.Method(guilayoutType, "FlexibleSpace", new System.Type[] { });
				if (flexibleSpaceMethod != null)
				{
					yield return flexibleSpaceMethod;
				}
				else
				{
					UnityEngine.Debug.LogError("Needle Console: Could not find GUILayout.FlexibleSpace() method to patch for DrawFoldoutsPostfix.");
				}

				// For Postfix on GUILayout.EndHorizontal
				var endHorizontalMethod = AccessTools.Method(guilayoutType, "EndHorizontal", new System.Type[] { });
				if (endHorizontalMethod != null)
				{
					yield return endHorizontalMethod;
				}
				else
				{
					UnityEngine.Debug.LogError("Needle Console: Could not find GUILayout.EndHorizontal() method to patch for DrawToolbarIconPostfix.");
				}

				// For Postfix on LogEntries.GetLinesAndModeFromEntryInternal
				var logEntriesType = typeof(LogEntries);
				var getLinesAndModeMethod = AccessTools.Method(logEntriesType, "GetLinesAndModeFromEntryInternal", new[]
				{
					typeof(LogEntry), // entry
					typeof(int),      // row
					typeof(int),      // totalRows
					typeof(string).MakeByRefType(), // outString
					typeof(int).MakeByRefType()     // outMode
				});

				if (getLinesAndModeMethod != null)
				{
					yield return getLinesAndModeMethod;
				}
				else
				{
					UnityEngine.Debug.LogError("Needle Console: Could not find LogEntries.GetLinesAndModeFromEntryInternal method to patch for ModifyTextPostfix.");
				}
			}

			[HarmonyPrefix]
			private static bool DrawListPrefix()
			{
				var consoleWindow = EditorWindow.GetWindow(Patch_Console.ConsoleWindowType);
				if (consoleWindow == null)
				{
					return true;
				}

				if (!ConsoleList.OnDrawList(consoleWindow))
				{
					return false; // Skip original method (BeginVerticalSplit)
				}
				return true; // Proceed with original method
			}

			[HarmonyPostfix]
			private static void DrawFoldoutsPostfix()
			{
				ConsoleToolbarFoldout.OnDrawFoldouts();
			}

			[HarmonyPostfix]
			private static void DrawToolbarIconPostfix()
			{
				ConsoleToolbarIcon.OnDrawToolbar();
			}

			[HarmonyPostfix]
			private static void ModifyTextPostfix(
				[HarmonyArgument(0)] LogEntry entry,
				[HarmonyArgument(1)] int row,
				[HarmonyArgument(3)] ref string outString)
			{
				// The original method is LogEntries.GetLinesAndModeFromEntryInternal(LogEntry entry, int row, int totalRows, out string outString, out int outMode)
				// entry is argument 0
				// row is argument 1
				// outString is argument 3
				if (entry == null) return;
				Needle.Console.ConsoleTextPrefix.ModifyTextInternal(entry, row, ref outString);
			}
			// Original commented out code follows...
			// private class Instructor
			// {
			// 	public List<Func<CodeInstruction, bool>> List = new List<Func<CodeInstruction, bool>>();
			// 	public Func<CodeInstruction, bool> OnFound;
			//
			// 	private int currentListIndex;
			// 	private int firstIndex;
			// 	
			// 	public void Check(CodeInstruction inst, int index)
			// 	{
			// 		var check = List[currentListIndex];
			// 		if (check(inst))
			// 		{
			// 			currentListIndex += 1;
			// 			if (currentListIndex >= List.Count)
			// 			{
			// 				OnFound?.Invoke()
			// 			}
			// 		}
			// 	}
			// }
		}
}