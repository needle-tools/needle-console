using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Needle.Demystify
{
	internal static class CustomCollapse
	{
		private class CustomDrawer : ICustomLogDrawer
		{
			public bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height)
			{
				if (!visible)
				{
					height = 0;
					return false;
				}
				var custom = index % 3 == 0;
				const int graphHeight = 40;
				var orig = rect;
				if (custom)
				{
					rect.height += graphHeight;
				}
				height = ConsoleList.DrawDefaultRow(index, rect);

				if (custom)
				{
					var graphRect = new Rect(rect.x, orig.y + orig.height, orig.width, graphHeight);
					GUIUtils.SimpleColored.SetPass(0);
					GL.PushMatrix();
					GL.Begin(GL.LINE_STRIP);
					for (int i = 0; i < 100; i++)
					{
						var t = i / 100f;
						var x = t * graphRect.width;
						var y = t + Random.value * .1f;
						GL.Color(Color.Lerp(Color.green, Color.red, t));
						GL.Vertex3(x, graphRect.y + graphRect.height - graphHeight * y, 0);
					}

					GL.End();
					GL.PopMatrix();
				}

				return true;
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			ConsoleFilter.ClearingCachedData += OnClear;
			ConsoleFilter.CustomAddEntry += CustomAdd;
			ConsoleList.LogEntryContextMenu += OnLogEntryContext;
			ConsoleList.RegisterCustomDrawer(new CustomDrawer());
		}

		private static void OnLogEntryContext(GenericMenu menu, int itemIndex)
		{
			if (itemIndex <= 0) return;
			var item = ConsoleList.CurrentEntries[itemIndex];
			Debug.Log(item.str);
			menu.AddItem(new GUIContent(item.str.SanitizeMenuItemText()), false, () => { });
		}

		private static void OnClear()
		{
			groupedLogs.Clear();
			collapsed.Clear();
		}

		private static readonly Dictionary<string, int> groupedLogs = new Dictionary<string, int>();
		private static readonly Dictionary<int, CollapseData> collapsed = new Dictionary<int, CollapseData>();

		private class CollapseData
		{
		}

		private static readonly StringBuilder builder = new StringBuilder();

		// number matcher https://regex101.com/r/D0dFIj/1/
		// non number matcher https://regex101.com/r/VRXwpC/1/
		// private static readonly Regex noNumberMatcher = new Regex(@"[^-\d.]+", RegexOptions.Compiled | RegexOptions.Multiline);

		private static bool CustomAdd(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			if (!DemystifySettings.instance.DynamicGrouping)
			{
				return true;
			}

			using (new ProfilerMarker("Console Log Grouping").Auto())
			{
				var text = preview;
				const string marker = "<group>";
				var start = text.IndexOf(marker, StringComparison.InvariantCulture);
				if (start <= 0) return true;
				const string timestampEnd = "] ";
				var timestampIndex = text.IndexOf(timestampEnd, StringComparison.Ordinal);
				var timestamp = string.Empty;
				if (timestampIndex < start)
				{
					timestamp = text.Substring(0, timestampIndex + timestampEnd.Length);
				}


				// var match = noNumberMatcher.Match(text);
				text = text.Substring(start + marker.Length).TrimStart();

				builder.Clear();
				var key = builder.Append(entry.file).Append("::").Append(entry.line).Append("::").ToString();
				builder.Clear();


				text = builder.Append(timestamp).Append(text).ToString();

				entry.message += "\n" + UnityDemystify.DemystifyEndMarker;
				var newEntry = new CachedConsoleInfo()
				{
					entry = new LogEntryInfo(entry),
					row = row,
					str = text,
					groupSize = 1
				};

				if (groupedLogs.TryGetValue(key, out var index))
				{
					var ex = entries[index];
					newEntry.row = ex.row;
					newEntry.groupSize = ex.groupSize + 1;
					var history = "\n" + ex.str;
					newEntry.str += history;
					newEntry.entry.message += history;
					entries[index] = newEntry;
				}
				else
				{
					groupedLogs.Add(key, entries.Count);
					collapsed.Add(entries.Count, new CollapseData());
					entries.Add(newEntry);
				}

				return false;
			}
		}
	}
}