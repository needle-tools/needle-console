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
			@"(?<exception>.*?\w*Exception:.+)",
			// @"(?<method_name>\w+?)(?<generic_type><.*?>)(\+)?|((?<empty_brackets>\(\))|(?<params>\(.*?\)))(\+)?(?<lambda>\(.*?\) => {.*?})?",
			// @"(?<type>(<.+?>))",
			// @"((?<constructor>new\s\w+?)[\.\s])",
		};

		private static readonly Dictionary<string, string> theme = new Dictionary<string, string>()
		{
			{"new", "#F5D96A"},
			{"async", "#63FFF2"},
			{"return_tuple", "#63FFF2"},
			{"return_type", "#63FFF2"},
			{"namespace", "#B3B3B3"},
			{"class", "#FFFFFF"},
			{"method_name", "#63FFF2"},
			{"params", "#63FFF2"},
			{"func", "#B09BDD"},
			{"local_func", "#B09BDD"},
			{"local_func_params", "#B09BDD"},
			{"exception", "#ff3333"},
		};

		private static readonly Lazy<string> Pattern = new Lazy<string>(() =>
		{
			var allPatterns = string.Join("|", patterns);
			Debug.Log("<b>Patterns</b>: " + allPatterns);
			return allPatterns;
		});


		private static void AddBasicSyntaxHighlighting(ref string line)
		{
			static string Eval(Match m)
			{
				var str = m.Value;
				
				if (m.Groups.Count <= 1) return str;
				for (var index = m.Groups.Count - 1; index >= 1; index--)
				{
					var @group = m.Groups[index];
					if (string.IsNullOrWhiteSpace(@group.Value) || string.IsNullOrEmpty(@group.Name)) continue;
					if (theme.TryGetValue(@group.Name, out var col))
					{
						var replaced = false;
						str = Regex.Replace(@str, Regex.Escape(@group.Value),
							m =>
							{
								if (replaced) return m.Value;
								replaced = true;
								return "<color=" + col + ">" + @group.Value + "</color>";
							},
							RegexOptions.Compiled);
					}
#if UNITY_DEMYSTIFY_DEV
					else
					{
						if (int.TryParse(@group.Name, out _)) continue;
						if (!string.IsNullOrWhiteSpace(@group.Name) && @group.Name.Length > 1)
							Debug.LogWarning("Missing color entry for " + @group.Name + ", matched for " + @group);
					}
#endif
				}

				return str;
			}

			line = Regex.Replace(line, Pattern.Value, Eval, RegexOptions.Compiled | RegexOptions.Singleline);
		}
	}
}