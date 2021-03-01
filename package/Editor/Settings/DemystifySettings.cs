using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[FilePath("Preferences/DemystifySettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class DemystifyProjectSettings : ScriptableSingleton<DemystifyProjectSettings>
	{
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Demystify Project Settings");
			base.Save(true);
		}
		
		[SerializeField]
		internal bool FirstInstall = true;
	}
	

	[FilePath("Preferences/DemystifySettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	internal class DemystifySettings : ScriptableSingleton<DemystifySettings>
	{
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Demystify Settings");
			base.Save(true);
		}
		
		public bool DevelopmentMode
		{
			get => SessionState.GetBool("Demystify.DevelopmentMode", false);
			set => SessionState.SetBool("Demystify.DevelopmentMode", value);
		}
		
		public Highlighting SyntaxHighlighting = Highlighting.Simple;
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
	}
}