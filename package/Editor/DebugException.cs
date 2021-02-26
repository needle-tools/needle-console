#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace Demystify.DebugPatch
{
	[HarmonyPatch(typeof(Exception))]
	// ReSharper disable once UnusedType.Global
	public class DemystifyExceptions
	{
		[HarmonyPostfix]
		[HarmonyPatch("GetStackTrace")]
		private static void Postfix(object __instance, ref string __result)
		{
			if (__instance is Exception ex)
			{
				__result = ex.ToStringDemystified();
			}

			try
			{
				var str = "";
				var lines = __result.Split('\n');
				for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];
					var path = FixHyperlinks(ref line);
					AddBasicSyntaxHighlighting(ref line);
					str += line;
					str += ")" + path;
					str += "\n";
				}

				__result = str;
			}
			catch (Exception e)
			{
				// IGNORE
				Debug.LogWarning(e.Message);
			}
		}

		private static readonly Regex hyperlinks = new Regex(@"((?<brackets>\))?(?<prefix> in) (?<file>.*?):line (?<line>\d+)(?<post>.*))",
			RegexOptions.Compiled | RegexOptions.Multiline);

		private static string FixHyperlinks(ref string line)
		{
			var match = hyperlinks.Match(line);
			if (match.Success)
			{
				var file = match.Groups["file"];
				var lineNr = match.Groups["line"];
				var brackets = match.Groups["brackets"].ToString();
				if (string.IsNullOrEmpty(brackets)) brackets = ")";
				var post = match.Groups["post"];

				var end = line.Substring(match.Index, line.Length - match.Index);
				line = line.Remove(match.Index, end.Length) + brackets;
				var newString = " (at " + file + ":" + lineNr + ")\n";
				if (post.Success)
					newString += post;
				return newString;
				// line = line.Replace(match.Value, newString);
			}

			return "";
		}

		private static readonly List<string> patterns = new List<string>()
		{
			// https://regex101.com/r/rv7QXz/2
			// @"(?<type>ref|out|async|string|bool|void|object|byte|int|long)",
			@"at ((?<new>new)|(((?<return_tuple>\(.*\))|((?<async>async)?( ?(?<return_type>.*?))))) )?((?<namespace>.*) ?(\.|\+))?(?<class>.*?)\.(?<method_name>.+?)(?<params>\(.*?\))\+?(((?<func>\(.*?\) => {.*}))|((?<local_func>.*?)\((?<local_func_params>.*)\)))?",
			// @"(?<exception>.*?\w*Exception:.+)",
			// @"(?<method_name>\w+?)(?<generic_type><.*?>)(\+)?|((?<empty_brackets>\(\))|(?<params>\(.*?\)))(\+)?(?<lambda>\(.*?\) => {.*?})?",
			// @"(?<type>(<.+?>))",
			// @"((?<constructor>new\s\w+?)[\.\s])",
		};

		private static readonly Dictionary<string, string> theme = new Dictionary<string, string>()
		{
			{"new", "#ffcc99"},
			{"return_tuple", "#66ffff"},
			{"async", "#ff8888"},
			{"return_type", "#66ffff"},
			{"namespace", "#999999"},
			{"class", "#669955"},
			{"method_name", "#33ff88"},
			{"params", "#66ffff"},
			{"func", "#55ff33"},
			{"local_func", "#ffff00"},
			{"local_func_params", "#ffff00"},
		};

		private static readonly Lazy<string> Pattern = new Lazy<string>(() =>
		{
			var allPatterns = string.Join("|", patterns);
			Debug.Log("<b>Patterns</b>: " + allPatterns);
			return allPatterns;
		});

		// private static List<string> cache = new List<string>();

		private static void AddBasicSyntaxHighlighting(ref string line)
		{
			static string Eval(Match m)
			{
				var str = m.Value;
				// var l = "";


				if (m.Groups.Count > 1)
				{
					// Debug.Log(m.Groups[0]);
					// int inserted = 0;
					for (var index = m.Groups.Count - 1; index >= 1; index--)
					{
						var @group = m.Groups[index];
						// Debug.Log(str + " -> " + @group.Value + " - " + @group.Name);
						if (string.IsNullOrWhiteSpace(@group.Value) || string.IsNullOrEmpty(group.Name)) continue;
						// skip numbers only
						// if (str.Substring(group.Index, group.Length).Contains("<color=")) continue;
						// if(!l.Contains(group.Value))
						// 	l = group + l;
						if (theme.TryGetValue(@group.Name, out var col))
						{
							// var start = group.Index - inserted;
							// if (start < 0) continue;
							// var portion = str.Substring(start, group.Length);
							// Debug.Assert(portion == group.Value);
							var prefix = "<color=" + col + ">";
							var postfix = "</color>";
							// str = str.Substring(0, start) + prefix + portion + postfix + str.Substring(start + portion.Length);
							// inserted += prefix.Length + postfix.Length;
							str = str.Replace(@group.Value, prefix + @group.Value + postfix);
						}
#if UNITY_DEMYSTIFY_DEV
						else
						{
							if (int.TryParse(group.Name, out _)) continue;
							if (!string.IsNullOrWhiteSpace(group.Name) && group.Name.Length > 1)
								Debug.LogWarning("Missing color entry for " + @group.Name + ", matched for " + @group);
						}
#endif
					}
				}

				// if (!cache.Contains(l))
				// {
				// 	cache.Add(l);
				// 	Debug.Log(l);
				// }

				return str;
			}

			line = Regex.Replace(line, Pattern.Value, Eval, RegexOptions.Compiled | RegexOptions.Singleline);
		}
	}
}