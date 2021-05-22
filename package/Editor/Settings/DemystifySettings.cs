using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	[FilePath("Preferences/DemystifySettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	internal class DemystifySettings : ScriptableSingleton<DemystifySettings>
	{
		internal bool Enabled = true;
		
		public static event Action Changed;

		internal static void RaiseChangedEvent() => Changed?.Invoke();
		
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Demystify Settings");
			base.Save(true);
		}

		public static bool DevelopmentMode
		{
			get => SessionState.GetBool("Demystify.DevelopmentMode", false);
			set => SessionState.SetBool("Demystify.DevelopmentMode", value);
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
		
		private static Theme GetNewDefaultThemeInstance() => new Theme(Theme.DefaultThemeName);

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
		public bool DynamicGrouping = true;
	}
	
}
