using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleListView
	{
		private static readonly LogEntry tempEntry = new LogEntry();
		private const string FilterPrefix = "";

		// called from console list with current list view element and console text
		internal static void ModifyText(ListViewElement element, ref string text)
		{
			if (!DemystifySettings.instance.ShowFileName) return;

			// LogEntries.SetFilteringText("PatchManager");
			if (LogEntries.GetEntryInternal(element.row, tempEntry))
			{
				var filePath = tempEntry.file;
				if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
				{
					var fileName = Path.GetFileNameWithoutExtension(filePath);
					const string colorPrefix = "<color=grey>";
					const string colorPostfix = "</color>";

					string GetText()
					{
						return colorPrefix + "[" + fileName + "]" + colorPostfix;
					}

					var endTimeIndex = text.IndexOf("] ", StringComparison.InvariantCulture);
					// no time:
					if (endTimeIndex == -1)
					{
						text = $"{GetText()} {text}";
					}
					// contains time:
					else
					{
						text = $"{colorPrefix}{text.Substring(0, endTimeIndex + 1)}{colorPostfix} {GetText()}{text.Substring(endTimeIndex + 1)}";
					}


					if (DemystifySettings.instance.AutoFilter)
					{
						// text = element.row + " - " + text;

						// this is only for filtering
						var filter = Path.GetFullPath(filePath); // Path.GetDirectoryName(filePath) + fileName;
						// path = path.Substring((int)(path.Length * .5f));
						filter = FilterPrefix + MakeFilterable(filter);
						// text = filter;
						// return;

						// many spaces to hide the search match highlight for invisible text
						text +=
							$"\n                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                {FilterPrefix}{MakeFilterable(filter)}";
					}
				}
			}
		}

		private static string MakeFilterable(string str)
		{
			str = str.Replace("\\", "/");
			// str = str.Replace("/", ".");
			str = str.Replace(":", string.Empty);
			return str;
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			Selection.selectionChanged += OnSelectionChanged;
		}

		private static string previousFilter;
		private static readonly List<Component> tempComponents = new List<Component>();

		private static void OnSelectionChanged()
		{
			if (!DemystifySettings.instance || !DemystifySettings.instance.AutoFilter) return;
			
			var sel = Selection.activeObject;
			if (!sel) return;

			void SetFilter(string filter)
			{
				const int maxLength = 50;
				if (filter.Length > maxLength)
					filter = filter.Substring(filter.Length - maxLength);
				// filter = MakeFilterable(filter);
				LogEntries.SetFilteringText(FilterPrefix + filter);
				if (Patch_Console.ConsoleWindow)
				{
					Patch_Console.ConsoleWindow.GetType().GetField("m_SearchText", AccessTools.allDeclared).SetValue(Patch_Console.ConsoleWindow, filter);
					Patch_Console.ConsoleWindow.Repaint();
				}
			}

			if (EditorUtility.IsPersistent(sel))
			{
				var path = AssetDatabase.GetAssetPath(sel);
				path = Path.GetFullPath(path);
				path = MakeFilterable(path);
				// path = Path.GetFileName(path);
				// path = path.Substring((int)(path.Length * .5f));
				// var file = Path.GetFileName(path);
				if (previousFilter == null)
					previousFilter = LogEntries.GetFilteringText();
				SetFilter(path);
			}
			else
			{
				if (sel is GameObject go)
				{
					tempComponents.Clear();
					go.GetComponents(tempComponents);
					foreach (var comp in tempComponents)
					{
						if (comp is Transform) continue;
						// var instanceId = comp.GetInstanceID();
						// var path = AssetDatabase.GetAssetPath(instanceId);
						// // Debug.Log(instanceId + " Found " + path);

						var filter = comp.GetType().Name;
						var res = AssetDatabase.FindAssets(filter);
						foreach (var guid in res)
						{
							var path = AssetDatabase.GUIDToAssetPath(guid);
							if (path.EndsWith(".cs"))
							{
								path = MakeFilterable(path);
								// Debug.Log(file);
								SetFilter(path);
							}
						}
						// Debug.Log("assets: " + string.Join("\n", res));
						//
						// if (!string.IsNullOrEmpty(path))
						// {
						// 	Debug.Log(file);
						// 	break;
						// }

						// SetFilter(string.Empty);
					}
				}
			}
		}

		// private static void EditorUpdate()
		// {
		// 	var sel = Selection.activeObject;
		// 	if (!sel)
		// 	{
		// 		return;
		// 	}
		// 	LogEntries.SetFilteringText("PortalVisibility"); 
		// }
	}
}