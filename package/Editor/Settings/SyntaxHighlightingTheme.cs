using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            DemystifySettingsProvider.DrawThemeColorOptions(theme);

            EditorGUILayout.Space();
            if (GUILayout.Button("Copy from current"))
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
                if(theme == DemystifySettings.instance.CurrentTheme) 
                    DemystifySettings.instance.UpdateCurrentTheme();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
