using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Demystify
{
	public class DemystifySettingsProvider : SettingsProvider
	{
		public const string SettingsPath = "Preferences/Needle/Demystify";
		[SettingsProvider]
		public static SettingsProvider CreateDemystifySettings()
		{
			try
			{
				DemystifySettings.instance.Save();
				return new DemystifySettingsProvider(SettingsPath, SettingsScope.User);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return null;
		}

		private DemystifySettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
			ThemeNames = null;
		}

		[MenuItem("Tools/Demystify/Enable Development Mode", true)]
		private static bool EnableDevelopmentModeValidate() => !DemystifySettings.DevelopmentMode;
		[MenuItem("Tools/Demystify/Enable Development Mode")]
		private static void EnableDevelopmentMode() => DemystifySettings.DevelopmentMode = true;
		[MenuItem("Tools/Demystify/Disable Development Mode", true)]
		private static bool DisableDevelopmentModeValidate() => DemystifySettings.DevelopmentMode;
		[MenuItem("Tools/Demystify/Disable Development Mode")]
		private static void DisableDevelopmentMode() => DemystifySettings.DevelopmentMode = false;

		private Vector2 scroll;

		public override void OnGUI(string searchContext)
		{
			base.OnGUI(searchContext);
			var settings = DemystifySettings.instance;

			EditorGUI.BeginChangeCheck();

			using (var s = new EditorGUILayout.ScrollViewScope(scroll))
			{
				scroll = s.scrollPosition;
				DrawActivateGUI(settings);
				DrawSyntaxGUI(settings);

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Console Options", EditorStyles.boldLabel);
				settings.Separator = EditorGUILayout.TextField(new GUIContent("Stacktrace Separator", "Adds a separator to Console stacktrace output between each stacktrace"), settings.Separator);
				settings.AllowCodePreview = EditorGUILayout.Toggle(new GUIContent("Allow Code Preview", "Show code context in popup window when hovering over console log line with file path"), settings.AllowCodePreview); 
				settings.CodePreviewKeyCode = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Code Preview Key", "If None: code preview popup will open on hover. If any key assigned: code preview popup will only open if that key is pressed on hover"), settings.CodePreviewKeyCode);
				settings.ShortenFilePaths = EditorGUILayout.Toggle(new GUIContent("Shorten File Paths", "When enabled demystify tries to shorten package paths to <package_name>@<version> <fileName><line>"), settings.ShortenFilePaths); 
				settings.ShowFileName = EditorGUILayout.Toggle(new GUIContent("Show Filename", "When enabled demystify will prefix console log entries with the file name of the log source"), settings.ShowFileName); 

				if(DemystifySettings.DevelopmentMode)
				// using(new EditorGUI.DisabledScope(!settings.DevelopmentMode))
				{
					EditorGUILayout.Space(10);
					EditorGUILayout.LabelField("Development Settings", EditorStyles.boldLabel);
					if (GUILayout.Button("Refresh Themes List"))
						Themes = null;
				}
			}

			// GUILayout.FlexibleSpace();
			// EditorGUILayout.Space(10);
			// using (new EditorGUILayout.HorizontalScope())
			// {
			// 	settings.DevelopmentMode = EditorGUILayout.ToggleLeft("Development Mode", settings.DevelopmentMode);
			// }

			if (EditorGUI.EndChangeCheck())
			{
				settings.Save();
			}
		}

		private static bool SyntaxHighlightSettingsThemeFoldout
		{
			get => SessionState.GetBool("Demystify.SyntaxHighlightingThemeFoldout", true);
			set => SessionState.SetBool("Demystify.SyntaxHighlightingThemeFoldout", value);
		}

		public static event Action ThemeEditedOrChanged;

		private static readonly string[] AlwaysInclude = new[] {"keywords", "link", "string_literal", "comment"};

		private static void DrawSyntaxGUI(DemystifySettings settings)
		{
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("Syntax Highlighting", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			settings.SyntaxHighlighting = (Highlighting) EditorGUILayout.EnumPopup("Syntax Highlighting", settings.SyntaxHighlighting);
			if (EditorGUI.EndChangeCheck())
			{
				SyntaxHighlighting.OnSyntaxHighlightingModeHasChanged();
				ThemeEditedOrChanged?.Invoke();
			}
			DrawThemePopup();
			SyntaxHighlightSettingsThemeFoldout = EditorGUILayout.Foldout(SyntaxHighlightSettingsThemeFoldout, "Colors");
			if (SyntaxHighlightSettingsThemeFoldout)
			{
				var theme = settings.CurrentTheme;
				if (theme != null)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUI.indentLevel++;
					DrawThemeColorOptions(theme);
					EditorGUI.indentLevel--;
					if (EditorGUI.EndChangeCheck())
					{
						theme.SetActive();
						ThemeEditedOrChanged?.Invoke();
					}
			
				}
			}
		}

		internal static void DrawThemeColorOptions(Theme theme, bool skipUnused = true)
		{
			var currentPattern = SyntaxHighlighting.CurrentPatternsList;
			for (var index = 0; index < theme.Entries?.Count; index++)
			{
				var entry = theme.Entries[index];
				var usedByCurrentRegex = AlwaysInclude.Contains(entry.Key) || (currentPattern?.Any(e => e.Contains("?<" + entry.Key)) ?? true);
				if (skipUnused  && !usedByCurrentRegex) continue;
				// using(new EditorGUI.DisabledScope(!usedByCurrentRegex))
				{
					var col = GUI.color;
					GUI.color = !usedByCurrentRegex || Theme.Ignored(entry.Color) ? Color.gray : col;
					entry.Color = EditorGUILayout.ColorField(entry.Key, entry.Color);
					GUI.color = col;
				}
			}
		}

		private static void DrawActivateGUI(DemystifySettings settings)
		{
			if (!UnityDemystify.Patches().All(PatchManager.IsActive))
			{
				if (GUILayout.Button(new GUIContent("Enable Demystify", 
					"Enables patches:\n" + string.Join("\n", UnityDemystify.Patches())
				)))
					UnityDemystify.Enable(true);
				EditorGUILayout.HelpBox("Demystify is disabled, click the Button above to enable it", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button(new GUIContent("Disable Demystify", 
					"Disables patches:\n" + string.Join("\n", UnityDemystify.Patches())
					)))
					UnityDemystify.Disable();
			}
		}
		
		
		
		/// <summary>
		/// this is just for internal use and "visualizing" via GUI
		/// </summary>
		internal static void ApplySyntaxHighlightingMultiline(ref string str, Dictionary<string, string> colorDict = null)
		{
			var lines = str.Split('\n');
			str = "";
			// Debug.Log("lines: " + lines.Count());
			foreach (var t in lines)
			{
				var line = t;
				var pathIndex = line.IndexOf("C:/git/", StringComparison.Ordinal);
				if (pathIndex > 0) line = line.Substring(0, pathIndex - 4);
				if (!line.TrimStart().StartsWith("at "))
					line = "at " + line;
				SyntaxHighlighting.AddSyntaxHighlighting(ref line, colorDict);
				line = line.Replace("at ", "");
				str += line + "\n";
			}
		}

		private static string[] ThemeNames;
		private static Theme[] Themes;

		private static void EnsureThemeOptions()
		{
			if (ThemeNames == null || Themes == null)
			{
				var themeAssets = AssetDatabase.FindAssets("t:" + nameof(SyntaxHighlightingTheme)).Select(AssetDatabase.GUIDToAssetPath).ToArray();
				ThemeNames = new string[themeAssets.Length];
				Themes = new Theme[ThemeNames.Length];
				for (var index = 0; index < themeAssets.Length; index++)
				{
					var path = themeAssets[index];
					var asset = AssetDatabase.LoadAssetAtPath<SyntaxHighlightingTheme>(path);
					if (asset.theme == null) asset.theme = new Theme("Unknown");
					ThemeNames[index] = asset.theme.Name;
					Themes[index] = asset.theme;
				}
			}
			else if(ThemeNames != null && Themes != null && ThemeNames.Length == Themes.Length)
			{
				for (var index = 0; index < Themes.Length; index++)
				{
					var t = Themes[index];
					ThemeNames[index] = t.Name;
				}
			}
		}

		private static int ActiveThemeIndex()
		{
			var active = DemystifySettings.instance.CurrentTheme;
			for (var index = 0; index < Themes.Length; index++) 
			{
				var theme = Themes[index];
				if (theme.Equals(active) || theme.Name == active.Name) return index; 
			}
			return -1;
		}

		private static void DrawThemePopup()
		{
			EnsureThemeOptions(); 
			EditorGUI.BeginChangeCheck(); 
			var selected = EditorGUILayout.Popup("Theme", ActiveThemeIndex(), ThemeNames);
			if(selected >= 0 && selected < Themes.Length)
				DemystifySettings.instance.CurrentTheme = Themes[selected];
			if (EditorGUI.EndChangeCheck())
			{
				DemystifySettings.instance.Save();
				ThemeEditedOrChanged?.Invoke();
			}
		}

		private class AssetProcessor : AssetPostprocessor
		{
			private void OnPreprocessAsset()
			{
				ThemeNames = null;
				Themes = null;
			}
		}
	}
}
