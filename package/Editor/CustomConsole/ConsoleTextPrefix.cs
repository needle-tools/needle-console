using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleTextPrefix
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			ClearCaches();

			// clear cache when colors change
			NeedleConsoleProjectSettings.ColorSettingsChanged += ClearCaches;
			NeedleConsoleSettings.Changed += ClearCaches;
			EditorApplication.playModeStateChanged += mode => ClearCaches();
		}

		private static void ClearCaches()
		{
			cachedInfo.Clear();
			cachedPrefix.Clear();
		}

		private static readonly LogEntry tempEntryForOriginalModifyText = new LogEntry(); // Renamed to avoid conflict if ModifyTextInternal uses a local var named tempEntry

		private static readonly string[] onlyUseMethodNameFromLinesWithout = new[]
		{
			"UnityEngine.UnitySynchronizationContext",
			"UnityEngine.Debug",
			"UnityEngine.Logger",
			"UnityEngine.DebugLogHandler",
			"System.Runtime.CompilerServices"
		};

		private static readonly Dictionary<string, string> cachedInfo = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> cachedPrefix = new Dictionary<string, string>();
		private static readonly StringBuilder keyBuilder = new StringBuilder();

		internal static void ModifyTextInternal(LogEntry entry, int entryRow, ref string text)
		{
			// entry is used directly instead of LogEntries.GetEntryInternal(element.row, tempEntry)
			// entryRow is used instead of element.row

			using (new ProfilerMarker("ConsoleList.ModifyTextInternal").Auto())
			{
				var settings = NeedleConsoleSettings.instance;
				if (!settings.ShowLogPrefix && (string.IsNullOrWhiteSpace(settings.ColorMarker) || !settings.UseColorMarker))
				{
					return;
				}

				// Removed: if (!LogEntries.GetEntryInternal(element.row, tempEntry)) { return; }
				// Assuming 'entry' is valid if this method is called. 
				// If 'entry' can be null or invalid, checks might be needed here.

				keyBuilder.Clear();
				keyBuilder.Append(entry.file).Append(entry.line).Append(entry.column).Append(entry.mode);
				
#if UNITY_2021_2_OR_NEWER
				if(string.IsNullOrWhiteSpace(entry.file))
					keyBuilder.Append(entry.identifier).Append(entry.globalLineIndex);
#else
				// Original logic used element.row when file info was missing. Using entryRow here.
				if (entry.file == "" && entry.line == 0 && entry.column == -1) 
				{
					keyBuilder.Append(entryRow);
				}
#endif
				
				var key = keyBuilder.Append(text).ToString();
				var isSelected = ConsoleList.IsSelectedRow(entryRow); // Use entryRow
				var cacheEntry = !isSelected;
				var isInCache = cachedInfo.ContainsKey(key);
				if (cacheEntry && isInCache)
				{
					using (new ProfilerMarker("ConsoleList.ModifyTextInternal cached").Auto())
					{
						text = cachedInfo[key];
						if (NeedleConsoleSettings.DevelopmentMode)
						{
							text += " \t<color=#ff99ff>CacheKey: " + key + "</color>";
						}
						return;
					}
				}

				using (new ProfilerMarker("ConsoleList.ModifyTextInternal (Not in cache)").Auto())
				{
					try
					{
						var filePath = entry.file;
						var fileName = Path.GetFileNameWithoutExtension(filePath);
						const string colorPrefixDefault = "<color=#888888>";
						const string colorPrefixSelected = "<color=#ffffff>";
						var colorPrefix = isInCache && isSelected ? colorPrefixSelected : colorPrefixDefault; // Corrected: was always colorPrefixDefault before due to cache check placement
						const string colorPostfix = "</color>";

						string GetPrefix()
						{
							if (fileName == "Debug.bindings") return "";
							if (!NeedleConsoleSettings.instance.ShowLogPrefix) return string.Empty;
							keyBuilder.Clear();
							keyBuilder.Append(entry.file).Append(entry.line).Append(entry.column).Append(entry.mode);
							keyBuilder.Append(entry.message); 
#if UNITY_2021_2_OR_NEWER
							if(string.IsNullOrWhiteSpace(entry.file))
								keyBuilder.Append(entry.identifier).Append(entry.globalLineIndex);
#else
							// Original logic used element.row when file info was missing. Using entryRow here.
							if (entry.file == "" && entry.line == 0 && entry.column == -1)
							{
								keyBuilder.Append(entryRow); 
							}
#endif
							var key2 = keyBuilder.ToString();
							if (!isSelected && cachedPrefix.TryGetValue(key2, out var cached))
							{
								return cached;
							}

							var str = default(string);
							// Pass entry.message to TryGetMethodName
							if (TryGetMethodName(entry.message, out var typeName, out var methodName))
							{
								if (string.IsNullOrWhiteSpace(typeName))
									str = fileName;
								else str = typeName;
								str += "." + methodName;
							}
							else if (!isSelected && cachedPrefix.TryGetValue(key2, out cached)) // Check cache again if TryGetMethodName failed
							{
								return cached;
							}

							if (string.IsNullOrWhiteSpace(str))
							{
								if (string.IsNullOrEmpty(fileName)) return string.Empty;
								str = fileName;
							}

							if (entry.line > 0)
								str += ":" + entry.line;
							
							str = PrefixFormat(str); // Renamed Prefix to PrefixFormat to avoid conflict with parameter name
							if (cacheEntry)
							{
								if (!cachedPrefix.ContainsKey(key2))
									cachedPrefix.Add(key2, str);
								else cachedPrefix[key2] = str;
							}
							return str;

							string PrefixFormat(string s) => $"{colorPrefix} {s} {colorPostfix}";
						}
						
						var endTimeIndex = text.IndexOf("] ", StringComparison.InvariantCulture);
						var calculatedPrefix = GetPrefix(); // Renamed from prefix to avoid conflict

						var colorKey = string.IsNullOrWhiteSpace(fileName) ? calculatedPrefix : fileName;
						var colorMarker = settings.UseColorMarker ? NeedleConsoleSettings.instance.ColorMarker : string.Empty;
						if (settings.UseColorMarker && !string.IsNullOrWhiteSpace(colorMarker))
							LogColor.CalcLogColor(colorKey, ref colorMarker);
						
						// Pass 'entry' to RemoveFilePathInCompilerErrorMessages
						RemoveFilePathInCompilerErrorMessages(ref text, entry); 
						
						if (endTimeIndex == -1)
						{
							text = $"{colorMarker}{calculatedPrefix}{text}";
						}
						else
						{
							var message = text.Substring(endTimeIndex + 1);
							// Pass 'entry' to RemoveFilePathInCompilerErrorMessages for the message part too
							RemoveFilePathInCompilerErrorMessages(ref message, entry);
							text = $"{colorPrefix}{text.Substring(1, endTimeIndex - 1)}{colorPostfix} {colorMarker}{calculatedPrefix}{message}";
						}

						if (cacheEntry)
						{
							if (!cachedInfo.ContainsKey(key))
								cachedInfo.Add(key, text);
							else cachedInfo[key] = text;
						}
					}
					catch (ArgumentException)
					{
						if (cacheEntry)
							cachedInfo.Add(key, text);
					}
					catch (Exception e)
					{
						Debug.LogException(e);
						if (cacheEntry)
							cachedInfo.Add(key, text);
					}
				}
			}
		}

		// called from console list with current list view element and console text
		internal static void ModifyText(ListViewElement element, ref string text)
		{
			// This method now acts as a wrapper for ModifyTextInternal
			if (!LogEntries.GetEntryInternal(element.row, tempEntryForOriginalModifyText))
			{
				return;
			}
			ModifyTextInternal(tempEntryForOriginalModifyText, element.row, ref text);
		}

		private static readonly Regex findEndOfFilePathRegex = new Regex(@"(\(\d{1,4},\d{1,3}\):)", RegexOptions.Compiled);

		// Modified to accept LogEntry
		private static void RemoveFilePathInCompilerErrorMessages(ref string str, LogEntry entry)
		{
			const ConsoleWindow.Mode modestoRemovePath = ConsoleWindow.Mode.ScriptCompileError
			                                         | ConsoleWindow.Mode.GraphCompileError
			                                         | ConsoleWindow.Mode.ScriptingWarning
			                                         | ConsoleWindow.Mode.ScriptCompileWarning 
			                                         | ConsoleWindow.Mode.AssetImportWarning
				;

			// tempEntry is no longer a class field for this method's direct use, using passed 'entry'
			if (ConsoleList.HasMode(entry.mode, modestoRemovePath)) 
			{
				var match = findEndOfFilePathRegex.Match(str);
				if (match.Success)
				{
					str = str.Substring(match.Index + match.Length).TrimStart();
				}
			}
		}

		private static bool TryGetMethodName(string message, out string typeName, out string methodName)
		{
			using (new ProfilerMarker("ConsoleList.ParseMethodName").Auto())
			{
				typeName = null;
				using (var rd = new StringReader(message))
				{
					var linesRead = 0;
					while (true)
					{
						var line = rd.ReadLine();
						if (line == null) break;
						if (onlyUseMethodNameFromLinesWithout.Any(line.Contains)) continue;
						if (!line.Contains(".cs")) continue;
						Match match;
						using (new ProfilerMarker("Regex").Auto())
							match = Regex.Match(line, @"([\. ](?<type_name>[\w\+]+?)){0,}[\.\:](?<method_name>\w+?)\(.+\.cs(:\d{1,})?",
								RegexOptions.Compiled | RegexOptions.ExplicitCapture);
						using (new ProfilerMarker("Handle Match").Auto())
						{
							var type = match.Groups["type_name"];
							if (type.Success)
							{
								typeName = type.Value.Trim();
							}

							var group = match.Groups["method_name"];
							if (group.Success)
							{
								methodName = group.Value.Trim();
								const string localPrefix = "g__";
								var localStart = methodName.IndexOf(localPrefix, StringComparison.InvariantCulture);
								if (localStart > 0)
								{
									var sub = methodName.Substring(localStart + localPrefix.Length);
									var localEnd = sub.IndexOf("|", StringComparison.InvariantCulture);
									if (localEnd > 0)
									{
										sub = sub.Substring(0, localEnd);
										if (!string.IsNullOrEmpty(sub))
											methodName = sub;
									}
								}
								return true;
							}
						}
						linesRead += 1;
						if (linesRead > 15) break;
					}
				}
				methodName = null;
				return false;
			}
		}
	}
}