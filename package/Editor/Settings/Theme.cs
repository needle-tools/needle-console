using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class Theme
	{
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
			foreach (var kvp in SyntaxHighlighting.DefaultTheme)
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