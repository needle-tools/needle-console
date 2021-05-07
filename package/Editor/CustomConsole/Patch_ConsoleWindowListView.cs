using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using needle.EditorPatching;
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
				var skipLabel = il.DefineLabel();
				var arr = instructions.ToArray();
				var loadListViewElementIndex = -1;

				for (var index = 0; index < arr.Length; index++)
				{
					var inst = arr[index];

					// if(customDraw && index >= 319 && index <= arr.Length-1) continue;
					// if(index > 350 && index < 500)
					Debug.Log("<color=grey>" + index + ": " + inst + "</color>");

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
							yield return CodeInstruction.Call(typeof(ConsoleText), nameof(ConsoleText.ModifyText));
							continue;
						}
					}

					if (inst.opcode == OpCodes.Ret)
					{
						inst.labels.Add(skipLabel);
					}

					// this is right before  SplitterGUILayout.BeginVerticalSplit(spl);
					if (index == 318)
					{
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return CodeInstruction.Call(typeof(ConsoleList), nameof(ConsoleList.OnDrawList));
						yield return new CodeInstruction(OpCodes.Brfalse, skipLabel);
					}
					// this is before "EndHorizontal"
					else if (index == 317)
					{
						yield return CodeInstruction.Call(typeof(ConsoleList), nameof(ConsoleList.OnDrawToolbar));
					}
					// this is before "GUILayout.FlexibleSpace"
					// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L539
					else if (index == 209)
					{
						yield return CodeInstruction.Call(typeof(ConsoleOptionsFoldout), nameof(ConsoleOptionsFoldout.OnDrawFoldouts));
					}
					
					yield return inst;
				}
			}
		}
	}
}