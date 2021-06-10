﻿using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Console
{
	[FilePath("Preferences/NeedleConsoleSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	internal class NeedleConsoleSettings : ScriptableSingleton<NeedleConsoleSettings>
	{
		internal bool Enabled = true;
		
		public static event Action Changed;

		internal static void RaiseChangedEvent() => Changed?.Invoke();
		
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Needle Console Settings");
			base.Save(true);
		}

		public static bool DevelopmentMode
		{
			get => SessionState.GetBool("Needle.Console.DevelopmentMode", false);
			set => SessionState.SetBool("Needle.Console.DevelopmentMode", value);
		}

		public Highlighting SyntaxHighlighting = Highlighting.Simple;
		public bool UseSyntaxHighlighting => SyntaxHighlighting != Highlighting.None;

		[SerializeField] private Theme Theme;

		public static event Action ThemeChanged;

		public Theme CurrentTheme
		{
			get
			{
				return Theme;
			}
			set
			{
				if (value == Theme) return;
				Theme = value;
				UpdateCurrentTheme();
			}
		}
 
		public void SetDefaultTheme()
		{
			CurrentTheme = GetNewDefaultThemeInstance();
			if (CurrentTheme.isDirty)
				CurrentTheme.SetActive();
		}
		
		private static Theme GetNewDefaultThemeInstance()
		{
			var themes = AssetDatabase.FindAssets("t:" + nameof(SyntaxHighlightingTheme));
			foreach (var theme in themes)
			{
				var path = AssetDatabase.GUIDToAssetPath(theme);
				var name = Path.GetFileName(path);
				if (name.ToLowerInvariant().Contains("default"))
				{
					var loaded = AssetDatabase.LoadAssetAtPath<SyntaxHighlightingTheme>(path);
					if (loaded) return loaded.theme; 
				}
			}
			return new Theme(Theme.DefaultThemeName);
		}

		public void UpdateCurrentTheme()
		{
			Theme.EnsureEntries();
			Theme.SetActive();
			ThemeChanged?.Invoke();
			RaiseChangedEvent();
			InternalEditorUtility.RepaintAllViews();
		}

		public string Separator = "—";
		public bool AllowCodePreview = true;
		public KeyCode CodePreviewKeyCode = KeyCode.None;

		public bool ShortenFilePaths = true;
		public bool ShowFileName = true;
		public string ColorMarker = "┃";// "┃";
		
		public bool CustomList = true;
		public bool RowColors = true;
		public bool IndividualCollapse = true;
		public bool UseCustomFont = true;
		public string LogEntryFont = "Fira Code";
	}
	
}
