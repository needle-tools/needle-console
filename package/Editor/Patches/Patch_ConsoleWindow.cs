using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Threading.Tasks;
using HarmonyLib;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Label = System.Reflection.Emit.Label;

namespace Needle.Demystify
{
	public class Patch_ConsoleWindow : EditorPatchProvider
	{
		public override string DisplayName { get; }
		public override string Description => "Custom List View";

		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new ListViewPatch());
		}

		private class ListViewPatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				PatchManager.AllowDebugLogs = true;
				var method = Patch_Console.ConsoleWindowType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static IEnumerable<CodeInstruction> Transpiler(MethodBase method, ILGenerator emitter, IEnumerable<CodeInstruction> _instructions)
			{
				var instructions = _instructions.ToArray();

				// LogVariables(instructions);
				// return instructions;


				// var myOnGui = typeof(ListViewPatch).GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Static);

				var myOnGuiNoParams = typeof(ListViewPatch).GetMethod(nameof(OnGUI2), BindingFlags.NonPublic | BindingFlags.Static);

				// var myInstructions = PatchProcessor.GetCurrentInstructions(myOnGui);
				// Debug.Log(string.Join("\n", myInstructions));

				var newResult = new List<CodeInstruction>();


				// var loadListViewElement = FindLoadingLocalVariable(34, instructions);
				object listViewOperand = null;

				for (var index = 0; index < instructions.Length; index++)
				{
					var instr = instructions[index];


					bool Log() => newResult.Count > 150 && newResult.Count < 1500;

					void Add(CodeInstruction instruction)
					{
						// if(instruction.opcode == OpCodes.Leave_S)
						// if (Log())
						// { 
						// 	Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, index + ":\t" + instruction);
						// }

						// skip drawing logs
						if (index >= 343 && index <= 1002)
						{
							if (index == 343)
							{
								// load console window instance
								newResult.Add(new CodeInstruction(OpCodes.Ldarg_0));
								newResult.Add(new CodeInstruction(OpCodes.Call, myOnGuiNoParams));
							}

							return;
						}

						newResult.Add(instruction);
					}

					// if (listViewOperand == null && instr.operand != null)
					// {
					// 	// if (Log() && index == 372)
					// 	// 	Debug.Log(instr.operand?.GetType().FullName);
					// 	if(instr.operand.GetType().Name.EndsWith("RuntimeType") && 
					// 	   instr.operand.ToString() == "UnityEditor.ListViewElement") 
					// 	{
					// 		listViewOperand = instr.operand;
					// 	}
					// }

					// 343:	ldloca.s 28 (UnityEditor.GettingLogEntriesScope)


					// 373: stloc.s 34 (UnityEditor.ListViewElement)
					// 409: ldloc.s 34 (UnityEditor.ListViewElement)
					// if (index == 373) // && index <= 999)
					// {
					// 	// var lb = loadListViewElement.operand as LocalBuilder;
					// 	// new CodeInstruction(OpCodes.Box, )
					// 	// Add(new CodeInstruction(OpCodes.Box, typeof(object)));
					//
					// 	// load list view instance, it is a struct so we box it
					// 	Add(loadListViewElement);
					// 	Add(new CodeInstruction(OpCodes.Box, listViewOperand)); // newResult[372].operand)); 
					// 	// this is how to create an object array with length of 1
					// 	// Add(new CodeInstruction(OpCodes.Ldc_I4_1)); 
					// 	// Add(new CodeInstruction(OpCodes.Newarr, typeof(object)));
					// 	// call method that takes one object as an argument
					// Add(new CodeInstruction(OpCodes.Call, myOnGui));
					// 	// Add(new CodeInstruction(OpCodes.Ret));
					//
					// 	// var label = instructions.Select(i => i.labels.FirstOrDefault(l => l.ToString() == "77")).FirstOrDefault();
					// 	// Debug.Log(label); 
					// 	// Add(new CodeInstruction(OpCodes.Leave_S, label));
					// }

					Add(instr);

					// if (instr.operand is MethodInfo mi)
					// {
					// 	if (mi.Name == "BeginVerticalSplit")
					// 	{
					// 		skipping = true;
					// 		continue;
					// 	}
					// }
					// if (index == 2)// instr.opcode == OpCodes.Ret)
					// {
					// 	Debug.Log("INSERT");
					// }
				}

				return newResult;
			}

			private static void LogVariables(IEnumerable<CodeInstruction> instructions)
			{
				foreach (var instr in instructions)
				{
					if (instr.IsStloc())
						Debug.Log("Store: " + instr);
				}
			}

			private static CodeInstruction FindLoadingLocalVariable(int index, IEnumerable<CodeInstruction> instructions)
			{
				foreach (var instr in instructions)
				{
					if (instr.IsLdloc() && instr.operand is LocalBuilder lb)
					{
						if (lb.LocalIndex == index)
						{
							return instr;
						}
					}
				}

				return null;
			}

			private static void OnGUI2(EditorWindow consoleWindow)
			{
				var t = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
				Debug.Assert(t != null);
				try
				{
					LogEntriesAccess.StartGettingEntries();
					
					consoleWindow.GetType().GetField("ms_LVHeight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(consoleWindow, 100);


					var lv = consoleWindow.GetType().GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(consoleWindow);
					// lv.GetType().GetField("row").SetValue(lv, 5);
					// lv.GetType().GetField("totalRows").SetValue(lv, 5);
					// lv.GetType().GetField("rowHeight").SetValue(lv, 5);
					// lv.GetType().GetField("scrollPos").SetValue(lv, new Vector2(0, 10));
					// lv.GetType().GetField("initialRow").SetValue(lv, 1);
					var state = lv.GetType().GetField("ilvState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lv);
					state.GetType().BaseType.GetField("rectHeight").SetValue(state, 100);

					
					if (Event.current.type == EventType.Repaint)
					{
						
						
						
						Debug.Log("HELLO");
						GUI.Label(new Rect(0, -16, Screen.width, 100), "TEST " + LogEntriesAccess.GetCount() + ", " + consoleWindow);
						GUI.Label(new Rect(0, 200, Screen.width, 100), "TEST2");


						// EditorGUILayout.LabelField("TEST");

						if (LogEntriesAccess.GetCount() > 0)
						{
							if (LogEntriesAccess.GetEntry(0, out var log))
							{
								consoleWindow.GetType().GetMethod("SetActiveEntry", BindingFlags.NonPublic | BindingFlags.Instance)
									?.Invoke(consoleWindow, new object[] {log});
							}
						}
					}
					else if(Event.current.type == EventType.Layout)
					{
						// GUILayout.Space(5);
					}
					// if (Event.current.type == EventType.Repaint)
					// {
					// 	Debug.Log(element);
					// }
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
				finally
				{
					LogEntriesAccess.EndGettingEntries();
				}
			}

			private static bool didPrint = false;

			private static void OnGUI(object element)
			{
				if (didPrint) return;
				didPrint = true;
				Debug.Log("HELLO " + Event.current.type + ", " + element);
				// if (Event.current.type == EventType.Repaint)
				// {
				// 	Debug.Log(element);
				// }
			}

			private static class LogEntriesAccess
			{
				public static int StartGettingEntries() => (int) LogEntries.GetMethod("StartGettingEntries").Invoke(null, null);
				public static void EndGettingEntries() => LogEntries.GetMethod("EndGettingEntries").Invoke(null, null);

				public static bool GetEntry(int row, out object outputEntry)
				{
					var logEntry = Activator.CreateInstance(Entry);
					object[] args = {row, logEntry};
					var method = LogEntries.GetMethod("GetEntryInternal");
					var res = (bool) method?.Invoke(null, args);
					outputEntry = args[1];
					return res;
				}

				public static int GetCount()
				{
					return (int) LogEntries.GetMethod("GetCount").Invoke(null, null);
				}

				public static int GetEntryCount(int row)
				{
					return (int) LogEntries.GetMethod("GetEntryCount").Invoke(null, new object[] {row});
				}

				private static Type _logEntries;

				private static Type LogEntries
				{
					get
					{
						if (_logEntries == null) _logEntries = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
						return _logEntries;
					}
				}

				private static Type _entry;

				private static Type Entry
				{
					get
					{
						if (_entry == null) _entry = typeof(Editor).Assembly.GetType("UnityEditor.LogEntry");
						return _entry;
					}
				}
			}
		}
	}
}