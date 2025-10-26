using System;
using Needle.Editors;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleToolbarIcon
	{
		private static bool filterTextureInit;
		private static Texture2D filterIcon, filterIconDisabled;
		private static Texture2D orientationVerticalIcon, orientationHorizontalIcon, orientationAutoIcon;
		private static GUIStyle filterButtonStyle;

		internal static void OnDrawToolbar()
		{
			if (!NeedleConsoleSettings.instance.CustomConsole) return;
			
			if (!filterTextureInit)
			{
				filterTextureInit = true;
				filterIcon = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				filterIconDisabled = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
				orientationVerticalIcon = EditorGUIUtility.FindTexture("HorizontalSplit");
				orientationHorizontalIcon = EditorGUIUtility.FindTexture("VerticalSplit");
				orientationAutoIcon = EditorGUIUtility.FindTexture("BuildSettings.N3DS.Small");
			}
			if (filterButtonStyle == null)
			{
#if UNITY_2019_4_OR_NEWER
				filterButtonStyle = new GUIStyle(ConsoleWindow.Constants.MiniButtonRight);
#else
				filterButtonStyle = new GUIStyle(ConsoleWindow.Constants.MiniButton);
#endif
				filterButtonStyle.alignment = TextAnchor.MiddleLeft;
			}

			var count = ConsoleFilter.HiddenCount;
			var aboveThreshold = count >= 1000;
			var text = " " + (aboveThreshold ? "999+" : count.ToString());
			var icon = ConsoleFilter.enabled ? filterIcon : filterIconDisabled;
			var tooltip = count > 1 ? count + " logs" : count + " log";
			if (ConsoleFilter.enabled) tooltip += " hidden.";
			else tooltip += " would be hidden when enabled.";
			var content = new GUIContent(text, icon, tooltip + "\n\nRight click on console logs to set filters. Use the \"Filter\" button to manage filtered logs.");

			var width = count < 10 ? new[] { GUILayout.MinWidth(40) } : count < 100 ? new[] { GUILayout.MinWidth(50) } : Array.Empty<GUILayoutOption>();
			ConsoleFilter.enabled = !GUILayout.Toggle(!ConsoleFilter.enabled, content, filterButtonStyle, width);

			NeedleConsoleSettings.instance.StacktraceOrientation = (NeedleConsoleSettings.StacktraceOrientations)
				EditorGUILayout.CycleButton((int) NeedleConsoleSettings.instance.StacktraceOrientation, new GUIContent[]
				{
					new(orientationVerticalIcon, "Stacktrace orientation: Vertical. \n\nClick to cycle through options"),
					new(orientationHorizontalIcon, "Stacktrace orientation: Horizontal. \n\nClick to cycle through options."),
					new(orientationAutoIcon, $"Stacktrace orientation: Auto. Your stacktrace will use vertical orientation or change to horizontal orientation when your Console window height is below {NeedleConsoleSettings.instance.StacktraceOrientationAutoHeight} pixel. \n\nClick to cycle through options."),
				}, ConsoleWindow.Constants.MiniButton);
			
			// draw vertical line
			// using(new GUIColorScope(Color.black * .3f))
			// 	GUILayout.Box(GUIContent.none, ConsoleWindow.Constants.Toolbar, GUILayout.Width(1), GUILayout.Height(15));
			GUILayout.Space(4);
			Assets.DrawGUILogoMiniButton();
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
