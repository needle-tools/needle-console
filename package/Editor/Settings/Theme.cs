using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class Theme
	{
		internal static readonly Dictionary<string, string> DefaultThemeDark = new Dictionary<string, string>()
		{
			{"new", "#C0FF34"},
			{"async", "#C9D996"},
			{"return_tuple", "#C9D996"},
			{"return_type", "#C9D996"},
			{"namespace", "#FFFFFF"},
			{"class", "#9C9C9C"},
			{"method_name", "#C9D996"},
			{"params", "#C9D996"},
			{"func", "#FBAEFF"},
			{"local_func", "#FBAEFF"},
			{"local_func_params", "#FBAEFF"},
			{"exception", "#FF3636"},
			{"link", "#699AFF"}
		};

		internal static readonly Dictionary<string, string> DefaultThemeLight = new Dictionary<string, string>()
		{
			{"new", "#C600FF"},
			{"async", "#6C1FB0"},
			{"return_tuple", "#6C1FB0"},
			{"return_type", "#6C1FB0"},
			{"namespace", "#000000"},
			{"class", "#000000"},
			{"method_name", "#6C1FB0"},
			{"params", "#6C1FB0"},
			{"func", "#000000"},
			{"local_func", "#000000"},
			{"local_func_params", "#000000"},
			{"exception", "#EE0303"},
			{"link", "#008FB7"}
		};

		public string Name;
		public List<Entry> Entries;
		public bool IsDefault => Name == DefaultThemeName;
		public const string DefaultThemeName = "Default";


		public Theme(string name) => Name = name;

		internal void SetActive()
		{
			if (this.Entries.Count >= 0)
			{
				SyntaxHighlighting.CurrentTheme.Clear();
				foreach (var entry in this.Entries)
				{
					if (entry.Color.a <= 0.01f) continue;
					var html = ColorUtility.ToHtmlStringRGB(entry.Color);
					if (string.IsNullOrEmpty(html)) continue;
					if (!html.StartsWith("#")) html = "#" + html;
					SyntaxHighlighting.CurrentTheme.Add(entry.Key, html);
				}

				isDirty = false;
			}
		}

		internal bool isDirty;

		internal bool EnsureEntries()
		{
			if (Entries == null)
				Entries = new List<Entry>();
			var changed = false;
			foreach (var kvp in EditorGUIUtility.isProSkin ? DefaultThemeDark : DefaultThemeLight)
			{
				var token = kvp.Key;
				var hex = kvp.Value;
				if (Entries.Any(e => e.Key == token)) continue;
				ColorUtility.TryParseHtmlString(hex, out var color);
				Entries.Add(new Entry(token, color));
				changed = true;
			}

			isDirty |= changed;
			return isDirty;
		}


		[Serializable]
		public class Entry
		{
			public string Key;
			public Color Color;

			public Entry(string key, Color col)
			{
				this.Key = key;
				this.Color = col;
			}
		}
	}
}