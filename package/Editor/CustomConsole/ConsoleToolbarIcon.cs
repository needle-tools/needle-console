using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleToolbarIcon
	{
		private static bool filterTextureInit;
		private static Texture2D filterIcon, filterIconDisabled;

		internal static void OnDrawToolbar()
		{
			if (!NeedleConsoleSettings.instance.CustomConsole) return;
			
			if (!filterTextureInit)
			{
				filterTextureInit = true;
				filterIcon = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				filterIconDisabled = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
			}

			var text = " " + (ConsoleFilter.HiddenCount >= 1000 ? "999+" : ConsoleFilter.HiddenCount.ToString());
			var icon = ConsoleFilter.enabled ? filterIcon : filterIconDisabled;
			var tooltip = ConsoleFilter.HiddenCount > 1 ? ConsoleFilter.HiddenCount + " logs" : ConsoleFilter.HiddenCount + " log";
			if (ConsoleFilter.enabled) tooltip += " hidden";
			else tooltip += " would be hidden";
			var content = new GUIContent(text, icon, tooltip);
			ConsoleFilter.enabled = !GUILayout.Toggle(!ConsoleFilter.enabled, content, ConsoleWindow.Constants.MiniButtonRight);
			
			// var rect = GUILayoutUtility.GetLastRect();
			// rect.x += rect.width;
			// rect.width = 50;
			// GUILayout.Space(rect.width);
			// if (EditorGUI.DropdownButton(rect, new GUIContent("Filter"), FocusType.Passive, EditorStyles.toolbarDropDown))
			// {
			// 	PopupWindow.Show(rect, new FilterFoldoutContent(), new[] {PopupLocation.Below});
			// }
		}
	}
}