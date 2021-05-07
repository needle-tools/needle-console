using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	public class TempFilterWindow : EditorWindow
	{
		[MenuItem("Demystify/Console Filter")]
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

			ConsoleList.DrawCustom = EditorGUILayout.Toggle("Draw Custom", ConsoleList.DrawCustom);
			GUILayout.Space(10);

			foreach (var filter in ConsoleFilter.RegisteredFilter)
			{
				filter.OnGUI();
				GUILayout.Space(5);
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