using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[CreateAssetMenu(menuName = "Needle/Demystify/Syntax Highlighting Theme")]
	public class SyntaxHighlightingTheme : ScriptableObject
	{
		public Theme theme = new Theme("New Theme");
	}

	[CustomEditor(typeof(SyntaxHighlightingTheme))]
	public class SyntaxHighlightingThemeEditor : Editor
	{
		private Highlighting previewHighlightingStyle;

		private void OnEnable()
		{
			previewHighlightingStyle = DemystifySettings.instance.SyntaxHighlighting;
			if (target is SyntaxHighlightingTheme sh && sh.theme != null)
				sh.theme.Name = target.name;
		}

		public override void OnInspectorGUI()
		{
			var targetTheme = target as SyntaxHighlightingTheme;
			if (!targetTheme) return;
			var theme = targetTheme.theme;

			theme.EnsureEntries();

			serializedObject.Update();

			// Name inspector
			var themeProperty = serializedObject.FindProperty("theme");
			var nameProperty = themeProperty.FindPropertyRelative("Name");

			EditorGUI.BeginChangeCheck();
			using (new EditorGUI.DisabledScope(true))
			{
				nameProperty.stringValue = target.name;
				EditorGUILayout.PropertyField(nameProperty);
			}

			EditorGUILayout.Space(); 

			DemystifySettingsProvider.DrawThemeColorOptions(theme, false);

			EditorGUILayout.Space();
			if (GUILayout.Button("Copy from Active"))
			{
				var currentTheme = DemystifySettings.instance.CurrentTheme;
				targetTheme.theme = currentTheme; 
			}

			if (GUILayout.Button("Activate"))
			{
				DemystifySettings.instance.CurrentTheme = theme;
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(target, "Edited " + name);
				if (theme == DemystifySettings.instance.CurrentTheme)
					DemystifySettings.instance.UpdateCurrentTheme();
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);
			}

			EditorGUILayout.Space();
			DrawPreview(theme, ref previewHighlightingStyle);
		}

		private static readonly Dictionary<string, string> previewColorDict = new Dictionary<string, string>();
		private static GUIStyle previewStyle;

		private static bool themePreviewFoldout 
		{
			get => SessionState.GetBool("DemystifySyntaxPreviewFoldout", false);
			set => SessionState.SetBool("DemystifySyntaxPreviewFoldout", value);
		}
		
		private static void DrawPreview(Theme theme, ref Highlighting style)
		{
			EditorGUILayout.Space(8);
			themePreviewFoldout = EditorGUILayout.Foldout(themePreviewFoldout, "Theme Preview");
			if(!themePreviewFoldout) return;
			// style = (Highlighting) EditorGUILayout.EnumPopup("Preview Style", style); 
			EditorGUILayout.Space(5);
			
			if(previewStyle == null)
				previewStyle = new GUIStyle(EditorStyles.label) {richText = true, wordWrap = false};
			using (new EditorGUI.DisabledScope(true))
			{
				var settings = DemystifySettings.instance;
				// var currentStyle = settings.SyntaxHighlighting;
				// settings.SyntaxHighlighting = style;
				theme.SetActive(previewColorDict);
				var str = DummyData.SyntaxHighlightVisualization;
				DemystifySettingsProvider.ApplySyntaxHighlightingMultiline(ref str, previewColorDict);;
				// settings.SyntaxHighlighting = currentStyle;
				GUILayout.TextArea(str, previewStyle);
			}
		}
	}
}