using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal class AdvancedLogCollapse : ICustomLogCollapser
	{
		internal static IReadOnlyDictionary<int, AdvancedLogData> LogsData => logsData;

		private readonly List<AdvancedLogEntry> selectedLogs;

		public AdvancedLogCollapse(List<AdvancedLogEntry> selectedLogs)
		{
			this.selectedLogs = selectedLogs;
		}
		
		private static readonly Dictionary<string, int> logs = new Dictionary<string, int>();
		private static readonly Dictionary<int, AdvancedLogData> logsData = new Dictionary<int, AdvancedLogData>();

		private static readonly StringBuilder builder = new StringBuilder();

		internal void ClearCache()
		{
			logs.Clear();
			logsData.Clear();
		}

		private static readonly Dictionary<string, string[]> fileContent = new Dictionary<string, string[]>();
		private static readonly List<int> fileMatchesButDidntFind = new List<int>();


		public bool OnHandleLog(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			fileMatchesButDidntFind.Clear();
			for (var i = 0; i < selectedLogs.Count; i++)
			{
				var selected = selectedLogs[i];
				if (!selected.Active) continue;
				var collapse = selected.Line == entry.line && selected.File == entry.file;
				
				// cache file lines to try to recover entry
				if (!fileContent.ContainsKey(entry.file))
				{
					if (File.Exists(entry.file))
						fileContent.Add(entry.file, File.ReadAllLines(entry.file));
					else fileContent.Add(entry.file, null);
				}

				if (!collapse)
				{
					if (selected.File == entry.file) fileMatchesButDidntFind.Add(i);
					continue;
				}

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


				var content = fileContent[entry.file];
				if (content != null && content.Length > selected.Line)
				{
					var lineStr = content[selected.Line];
					var changed = selected.LineString == lineStr;
					selected.LineString = lineStr;
					selectedLogs[i] = selected;
					if(changed) AdvancedLog.SaveEntries();
				}

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

				var id = 0;
				data.AddData(preview, id++);

				// parse data
				// const string timestampStart = "[";
				// const string timestampEnd = "] ";
				// var timestampIndex = preview.IndexOf(timestampEnd, StringComparison.Ordinal);
				// var messageStart = timestampIndex > 0 ? (timestampIndex + timestampEnd.Length) : 0;
				// ParseLogData(data, preview, messageStart, id);
				return true;
			}

			
			if (fileMatchesButDidntFind.Count > 0)
			{
				foreach (var index in fileMatchesButDidntFind)
				{
					var selection = selectedLogs[index];
					if (!string.IsNullOrEmpty(selection.LineString))
					{
						if (fileContent.TryGetValue(selection.File, out var lines))
						{
							if (lines == null) continue;
							for (var i = Mathf.Max(0, selection.Line - 50); i < lines.Length && i < selection.Line + 50; i++)
							{
								var line = lines[i];
								if (line == selection.LineString)
								{
									// recovered line
									Debug.Log("FOUND LINE " + selection.Line + " new " + i + ", " + line);
									selection.Line = i;
									AdvancedLog.SaveEntries();
									return true;
								}
							}
						}
					}
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

		private void ParseLogData(AdvancedLogData data, string text, int startIndex, int id)
		{
			var numbers = numberMatcher.Matches(text, startIndex);
			// id marks the # of value fields in a log message

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
	}
}