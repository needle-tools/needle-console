using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Needle.Demystify
{
	internal static class SyntaxHighlighting
	{
		// Complex: https://regex101.com/r/rv7QXz/2
		private static readonly List<string> _complexRegexPatterns = new List<string>()
		{
			// @"(?<type>ref|out|async|string|bool|void|object|byte|int|long)",
			@"at ?((?<new>new)|(((?<return_tuple>\(.*\))|((?<async>async)?( ?(?<return_type>.*?))))) )?((?<namespace>.*) ?(\.|\+))?(?<class>.*?)\.(?<method_name>.+?)(?<params>\(.*?\))\+?(((?<func>\(.*?\) => {.*}))|((?<local_func>.*?)\((?<local_func_params>.*)\)))?",
			@"(?<exception>.*?\w*Exception:.+)",
			// @"(?<method_name>\w+?)(?<generic_type><.*?>)(\+)?|((?<empty_brackets>\(\))|(?<params>\(.*?\)))(\+)?(?<lambda>\(.*?\) => {.*?})?",
			// @"(?<type>(<.+?>))",
			// @"((?<constructor>new\s\w+?)[\.\s])",
		};
		
		// Simple: https://regex101.com/r/sWR1X1/2
		private static readonly List<string> _simpleRegexPatterns = new List<string>()
		{
			@"(?<return_type>.*) (?<class>.*)\.(?<method_name>.+?)(?<params>\(.*?\))\+?",
		};
		
		internal static List<string> CurrentPatternsList
		{
			get
			{
				switch (DemystifySettings.instance.SyntaxHighlighting)
				{
					case Highlighting.None:
						return null;
					case Highlighting.Simple:
						return _simpleRegexPatterns;
					case Highlighting.Complex:
						return _complexRegexPatterns;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		internal static void OnSyntaxHighlightingModeHasChanged() => _currentPattern = null;

		private static string _currentPattern;
		private static string CurrentPattern
		{
			get
			{
				if (_currentPattern == null)
				{
					_currentPattern = string.Join("|", CurrentPatternsList);

					if (DemystifySettings.instance.DevelopmentMode)
					{
						// this is just to give patching time to being loaded to add syntax highlighting to this call too :)
						void NextFrame()
						{
							Debug.Log("<b>Patterns</b>: " + _currentPattern);
							EditorApplication.update -= NextFrame;
						}

						EditorApplication.update += NextFrame;
					}
				}
				return _currentPattern;
			}
		}

		
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

		public static void AddSyntaxHighlighting(ref string line)
		{
			var pattern = CurrentPattern;
			AddSyntaxHighlighting(pattern, ref line);
		}

		public static void AddSyntaxHighlighting(string pattern, ref string line)
		{
			if (string.IsNullOrEmpty(pattern)) return;
			
#if !UNITY_2019_4
			static 
#endif
			string Eval(Match m)
			{
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
								m1 =>
								{
									if (replaced) return m1.Value;
									if (m1.Index != group.Index) 
										return m1.Value;
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
				return str;
			}

			line = Regex.Replace(line.TrimStart(), pattern, Eval, RegexOptions.Compiled | RegexOptions.Singleline);
		}
		
		
		

	}
}