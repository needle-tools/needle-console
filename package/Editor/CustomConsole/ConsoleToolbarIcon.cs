using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleToolbarIcon
	{
		private static bool filterTextureInit;
		private static Texture2D filterIcon, filterIconDisabled;

		internal static void OnDrawToolbar()
		{
			if (!DemystifySettings.instance.CustomList) return;
			
			if (!filterTextureInit)
			{
				filterTextureInit = true;
				filterIcon = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				filterIconDisabled = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
			}

			var text = ConsoleFilter.enabled 
				? " " + (ConsoleFilter.filteredCount >= 1000 ? "999+" : ConsoleFilter.filteredCount.ToString()) 
				: string.Empty;
			var icon = ConsoleFilter.enabled ? filterIcon : filterIconDisabled;
			var tooltip = ConsoleFilter.filteredCount > 1 ? ConsoleFilter.filteredCount + " logs hidden" : ConsoleFilter.filteredCount + " log hidden";
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