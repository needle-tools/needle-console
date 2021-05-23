using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal class AdvancedLogHandler : ICustomLogDrawer, ICustomLogCollapser
	{
		private readonly float graphHeight = 60;
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

		private float height;

		public bool OnDrawStacktrace(int index, string rawText, Rect rect)
		{
			if (index < 0 || index >= ConsoleList.CurrentEntries.Count) return false;
			var log = ConsoleList.CurrentEntries[index];
			var custom = log.collapseCount > 0;
			if (!custom) return false;
			var graphRect = new Rect(1, 1, Screen.width, graphHeight);
			// var totalHeight = graphRect.height * 2 + stacktraceHeight;
			// if (totalHeight > rect.height)
			graphRect.width -= 13; // scrollbar
			if (Event.current.type == EventType.Repaint)
			{
				DrawGraph(index, rect, out height);
			}

			GUILayout.Space(height);

			// GUILayout.Space(graphHeight + EditorGUIUtility.singleLineHeight);
			ConsoleList.DrawDefaultStacktrace(log.entry.message);
			return true;
		}

		public bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height)
		{
			height = 0;
			return false;
		}

		private static readonly List<ILogData<float>> floatValues = new List<ILogData<float>>(300);
		private static readonly List<ILogData<Vector3>> vecValues = new List<ILogData<Vector3>>(300);
		private static readonly List<ILogData<string>> values = new List<ILogData<string>>(300);

		private void DrawGraph(int index, Rect rect, out float height)
		{
			height = 0;
			if (!logsData.ContainsKey(index)) return;
			var data = logsData[index];

			// GraphUtils.DrawRect(rect, new Color(0,0,0,.1f));
			// GraphUtils.DrawOutline(rect, new Color(.7f,.7f,.7f,.3f));

			var list = values;
			data.TryGetData(list, 0);

			var str = string.Empty;
			var line = new Rect(0, 0, rect.width, EditorGUIUtility.singleLineHeight);
			for (var i = list.Count - 1; i >= 0; i--)
			{
				var e = list[i];
				GUI.Label(line, e.Value);
				line.y += line.height;
			}

			height = line.y;

			// GraphUtils.DrawGraph(rect, floatValues, -1, 1, Color.white);

			// floatValues.Clear();
			// data.GetData(floatValues, 1);
			// GraphUtils.DrawGraph(rect, floatValues, -1, 1, Color.gray);
		}


		public int GetCount(int index)
		{
			return 0;
		}

		private static readonly Dictionary<string, int> logs = new Dictionary<string, int>();
		private static readonly Dictionary<int, AdvancedLogData> logsData = new Dictionary<int, AdvancedLogData>();

		private static readonly StringBuilder builder = new StringBuilder();

		internal void ClearCache()
		{
			logs.Clear();
			logsData.Clear();
		}

		public bool OnHandleLog(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			for (var i = 0; i < selectedLogs.Count; i++)
			{
				var selected = selectedLogs[i];
				if (!selected.Active) continue;
				if (selected.Line == entry.line && selected.File == entry.file)
				{
					builder.Clear();
					builder.Append(entry.file).Append("::").Append(entry.line);
					if (entry.instanceID != 0) builder.Append("@").Append(entry.instanceID);
					var key = builder.ToString();

					// entry.message += "\n" + UnityDemystify.DemystifyEndMarker;
					var newEntry = new CachedConsoleInfo()
					{
						entry = new LogEntryInfo(entry),
						row = row,
						str = preview,
						collapseCount = 1
					};

					AdvancedLogData data;
					if (logs.TryGetValue(key, out var index))
					{
						var ex = entries[index];
						newEntry.row = ex.row;
						newEntry.collapseCount = ex.collapseCount + 1;
						entries[index] = newEntry;
						data = logsData[index];
					}
					else
					{
						var newIndex = entries.Count;
						logs.Add(key, newIndex);
						entries.Add(newEntry);

						data = new AdvancedLogData();
						logsData.Add(newIndex, data);
					}


					// parse data
					// const string timestampStart = "[";
					const string timestampEnd = "] ";
					var timestampIndex = preview.IndexOf(timestampEnd, StringComparison.Ordinal);
					var messageStart = timestampIndex > 0 ? (timestampIndex + timestampEnd.Length) : 0;
					ParseLogData(data, preview, messageStart);
					return true;
				}
			}

			return false;
		}

		// number matcher https://regex101.com/r/D0dFIj/1/ -> [-\d.]+
		// non number matcher https://regex101.com/r/VRXwpC/1/
		// grouped numbers, separated by ,s https://regex101.com/r/GJVz7t/1 -> [,-.\d ]+
		//TODO: capture arrays

		private static readonly Regex numberMatcher = new Regex(@"[,-.\d ]+", RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly StringBuilder valueBuilder = new StringBuilder();
		private static readonly List<float> vectorValues = new List<float>(4);

		private void ParseLogData(AdvancedLogData data, string text, int startIndex)
		{
			var numbers = numberMatcher.Matches(text, startIndex);
			// id marks the # of value fields in a log message
			var id = 0;

			data.AddData(text, id);
			id += 1;

			foreach (Match match in numbers)
			{
				var str = match.Value;

				if (str.Length == 0) continue;
				// ignore empty spaces
				if (str.Length == 1 && str == " ") continue;

				// vector types:
				const char vectorSeparator = ',';
				if (str.Contains(vectorSeparator))
				{
					// TODO: test format of matrices if we want to support that at some point 
					valueBuilder.Clear();
					vectorValues.Clear();

					void TryParseCollectedValue()
					{
						if (valueBuilder.Length <= 0) return;
						var parsed = valueBuilder.ToString();
						valueBuilder.Clear();
						if (float.TryParse(parsed, out var val))
							vectorValues.Add(val);
					}

					for (var i = 0; i < str.Length; i++)
					{
						var c = str[i];
						switch (c)
						{
							case vectorSeparator:
								TryParseCollectedValue();
								break;
							case ' ':
								continue;
							default:
								valueBuilder.Append(c);
								break;
						}
					}

					TryParseCollectedValue();
					switch (vectorValues.Count)
					{
						case 1:
							data.AddData(vectorValues[0], id);
							break;
						case 2:
							data.AddData(new Vector2(vectorValues[0], vectorValues[1]), id);
							break;
						case 3:
							data.AddData(new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]), id);
							break;
						case 4:
							data.AddData(new Vector4(vectorValues[0], vectorValues[1], vectorValues[2], vectorValues[3]), id);
							break;
					}
				}
				// float types:
				else if (str.Contains("."))
				{
					if (float.TryParse(str, out var fl))
					{
						data.AddData(fl, id);
					}
				}

				// TODO integer or string types


				id += 1;
			}
		}

		private static DateTime playmodeStartTime;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void EnterPlaymode()
		{
			playmodeStartTime = DateTime.Now;
		}
	}
}