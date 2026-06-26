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
			var method = Patch_Console.ConsoleWindowType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
			yield return method;
		}

		private static IEnumerable<CodeInstruction> Transpiler(MethodBase method, ILGenerator il, IEnumerable<CodeInstruction> instructions)
		{
#if !NETSTANDARD
			var skipLabel = il.DefineLabel();
			var arr = instructions.ToArray();
			var loadListViewElementIndex = -1;

			// The insertion points used to be hard-coded instruction indices per Unity version.
			// Those indices shift whenever UnityEditor.ConsoleWindow.OnGUI changes and silently
			// broke patching on Unity 6.4+ with an InvalidProgramException (the OnDrawList guard
			// ended up at the wrong stack position). We now locate the insertion points by their
			// surrounding IL instead, so the patch keeps working across Unity versions.
			var foldoutIndex = -1; // before GUILayout.FlexibleSpace
			var toolbarIndex = -1; // before GUILayout.EndHorizontal
			var listIndex = -1; // before the SplitterGUILayout.BeginVerticalSplit(spl) block
#if UNITY_2020_1_OR_NEWER
			for (var i = 0; i < arr.Length; i++)
			{
				if (arr[i].opcode != OpCodes.Call || !(arr[i].operand is MethodInfo cm))
					continue;

				if (foldoutIndex == -1 && cm.Name == "FlexibleSpace" && cm.DeclaringType?.Name == "GUILayout")
					foldoutIndex = i;
				else if (toolbarIndex == -1 && cm.Name == "EndHorizontal" && cm.DeclaringType?.Name == "GUILayout")
					toolbarIndex = i;
				else if (listIndex == -1 && cm.Name == "BeginVerticalSplit" && cm.DeclaringType?.Name == "SplitterGUILayout")
				{
					// walk back to the start of the call's arguments ("ldarg.0; ldfld spl; ...") so the
					// inserted guard does not corrupt the stack used to push BeginVerticalSplit's arguments
					for (var k = i - 1; k > 0; k--)
					{
						if (arr[k].opcode == OpCodes.Ldfld && arr[k].operand is FieldInfo f && f.Name == "spl" && arr[k - 1].IsLdarg(0))
						{
							listIndex = k - 1;
							break;
						}
					}
				}
			}
#endif

			for (var index = 0; index < arr.Length; index++)
			{
				var inst = arr[index];

				// UnityEngine.Debug.Log("<color=grey>" + index + ": " + inst + "</color>");

				// get local index for current list view element
				if (loadListViewElementIndex == -1 || inst.IsStloc() && inst.operand is LocalBuilder)
				{
					var loc = inst.operand as LocalBuilder;
					if (loc?.LocalType == typeof(ListViewElement))
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
						yield return CodeInstruction.Call(typeof(ConsoleTextPrefix), nameof(ConsoleTextPrefix.ModifyText));
						continue;
					}
				}

				if (inst.opcode == OpCodes.Ret)
				{
					inst.labels.Add(skipLabel);
				}

				// this is before "GUILayout.FlexibleSpace"
				// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L539
				if (index == foldoutIndex)
				{
					yield return CodeInstruction.Call(typeof(ConsoleToolbarFoldout), nameof(ConsoleToolbarFoldout.OnDrawFoldouts));
				}

				// this is before "EndHorizontal"
				// https://github.com/Unity-Technologies/UnityCsReference/blob/4d031e55aeeb51d36bd94c7f20182978d77807e4/Editor/Mono/ConsoleWindow.cs#L600
				if (index == toolbarIndex)
				{
					yield return CodeInstruction.Call(typeof(ConsoleToolbarIcon), nameof(ConsoleToolbarIcon.OnDrawToolbar));
				}

				// this is right before  SplitterGUILayout.BeginVerticalSplit(spl);
				if (index == listIndex)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return CodeInstruction.Call(typeof(ConsoleList), nameof(ConsoleList.OnDrawList));
					yield return new CodeInstruction(OpCodes.Brfalse, skipLabel);
				}

				yield return inst;
			}
#else
				if (SessionState.GetBool("NeedleConsole:NetStandardIsUnsupportedWarning", false) == false)
			{
				SessionState.SetBool("NeedleConsole:NetStandardIsUnsupportedWarning", true);
				UnityEngine.Debug.LogWarning("Needle Console does currently not support .NET Standard ('Project Settings/Player/Editor Assemblies Compatibility Level') set to : https://github.com/needle-tools/needle-console/issues/27");
			}
			return instructions;
#endif
		}
	}
}
