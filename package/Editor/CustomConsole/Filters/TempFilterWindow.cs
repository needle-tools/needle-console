using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	public class TempFilterWindow : EditorWindow
	{
		[MenuItem("Tools/Demystify/Dev Console Filter Window")]
		private static void Open()
		{
			var window = CreateWindow<TempFilterWindow>();
			window.Show();
		}

		private Vector2 scroll;

		private void OnEnable()
		{
			titleContent = new GUIContent("Console Filter");
		}

		private void OnGUI()
		{
			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUI.BeginChangeCheck();
			ConsoleList.DrawCustom = EditorGUILayout.Toggle("Custom List", ConsoleList.DrawCustom);
			GUILayout.Space(10);

			foreach (var filter in ConsoleFilter.RegisteredFilter)
			{
				filter.OnGUI();
			}

			if (EditorGUI.EndChangeCheck())
			{
				ConsoleFilter.MarkDirty();
				InternalEditorUtility.RepaintAllViews();
			}

			EditorGUILayout.EndScrollView();
		}
	}
}