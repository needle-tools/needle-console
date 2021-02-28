using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal enum Highlighting
	{
		None = 0,
		Simple = 1,
		Complex = 2,
	}

	[FilePath("ProjectSettings/DemystifySettings.asset", FilePathAttribute.Location.ProjectFolder)]
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
		
		[SerializeField]
		internal bool FirstInstall = true;
		
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
	}
}