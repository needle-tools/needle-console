using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class AdvancedLog
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			ConsoleFilter.ClearingCachedData += OnClear;
			ConsoleFilter.CustomAddEntry += CustomAdd;
			ConsoleList.LogEntryContextMenu += OnLogEntryContext;

			var list = AdvancedLogUserSettings.instance.selections;  
			handler = new AdvancedLogHandler(list);
			ConsoleList.RegisterCustomDrawer(handler);
		}

		private static AdvancedLogHandler handler;

		private static void OnLogEntryContext(GenericMenu menu, int itemIndex)
		{
			if (itemIndex < 0) return;
			var log = ConsoleList.CurrentEntries[itemIndex];
			// var content = new GUIContent("Collapse " + Path.GetFileName(log.entry.file) + "::" + log.entry.line);
			var content = new GUIContent("Collapse");
			var contains = TryGetIndex(log.entry, out var index);
			var on = contains && Entries[index].Active;
			menu.AddItem(content, on, () =>
			{
				if (on)
				{
					Entries.RemoveAt(index);
					SaveEntries();
					ConsoleFilter.MarkDirty();
				}
				else
				{
					Select(log.entry);
					SaveEntries();
					ConsoleFilter.MarkDirty();
					// var item = ConsoleList.CurrentEntries[itemIndex];
					// var list = ConsoleLogAdvancedUserSettings.instance.selections;
				}
			});
		}

		private static void OnClear()
		{
			handler.ClearCache();
		}
		
		private static bool CustomAdd(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			if (!DemystifySettings.instance.DynamicGrouping)
			{
				return true;
			}

			using (new ProfilerMarker("Console Log Grouping").Auto())
			{
				return handler.OnHandleLog(entry, row, preview, entries);
			}
		}

		private static void SaveEntries() => AdvancedLogUserSettings.instance.Save();
		private static List<AdvancedLogEntry> Entries => AdvancedLogUserSettings.instance.selections;

		private static bool TryGetIndex(LogEntryInfo entry, out int index)
		{
			for (var i = 0; i < Entries.Count; i++)
			{
				var e = Entries[i];
				if (e.Line == entry.line && e.File == entry.file)
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		private static void Select(LogEntryInfo entry)
		{
			Entries.Add(new AdvancedLogEntry()
			{
				Active = true,
				File = entry.file,
				Line = entry.line
			});
		}
	}
}