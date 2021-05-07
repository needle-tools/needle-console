using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleOptionsFoldout
	{
		internal static void OnDrawFoldouts()
		{
			var rect = GUILayoutUtility.GetLastRect();
			rect.x += rect.width;
			var label = new GUIContent("Filter", "Console Filter Options");
			rect.width = EditorStyles.toolbarDropDown.CalcSize(label).x;
			if (EditorGUI.DropdownButton(rect, label, FocusType.Keyboard, EditorStyles.toolbarDropDown))
			{
				PopupWindow.Show(rect, new FilterFoldout(), new[] {PopupLocation.Below});
			}
		}

		private class FilterFoldout : PopupWindowContent
		{
			public override Vector2 GetWindowSize()
			{
				return new Vector2(350, 400);
			}

			private Vector2 scroll;

			public override void OnGUI(Rect rect)
			{
				scroll = EditorGUILayout.BeginScrollView(scroll);
				EditorGUI.BeginChangeCheck();
				foreach (var filter in ConsoleFilter.RegisteredFilter)
				{
					filter.OnGUI();
				}

				if (EditorGUI.EndChangeCheck())
				{
					ConsoleFilter.MarkDirty();
				}

				EditorGUILayout.EndScrollView();
			}
		}
	}
}