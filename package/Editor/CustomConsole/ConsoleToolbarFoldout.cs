using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleToolbarFoldout
	{
		internal static void OnDrawFoldouts()
		{
			var rect = GUILayoutUtility.GetLastRect();
			rect.x += rect.width;
			var label = new GUIContent("Filter", "Console Filter Options");
			rect.width = EditorStyles.toolbarDropDown.CalcSize(label).x;
			if (EditorGUI.DropdownButton(rect, label, FocusType.Keyboard, EditorStyles.toolbarDropDown))
			{
				PopupWindow.Show(rect, new FilterFoldoutContent(), new[] {PopupLocation.Below});
			}

			GUILayout.Space(rect.width);
		}
	}
}