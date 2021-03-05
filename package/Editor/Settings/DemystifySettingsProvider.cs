using System;
using System.Collections.Generic;
using System.Linq;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

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
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			return null;
		}

		private DemystifySettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		[MenuItem("Tools/Demystify/Enable Development Mode", true)]
		private static bool EnableDevelopmentModeValidate() => !DemystifySettings.instance.DevelopmentMode;
		[MenuItem("Tools/Demystify/Enable Development Mode")]
		private static void EnableDevelopmentMode() => DemystifySettings.instance.DevelopmentMode = true;
		[MenuItem("Tools/Demystify/Disable Development Mode", true)]
		private static bool DisableDevelopmentModeValidate() => DemystifySettings.instance.DevelopmentMode;
		[MenuItem("Tools/Demystify/Disable Development Mode")]
		private static void DisableDevelopmentMode() => DemystifySettings.instance.DevelopmentMode = false;

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

				// if(settings.DevelopmentMode)
				// // using(new EditorGUI.DisabledScope(!settings.DevelopmentMode))
				// {
				// 	EditorGUILayout.Space(10);
				// 	EditorGUILayout.LabelField("Development Settings", EditorStyles.boldLabel);
				// 	settings.FixHyperlinks = EditorGUILayout.ToggleLeft(new GUIContent("Convert demystified filepaths to Unity hyperlink format", "Unity expects paths in stacktraces in a specific format to make them clickable (blue). This option will convert the file paths in demystified stacktraces to conform to that format."), settings.FixHyperlinks);
				// }
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
			// using (new EditorGUI.DisabledScope(!settings.UseSyntaxHighlighting))
			{
				var theme = settings.CurrentTheme;
				if (theme != null)
				{
					SyntaxHighlightSettingsThemeFoldout = EditorGUILayout.Foldout(SyntaxHighlightSettingsThemeFoldout, "Theme: " + theme.Name);
					if (SyntaxHighlightSettingsThemeFoldout)
					{
						EditorGUI.indentLevel++;
						EditorGUI.BeginChangeCheck();
						var currentPattern = SyntaxHighlighting.CurrentPatternsList;
						for (var index = 0; index < theme.Entries?.Count; index++)
						{
							var entry = theme.Entries[index];
							var usedByCurrentRegex = entry.Key == "string_literal" || entry.Key == "comment" || entry.Key == "link" || (currentPattern?.Any(e => e.Contains("?<" + entry.Key)) ?? true);
							if (!usedByCurrentRegex) continue;
							// using(new EditorGUI.DisabledScope(!usedByCurrentRegex))
							{
								entry.Color = EditorGUILayout.ColorField(entry.Key, entry.Color);
							}
						}
						EditorGUI.indentLevel--;
					}

					if (EditorGUI.EndChangeCheck())
					{
						theme.SetActive();
						ThemeEditedOrChanged?.Invoke();
					}
					
					if (SyntaxHighlightSettingsThemeFoldout)
					{
						EditorGUILayout.Space(5);
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button(new GUIContent("Log Highlighted Message", "For testing when changing colors only")))
						{
							var str = DummyData.SyntaxHighlightVisualization;
							ApplySyntaxHighlightingMultiline(ref str);
							var p = settings.SyntaxHighlighting;
							settings.SyntaxHighlighting = Highlighting.None;
							Debug.Log("Example Log: " + "\n\n" + str + "\n\n--------\n");
							settings.SyntaxHighlighting= p;
						}

						if (GUILayout.Button(new GUIContent("Reset to Default Theme")))
						{
							Undo.RegisterCompleteObjectUndo(settings, "Reset to default theme");
							settings.SetDefaultTheme();
							ThemeEditedOrChanged?.Invoke();
						}

						EditorGUILayout.EndHorizontal();
					}
				}

				
				// if (GUILayout.Button("Reset Theme"))
				// {
				// 	settings.SetDefaultTheme();
				// }
			}
		}

		private static void DrawActivateGUI(DemystifySettings settings)
		{
			if (!UnityDemystify.Patches().All(PatchManager.IsActive))
			{
				if (GUILayout.Button(new GUIContent("Enable Demystify", 
					"Enables patches:\n" + string.Join("\n", UnityDemystify.Patches())
				)))
					UnityDemystify.Enable();
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
		private static void ApplySyntaxHighlightingMultiline(ref string str)
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
				SyntaxHighlighting.AddSyntaxHighlighting(ref line);
				line = line.Replace("at ", "");
				str += line + "\n";
			}
		}
	}
}