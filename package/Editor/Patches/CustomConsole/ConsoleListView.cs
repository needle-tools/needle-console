using System;
using System.Collections.Generic;
using System.IO;
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

					string GetText()
					{
						return "[" + fileName + "]";
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
						text = $"{text.Substring(0, endTimeIndex + 1)} {GetText()}{text.Substring(endTimeIndex + 1)}";
					}

					// this is only for filtering
					text += "\n" + FilterPrefix + fileName;// Path.GetFullPath(filePath);
				}
			}
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
			var sel = Selection.activeObject;
			if (!sel) return;

			void SetFilter(string filter)
			{
				LogEntries.SetFilteringText(FilterPrefix + filter);
				if (Patch_Console.ConsoleWindow)
					Patch_Console.ConsoleWindow.Repaint();
			}

			if (EditorUtility.IsPersistent(sel))
			{
				var path = AssetDatabase.GetAssetPath(sel);
				var file = Path.GetFileName(path);
				if (previousFilter == null)
					previousFilter = LogEntries.GetFilteringText();
				SetFilter(file);
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
						var instanceId = comp.GetInstanceID();
						var path = AssetDatabase.GetAssetPath(instanceId);
						Debug.Log(instanceId + " Found " + path);

						var filter = "t:" + comp.GetType().Name;
						var res = AssetDatabase.FindAssets(filter);
						Debug.Log("assets: " + string.Join("\n", res));
						
						if (!string.IsNullOrEmpty(path))
						{
							var file = Path.GetFileName(path);
							SetFilter(file);
							break;
						}

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