using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
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
		
		public bool DevelopmentMode
		{
			get => SessionState.GetBool("Demystify.DevelopmentMode", false);
			set => SessionState.SetBool("Demystify.DevelopmentMode", value);
		}
		
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

			if (instance.firstInstall)
			{
				instance.firstInstall = false;
				UnityDemystify.Enable();
			}
		}

		[SerializeField]
		private bool firstInstall = true;
	}
	
	#if !UNITY_2020_1_OR_NEWER
	
	[System.AttributeUsage(System.AttributeTargets.Class)]
	internal sealed class FilePathAttribute : System.Attribute
	{
		public enum Location
		{
			PreferencesFolder,
			ProjectFolder
		}

		public string filepath { get; set; }

		public FilePathAttribute(string relativePath, FilePathAttribute.Location location)
		{
			if (string.IsNullOrEmpty(relativePath))
			{
				Debug.LogError("Invalid relative path! (its null or empty)");
				return;
			}

			if (relativePath[0] == '/')
				relativePath = relativePath.Substring(1);
#if UNITY_EDITOR
			if (location == FilePathAttribute.Location.PreferencesFolder)
				this.filepath = InternalEditorUtility.unityPreferencesFolder + "/" + relativePath;
			else
#endif
				this.filepath = relativePath;
		}
	}
	#endif
}