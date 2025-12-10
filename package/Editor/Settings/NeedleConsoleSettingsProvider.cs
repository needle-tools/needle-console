using System;
using System.Collections.Generic;
using System.Linq;
using Needle.Editors;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Console
{
	public class NeedleConsoleSettingsProvider : SettingsProvider
	{
		public const string SettingsPath = "Preferences/Needle/Console";
		[SettingsProvider]
		public static SettingsProvider CreateDemystifySettings()
		{
			try
			{
				NeedleConsoleSettings.instance.Save();
				return new NeedleConsoleSettingsProvider(SettingsPath, SettingsScope.User);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return null;
		}

		private NeedleConsoleSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		private SerializedObject serializedObject;
		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
			ThemeNames = null;
			var settings = NeedleConsoleSettings.instance;
			if (serializedObject == null || serializedObject.targetObject != settings)
			{
				serializedObject = new SerializedObject(settings);
				serializedObject.targetObject.hideFlags = HideFlags.None; // Ensure it's editable
			}
		}

		[MenuItem("Tools/Needle Console/Enable Development Mode", true)]
		private static bool EnableDevelopmentModeValidate() => !NeedleConsoleSettings.DevelopmentMode;
		[MenuItem("Tools/Needle Console/Enable Development Mode")]
		private static void EnableDevelopmentMode() => NeedleConsoleSettings.DevelopmentMode = true;
		[MenuItem("Tools/Needle Console/Disable Development Mode", true)]
		private static bool DisableDevelopmentModeValidate() => NeedleConsoleSettings.DevelopmentMode;
		[MenuItem("Tools/Needle Console/Disable Development Mode")]
		private static void DisableDevelopmentMode() => NeedleConsoleSettings.DevelopmentMode = false;

		private Vector2 scroll;

		public override void OnGUI(string searchContext)
		{
			base.OnGUI(searchContext);
			var settings = NeedleConsoleSettings.instance;

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();


			Assets.DrawGUIFullLogo();

			using (var s = new EditorGUILayout.ScrollViewScope(scroll))
			{
				scroll = s.scrollPosition;
				DrawActivateGUI(settings);
				DrawSyntaxGUI(settings);

				GUILayout.Space(10);
				EditorGUILayout.LabelField("Console Options", EditorStyles.boldLabel);
				settings.Separator = EditorGUILayout.TextField(new GUIContent("Stacktrace Separator", "Adds a separator to Console stacktrace output between each stacktrace"), settings.Separator);
				settings.ShortenFilePaths = EditorGUILayout.Toggle(new GUIContent("Short File Paths", "When enabled Needle Console tries to shorten package paths to <package_name>@<version> <fileName><line>"), settings.ShortenFilePaths);
				settings.ShowLogPrefix = EditorGUILayout.Toggle(new GUIContent("Show Filename", "When enabled Needle Console will prefix console log entries with the file name of the log source"), settings.ShowLogPrefix);
				settings.HideInternalStacktrace = EditorGUILayout.Toggle(new GUIContent("Clean Stacktrace", "When enabled internal stacktrace elements will not be shown (e.g. System.Tasks or internal UnityEngine method calls). Changes will take effect for new logs (already logged items will not be updates)"), settings.HideInternalStacktrace);

				EditorGUILayout.Space();
				using (new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel(new GUIContent("Custom Console", "The custom list replaces the console log drawing with a custom implementation that allows for advanced features such like very custom log filtering via context menus"), EditorStyles.boldLabel, EditorStyles.boldLabel);
					GUILayout.Space(2);
					settings.CustomConsole = EditorGUILayout.Toggle(settings.CustomConsole);
				}
				using (new EditorGUI.DisabledScope(!settings.CustomConsole))
				{
					settings.RowColors = EditorGUILayout.Toggle(new GUIContent("Row Colors", "Allow custom list to tint row background for warnings and errors"), settings.RowColors);

					settings.StacktraceWrap = EditorGUILayout.Toggle(new GUIContent("Stacktrace: Line-Wrap", "When enabled logs in the stacktrace will word-wrap - meaning very long stacktrace lines will not wrap around making it sometimes easier to read."), settings.StacktraceWrap);


					settings.IndividualCollapse = EditorGUILayout.Toggle(new GUIContent("Allow Individual Collapse", "When enabled the log context menu allows to collapse individual logs. To add log messages for individual collapse simply right click on a message in the console window and select the 'Collapse' menu item (this action can be undone at any time)"), settings.IndividualCollapse);
					using (new EditorGUI.DisabledScope(!settings.IndividualCollapse))
					{
						EditorGUI.indentLevel++;
						settings.IndividualCollapsePreserveContext = EditorGUILayout.Toggle(new GUIContent("Keep Log Context", "When enabled collapsing will be interupted by other log messages. Disable this if you want exactly one log message for selected lines."), settings.IndividualCollapsePreserveContext);
						EditorGUI.indentLevel--;
					}

					settings.UseCustomFont = EditorGUILayout.Toggle(new GUIContent("Use Custom Font", "Allow using a custom font. Specify a font name that you have installed below"), settings.UseCustomFont);
					using (new EditorGUI.DisabledScope(!settings.UseCustomFont))
					{
						EditorGUI.indentLevel++;
						var fontOptions = Font.GetOSInstalledFontNames();
						var selectedFont = EditorGUILayout.Popup(new GUIContent("Installed Fonts"), fontOptions.IndexOf(f => f == settings.InstalledLogEntryFont), fontOptions);
						if (selectedFont >= 0 && selectedFont < fontOptions.Length) settings.InstalledLogEntryFont = fontOptions[selectedFont];
						settings.CustomLogEntryFont = (Font)EditorGUILayout.ObjectField(new GUIContent("Custom Font", "Will override installed font"), settings.CustomLogEntryFont, typeof(Font), false);
						EditorGUI.indentLevel--;
					}
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Experimental", EditorStyles.boldLabel);
				settings.AllowCodePreview = EditorGUILayout.Toggle(new GUIContent("Code Preview", "Show code context in popup window when hovering over console log line with file path"), settings.AllowCodePreview);
				EditorGUI.BeginDisabledGroup(!settings.AllowCodePreview);
				EditorGUI.indentLevel++;
				settings.CodePreviewKeyCode = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Shortcut", "If None: code preview popup will open on hover. If any key assigned: code preview popup will only open if that key is pressed on hover"), settings.CodePreviewKeyCode);
				EditorGUI.indentLevel--;
				EditorGUI.EndDisabledGroup();
				using (var _scope = new EditorGUI.ChangeCheckScope())
				{
					settings.UseColorMarker = EditorGUILayout.Toggle(new GUIContent("Draw Color Marker"), settings.UseColorMarker);

					using (new EditorGUI.DisabledScope(!settings.UseColorMarker))
					using (new GUILayout.HorizontalScope())
					{
						settings.ColorMarker = EditorGUILayout.TextField(new GUIContent("Color Marker", "Colored marker added before console log"), settings.ColorMarker);
						if (settings.ColorMarker != NeedleConsoleSettings.DefaultColorMarker && GUILayout.Button("Reset", GUILayout.Width(50)))
							settings.ColorMarker = NeedleConsoleSettings.DefaultColorMarker; ;

					}
					if (_scope.changed) NeedleConsoleProjectSettings.RaiseColorsChangedEvent();
				}

				GUILayout.Space(10);
				EditorGUILayout.LabelField("Experimental > Stacktrace", EditorStyles.boldLabel);
				using (var _scope = new EditorGUI.ChangeCheckScope()) {
					settings.StacktraceOrientation = (NeedleConsoleSettings.StacktraceOrientations)EditorGUILayout.EnumPopup("Orientation", settings.StacktraceOrientation);
					using (new EditorGUI.DisabledScope(settings.StacktraceOrientation != NeedleConsoleSettings.StacktraceOrientations.Auto))
					{
						var windowHeight = Math.Max(300, Screen.height);
						EditorGUI.indentLevel++;
						settings.StacktraceOrientationAutoHeight = EditorGUILayout.IntSlider(new GUIContent("Auto Height (in pixel)", "The height at which the stacktrace orientation will switch from Vertical to Horizontal when Stacktrace Orientation is set to Auto."), (int)settings.StacktraceOrientationAutoHeight, 100, windowHeight);
						EditorGUI.indentLevel--;
					}

					settings.StacktraceNamespaceMode = (NeedleConsoleSettings.StacktraceNamespace)EditorGUILayout.EnumPopup("Namespace", settings.StacktraceNamespaceMode);
					settings.StacktraceParamsMode = (NeedleConsoleSettings.StacktraceParams)EditorGUILayout.EnumPopup("Parameters", settings.StacktraceParamsMode);
					settings.StacktraceFilenameMode = (NeedleConsoleSettings.StacktraceFilename)EditorGUILayout.EnumPopup("Filename", settings.StacktraceFilenameMode);

					if (_scope.changed) ThemeEditedOrChanged?.Invoke();
				}

				// using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);
					settings.UseStacktraceIgnoreFilters = EditorGUILayout.Toggle(new GUIContent("Stacktrace Line Filters", "Enable filtering of stacktrace lines based on user defined filters. The stacktrace lines that match any of the given strings will not contain the log path. This is useful if you have custom Console.Log methods but when clicking on a stacktrace line you want to see the original log location."), settings.UseStacktraceIgnoreFilters);

					using (new EditorGUI.DisabledScope(!settings.UseStacktraceIgnoreFilters))
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						var property = serializedObject.FindProperty(nameof(NeedleConsoleSettings.StacktraceIgnoreFilters));
						EditorGUILayout.PropertyField(property, new GUIContent("Stacktrace Line String Filters (Advanced)", "Any line in the stacktrace that matches any of these strings will not be used for opening the file location. This is useful if you have custom Console.Log methods but when clicking on a stacktrace line you want to see the original log location.\n\nIt is recommended to enter the full namespace e.g. 'MyNamespace.MyLogger.Log'. All lines in the stacktrace will be checked for any occurrences.\n\nNote: Only new logs will be affected by this."), true);
						if (check.changed)
						{
							serializedObject.ApplyModifiedProperties();
							settings.Save();
						}
						if (settings.StacktraceIgnoreFilters.Any(e => string.IsNullOrWhiteSpace(e) || e.Length < 4))
						{
							EditorGUILayout.HelpBox("Some stacktrace line filters are very short or empty strings which may lead to unwanted filtering of stacktrace lines. It is recommended to use more specific filter strings. Filters shorter than 4 characters will be ignored.", MessageType.Warning);
						}
					}
				}


				if (NeedleConsoleSettings.DevelopmentMode)
				// using(new EditorGUI.DisabledScope(!settings.DevelopmentMode))
				{
					GUILayout.Space(10);
					EditorGUILayout.LabelField("Development Settings", EditorStyles.boldLabel);
					if (GUILayout.Button("Refresh Themes List"))
						Themes = null;
				}

				GUILayout.Space(20);
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
				NeedleConsoleSettings.RaiseChangedEvent();
			}
		}

		private static bool SyntaxHighlightSettingsThemeFoldout
		{
			get => SessionState.GetBool("NeedleConsole.SyntaxHighlightingThemeFoldout", false);
			set => SessionState.SetBool("NeedleConsole.SyntaxHighlightingThemeFoldout", value);
		}

		public static event Action ThemeEditedOrChanged;

		private static readonly string[] AlwaysInclude = new[] { "keywords", "link", "string_literal", "comment" };

		private static void DrawSyntaxGUI(NeedleConsoleSettings settings)
		{
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Syntax Highlighting", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			settings.SyntaxHighlighting = (Highlighting)EditorGUILayout.EnumPopup("Syntax Highlighting", settings.SyntaxHighlighting);
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
				if (skipUnused && !usedByCurrentRegex) continue;
				// using(new EditorGUI.DisabledScope(!usedByCurrentRegex))
				{
					var col = GUI.color;
					GUI.color = !usedByCurrentRegex || Theme.Ignored(entry.Color) ? Color.gray : col;
					var name = ToDisplayName(entry.Key);
					entry.Color = EditorGUILayout.ColorField(name, entry.Color);
					GUI.color = col;
				}
			}

			string ToDisplayName(string str)
			{
				// replace _ with " " and make first letters uppercase
				var parts = str.Split('_');
				for (var i = 0; i < parts.Length; i++)
				{
					if (parts[i].Length > 0)
						parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
				}
				return string.Join(" ", parts);
			}
		}

		private static void DrawActivateGUI(NeedleConsoleSettings settings)
		{
			if (!settings.Enabled)// !UnityDemystify.Patches().All(PatchManager.IsActive))
			{
				if (GUILayout.Button(new GUIContent("Enable Needle Console")))
					NeedleConsole.Enable();
				EditorGUILayout.HelpBox("Needle Console is disabled, click the Button above to enable it", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button(new GUIContent("Disable Needle Console")))
					NeedleConsole.Disable();
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
			else if (ThemeNames != null && Themes != null && ThemeNames.Length == Themes.Length)
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
			var active = NeedleConsoleSettings.instance.CurrentTheme;
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
			if (selected >= 0 && selected < Themes.Length)
				NeedleConsoleSettings.instance.CurrentTheme = Themes[selected];
			if (EditorGUI.EndChangeCheck())
			{
				NeedleConsoleSettings.instance.Save();
				ThemeEditedOrChanged?.Invoke();
			}
		}
	}
}
