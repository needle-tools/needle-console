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
			ConsoleFilter.enabled = !GUILayout.Toggle(!ConsoleFilter.enabled, new GUIContent(text, icon, tooltip), ConsoleWindow.Constants.MiniButtonRight);
		}
	}
}