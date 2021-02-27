using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

namespace needle.demystify
{
	internal static class SyntaxHighlighting
	{
		// https://regex101.com/r/rv7QXz/2

		private static readonly List<string> patterns = new List<string>()
		{
			// @"(?<type>ref|out|async|string|bool|void|object|byte|int|long)",
			@"at ?((?<new>new)|(((?<return_tuple>\(.*\))|((?<async>async)?( ?(?<return_type>.*?))))) )?((?<namespace>.*) ?(\.|\+))?(?<class>.*?)\.(?<method_name>.+?)(?<params>\(.*?\))\+?(((?<func>\(.*?\) => {.*}))|((?<local_func>.*?)\((?<local_func_params>.*)\)))?",
			@"(?<exception>.*?\w*Exception:.+)",
			// @"(?<method_name>\w+?)(?<generic_type><.*?>)(\+)?|((?<empty_brackets>\(\))|(?<params>\(.*?\)))(\+)?(?<lambda>\(.*?\) => {.*?})?",
			// @"(?<type>(<.+?>))",
			// @"((?<constructor>new\s\w+?)[\.\s])",
		};

		internal static readonly Dictionary<string, string> DefaultTheme = new Dictionary<string, string>()
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

		internal static readonly Dictionary<string, string> CurrentTheme = new Dictionary<string, string>();

		private static readonly Lazy<string> Pattern = new Lazy<string>(() =>
		{
			var allPatterns = string.Join("|", patterns);

			if (DemystifySettings.instance.DevelopmentMode)
			{
				// this is just to give patching time to being loaded to add syntax highlighting to this call too :)
				void NextFrame()
				{
					Debug.Log("<b>Patterns</b>: " + allPatterns);
					EditorApplication.update -= NextFrame;
				}

				EditorApplication.update += NextFrame;
			}
			
			return allPatterns;
		});

		public static void AddSyntaxHighlighting(ref string line)
		{
			AddSyntaxHighlighting(Pattern.Value, ref line);
		}

		public static void AddSyntaxHighlighting(string pattern, ref string line)
		{
			static string Eval(Match m)
			{
				Profiler.BeginSample("Regex Replace Eval");

				if (m.Groups.Count <= 1) return m.Value;
				var str = m.Value;
				var separators = new string[1]; 
				for (var index = m.Groups.Count - 1; index >= 1; index--)
				{
					var @group = m.Groups[index];
					if (string.IsNullOrWhiteSpace(@group.Value) || string.IsNullOrEmpty(@group.Name)) continue;
					if (CurrentTheme.TryGetValue(@group.Name, out var col))
					{
						// check if we have to use regex to replace it
						separators[0] = group.Value;
						var occ = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
						if (occ.Length >= 2)
						{
							var replaced = false;
							str = Regex.Replace(@str, Regex.Escape(@group.Value),
								m =>
								{
									if (replaced) return m.Value;
									replaced = true;
									// return group.Value;
									return "<color=" + col + ">" + @group.Value + "</color>";
								},
								RegexOptions.Compiled);
						}
						else
						{
							str = str.Replace(group.Value, "<color=" + col + ">" + @group.Value + "</color>");
						}
					}
					// else if()
					// {
					// 	if (int.TryParse(@group.Name, out _)) continue;
					// 	if (!string.IsNullOrWhiteSpace(@group.Name) && @group.Name.Length > 1)
					// 		Debug.LogWarning("Missing color entry for " + @group.Name + ", matched for " + @group);
					// }
				}
				Profiler.EndSample();

				return str;
			}

			line = Regex.Replace(line, Pattern.Value, Eval, RegexOptions.Compiled | RegexOptions.Singleline);
		}
		
		
		

	}
}