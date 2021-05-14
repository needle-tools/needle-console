using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleTextPrefix
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			cachedInfo.Clear();
			
			// clear cache when colors change
			DemystifyProjectSettings.ColorSettingsChanged += () =>
			{
				cachedInfo.Clear();
			};
		}
		
		private static readonly LogEntry tempEntry = new LogEntry();
		

		private static readonly string[] onlyUseMethodNameFromLinesWithout = new[]
		{
			"UnityEngine.UnitySynchronizationContext",
			"UnityEngine.Debug",
			"UnityEngine.Logger",
			"UnityEngine.DebugLogHandler",
			"System.Runtime.CompilerServices"
		};

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
							match = Regex.Match(line, @".*?(\.(?<type_name>.*?)){0,}[\.\:](?<method_name>.*?)\(.*\.cs(:\d{1,})?", RegexOptions.Compiled | RegexOptions.ExplicitCapture); 
						using (new ProfilerMarker("Handle Match").Auto())
						{
							// var match = matches[i];
							var type = match.Groups["type_name"];
							if (type.Success)
							{
								typeName = type.Value.Trim();
							}
							
							var group = match.Groups["method_name"];
							if (group.Success)
							{
								methodName = group.Value.Trim();
								
								// nicify local function names
								const string localPrefix = "g__";
								var localStart = methodName.IndexOf(localPrefix, StringComparison.InvariantCulture);
								if (localStart > 0)
								{
									var sub = methodName.Substring(localStart+localPrefix.Length);
									var localEnd = sub.IndexOf("|", StringComparison.InvariantCulture);
									if (localEnd > 0)
									{
										sub = sub.Substring(0, localEnd);
										if(!string.IsNullOrEmpty(sub))
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

		private static readonly Dictionary<string, string> cachedInfo = new Dictionary<string, string>();

		// called from console list with current list view element and console text
		internal static void ModifyText(ListViewElement element, ref string text)
		{
			// var rect = element.position;
			// GUI.DrawTexture(rect, Texture2D.whiteTexture);//, ScaleMode.StretchToFill, true, 1, Color.red, Vector4.one, Vector4.zero);
			
			using (new ProfilerMarker("ConsoleList.ModifyText").Auto())
			{
				if (!DemystifySettings.instance.ShowFileName) return;
				
				var key = text;
				if (cachedInfo.ContainsKey(key))
				{
					text = cachedInfo[key];
					return;
				}

				if (LogEntries.GetEntryInternal(element.row, tempEntry))
				{
					var filePath = tempEntry.file;
					try
					{
						var fileName = Path.GetFileNameWithoutExtension(filePath);
						const string colorPrefix = "<color=#999999>";
						const string colorPostfix = "</color>";

						var colorKey = fileName;
						var colorMarker = DemystifySettings.instance.ColorMarker; // " ▍";
						if (!string.IsNullOrWhiteSpace(colorMarker))
							LogColor.AddColor(colorKey, ref colorMarker);

						string GetPrefix()
						{
							var str = default(string);
							if (TryGetMethodName(tempEntry.message, out var typeName, out var methodName))
							{
								if (string.IsNullOrWhiteSpace(typeName))
									str = fileName;
								else str = typeName;
								str += "." + methodName;
							}

							if (string.IsNullOrWhiteSpace(str))
							{
								if (string.IsNullOrEmpty(fileName)) return string.Empty;
								str = fileName;
							}
							// str = colorPrefix + "[" + str + "]" + colorPostfix;
							// str = "<b>" + str + "</b>";
							// str = "\t" + str;
							str = $"{colorPrefix} {str} {colorPostfix}"; // + " |";
							return str;
						}

						var endTimeIndex = text.IndexOf("] ", StringComparison.InvariantCulture);

						// text = element.row.ToString();

						// no time:
						if (endTimeIndex == -1)
						{
							// LogColor.AddColor(colorKey, ref text);
							text = $"{colorMarker}{GetPrefix()}{text}";
						}
						// contains time:
						else
						{
							var message = text.Substring(endTimeIndex + 1);
							// LogColor.AddColor(colorKey, ref message);
							text = $"{colorPrefix}{text.Substring(1, endTimeIndex - 1)}{colorPostfix} {colorMarker}{GetPrefix()}{message}";
						}

						cachedInfo.Add(key, text);
					}
					catch (ArgumentException)
					{
						// sometimes filepath contains illegal characters and is not actually a path
						cachedInfo.Add(key, text);
					}
					catch (Exception e)
					{
						Debug.LogException(e);
						cachedInfo.Add(key, text);
					}
				}
			}
		}

	}
}