using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal class AdvancedLogHandler : ICustomLogDrawer, ICustomLogCollapser
	{
		private readonly float graphHeight = 20;
		private readonly List<AdvancedLogEntry> selectedLogs;

		public AdvancedLogHandler(List<AdvancedLogEntry> selectedLogs)
		{
			this.selectedLogs = selectedLogs;
		}

		public float GetContentHeight(float defaultRowHeight, int totalRows, out uint linesHandled)
		{
			linesHandled = 0;
			return defaultRowHeight;
			// linesHandled = (uint)ConsoleList.CurrentEntries.Count(e => e.groupSize > 0);
			// return linesHandled * graphHeight;
		}

		public bool OnDrawStacktrace(int index,
			string rawText,
			Rect rect,
			string stacktraceWithHyperlinks,
			float stacktraceHeight)
		{
			if (index < 0 || index >= ConsoleList.CurrentEntries.Count) return false;
			var custom = ConsoleList.CurrentEntries[index].groupSize > 0;
			if (!custom) return false;
			var graphRect = new Rect(0, 0, Screen.width, graphHeight);
			var totalHeight = graphRect.height * 2 + stacktraceHeight;
			if (totalHeight > rect.height)
				graphRect.width -= 13; // scrollbar
			if (Event.current.type == EventType.Repaint) DrawGraph(index, graphRect);
			GUILayout.Space(graphHeight);
			return false;
		}

		public bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height)
		{
			height = 0;
			return false;
		}

		private void DrawGraph(int index, Rect rect)
		{
			GUIUtils.SimpleColored.SetPass(0);
			GL.PushMatrix();
			GL.Begin(GL.LINES);
			for (int i = 0; i < rect.width; i++)
			{
				var t = i / rect.width;
				var x = rect.x + t * rect.width;
				GL.Color(Color.Lerp(Color.green, Color.red, t));
				GL.Vertex3(x, rect.y + rect.height, 0);
				var height = Mathf.Max(1, t * rect.height);
				GL.Vertex3(x, rect.y + rect.height - height, 0);
			}

			GL.End();
			GL.PopMatrix();
		}


		public int GetCount(int index)
		{
			return 0;
		}

		
		
		
		// number matcher https://regex101.com/r/D0dFIj/1/
		// non number matcher https://regex101.com/r/VRXwpC/1/
		// private static readonly Regex noNumberMatcher = new Regex(@"[^-\d.]+", RegexOptions.Compiled | RegexOptions.Multiline);

		
		private static readonly Dictionary<string, int> logs = new Dictionary<string, int>();
		private static readonly StringBuilder builder = new StringBuilder();

		internal void ClearCache()
		{
			logs.Clear();
		}

		public bool OnHandleLog(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			for (var i = 0; i < selectedLogs.Count; i++)
			{
				var selected = selectedLogs[i];
				if (!selected.Active) continue;
				if (selected.Line == entry.line && selected.File == entry.file)
				{var text = preview;
					const string timestampEnd = "] ";
					var timestampIndex = text.IndexOf(timestampEnd, StringComparison.Ordinal);
					var timestamp = string.Empty;
					if (timestampIndex > 0)
					{
						timestamp = text.Substring(0, timestampIndex + timestampEnd.Length);
					}

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

					if (logs.TryGetValue(key, out var index))
					{
						var ex = entries[index];
						newEntry.row = ex.row;
						newEntry.groupSize = ex.groupSize + 1;
						// var history = "\n" + ex.str;
						// newEntry.str += history;
						// newEntry.entry.message += history;
						entries[index] = newEntry;
					}
					else
					{
						logs.Add(key, entries.Count);
						entries.Add(newEntry);
					}

					return false;
				}
			}

			return true;
		}
	}
}