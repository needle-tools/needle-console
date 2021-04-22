using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

namespace Needle.Demystify
{
	public class Patch_ConsoleWindow : EditorPatchProvider
	{
		public override string Description => "Custom Console List View";

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

			private static Vector2 scroll;
			private static GUIStyle Box = "CN Box";

			// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L475
			private static bool Prefix(ConsoleWindow __instance, ListViewState ___m_ListView)
			{
				var e = Event.current;
				var m_ListView = ___m_ListView;
				int id = GUIUtility.GetControlID(0);
				using (new GettingLogEntriesScope(m_ListView))
				{
					int selectedRow = -1;
					bool openSelectedItem = false;
					bool collapsed = false;// HasFlag(ConsoleFlags.Collapse);
					var multiSelection = 0;// ListViewOptions.wantsRowMultiSelection;
					var tempContent = new GUIContent();
					foreach (ListViewElement el in ListViewGUI.ListView(m_ListView, Box))
					{
						
						if (e.type == EventType.Repaint)
						{
							int mode = 0;
							string text = null;
							LogEntries.GetLinesAndModeFromEntryInternal(el.row, 1, ref mode, ref text);
							
							
							tempContent.text = text;
							GUIStyle errorModeStyle =ConsoleWindow.GetStyleForErrorMode(mode, false, true);
							var textRect = el.position;
							// textRect.x += offset;
							
							errorModeStyle.Draw(textRect, tempContent, id, m_ListView.row == el.row);
						}

					}
				}

				// try
				// {
				// 	LogEntries.StartGettingEntries();
				// 	scroll = EditorGUILayout.BeginScrollView(scroll);
				// 	GUILayout.Space(5);
				// 	for (var i = 0; i < LogEntries.GetCount(); i++)
				// 	{
				// 		var entry = new LogEntry();
				// 		if (LogEntries.GetEntryInternal(i, entry))
				// 		{
				// 			var msg = entry.message;
				// 			EditorGUILayout.LabelField(entry.file + " - " + LogHelper.GetLines(msg));
				// 		}
				// 	}
				// 	EditorGUILayout.EndScrollView();
				// }
				// finally
				// {
				// 	LogEntries.EndGettingEntries();
				// }
				return false;
			}

			private static class LogHelper
			{
				public static string GetLines(string message)
				{
					var line = message.IndexOf("\n");
					var sub = message.Substring(0, line);
					return sub;
				}
			}
		}
	}
}