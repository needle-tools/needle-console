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

		[SerializeField] private FileFilter fileFilter = new FileFilter();
		[SerializeField] private PackageFilter packageFilter = new PackageFilter();

		private void OnEnable()
		{
			titleContent = new GUIContent("Console Filter");
			fileFilter.window = this;
			ConsoleFilter.AddFilter(fileFilter);
			ConsoleFilter.AddFilter(packageFilter);
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			ConsoleList.DrawCustom = EditorGUILayout.Toggle("Draw Custom", ConsoleList.DrawCustom);

			DrawFilter("Files", fileFilter);
			DrawFilter("Packages", packageFilter);

			if (EditorGUI.EndChangeCheck())
			{
				ConsoleFilter.MarkDirty();
				InternalEditorUtility.RepaintAllViews();
			}
		}

		private void DrawFilter(string header, BaseFilterWithActiveState<string> _filter)
		{
			var key = "ConsoleFilter" + header;
			var foldout = EditorGUILayout.Foldout(SessionState.GetBool(key, true), header);
			SessionState.SetBool(key, foldout);
			if (!foldout) return;
			for (var index = 0; index < _filter.Count; index++)
			{
				var file = _filter[index];
				var label = _filter.GetLabel(index);
				using (new GUILayout.HorizontalScope())
				{
					var ex = EditorGUILayout.ToggleLeft(new GUIContent(label, file), _filter.IsActive(index));
					_filter.SetActive(index, ex);
					if (GUILayout.Button("x", GUILayout.Width(30)))
					{
						_filter.Remove(index);
						index -= 1;
					}
				}
			}
		}
	}
}