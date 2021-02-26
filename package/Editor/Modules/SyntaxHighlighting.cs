using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Demystify.DebugPatch
{
	internal static class SyntaxHighlighting
	{
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
			// this is just to give patching time to being loaded to add syntax highlighting to this call too :)
			void NextFrame()
			{
				Debug.Log("<b>Patterns</b>: " + allPatterns);
				EditorApplication.update -= NextFrame;
			}
			EditorApplication.update += NextFrame;
			return allPatterns;
		});

		public static void AddSyntaxHighlighting(ref string line)
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