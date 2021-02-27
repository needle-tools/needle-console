using System;
using System.Collections.Generic;
using System.Linq;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

namespace needle.demystify
{
	public class DemystifySettingsProvider : SettingsProvider
	{
		[SettingsProvider]
		public static SettingsProvider CreateDemystifySettings()
		{
			try
			{
				DemystifySettings.instance.Save();
				return new DemystifySettingsProvider("Project/Needle/Unity Demystify", SettingsScope.Project);
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

		private Vector2 scroll;

		public override void OnGUI(string searchContext)
		{
			base.OnGUI(searchContext);
			var settings = DemystifySettings.instance;

			EditorGUI.BeginChangeCheck();

			using (var s = new EditorGUILayout.ScrollViewScope(scroll))
			{
				scroll = s.scrollPosition;
				DrawActivateGUI();

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
				settings.FixHyperlinks = EditorGUILayout.ToggleLeft("Fix Hyperlinks", settings.FixHyperlinks);
				DrawSyntaxGUI(settings);
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.Space(10);
			using (new EditorGUILayout.HorizontalScope())
			{
				settings.DevelopmentMode = EditorGUILayout.ToggleLeft("Development Mode", settings.DevelopmentMode);
			}

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

		private static void DrawSyntaxGUI(DemystifySettings settings)
		{
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("Syntax Highlighting", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			settings.SyntaxHighlighting = (Highlighting) EditorGUILayout.EnumPopup("Syntax Highlighting", settings.SyntaxHighlighting);
			if (EditorGUI.EndChangeCheck())
				SyntaxHighlighting.OnSyntaxHighlightingModeHasChanged();
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
							var usedByCurrentRegex = currentPattern?.Any(e => e.Contains(entry.Key)) ?? true;
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
					}
					
					if (SyntaxHighlightSettingsThemeFoldout)
					{
						EditorGUILayout.Space(5);
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button(new GUIContent("Log Highlighted Message", "For testing when changing colors only")))
						{
							var str = GUIUtils.SyntaxHighlightVisualization;
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

		private static void DrawActivateGUI()
		{
			if (!UnityDemystify.Patches().All(PatchManager.IsActive))
			{
				if (GUILayout.Button("Enable Unity Demystify"))
					UnityDemystify.Enable();
				EditorGUILayout.HelpBox("Unity Demystify is disabled, click the Button above to enable it", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button("Disable Unity Demystify"))
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