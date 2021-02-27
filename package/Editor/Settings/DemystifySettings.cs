using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace needle.demystify
{
	internal enum Highlighting
	{
		None = 0,
		Simple = 1,
		Complex = 2,
	}

	[FilePath("ProjectSettings/UnityDemystifySettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class DemystifySettings : ScriptableSingleton<DemystifySettings>
	{
		internal void Save()
		{
			base.Save(true);
		}

		public bool IsEnabled = true;
		public bool DevelopmentMode = false;
		public bool FixHyperlinks = true;
		public Highlighting SyntaxHighlighting = Highlighting.Complex;
		public bool UseSyntaxHighlighting => SyntaxHighlighting != Highlighting.None;

		[SerializeField] private Theme Theme;

		public Theme CurrentTheme
		{
			get
			{
				if (Theme == null) SetDefaultTheme();
				return Theme;
			}
			set
			{
				if (value == Theme) return;
				Theme = value;
				if (Theme.EnsureEntries())
					Theme.SetActive();
			}
		}

		public void SetDefaultTheme()
		{
			CurrentTheme = new Theme(Theme.DefaultThemeName);
			if (CurrentTheme.isDirty)
				CurrentTheme.SetActive();
		}

		[InitializeOnLoadMethod]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			instance.CurrentTheme.EnsureEntries();
			instance.CurrentTheme.SetActive();
		}
	}

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
			// Debug.Log("Activate");
			if (this.Entries.Count >= 0)
			{
				SyntaxHighlighting.CurrentTheme.Clear();
				foreach (var entry in this.Entries)
				{
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