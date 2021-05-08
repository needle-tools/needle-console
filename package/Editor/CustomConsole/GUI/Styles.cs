using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class Styles
	{
		private static GUIStyle toggleButton;

		public static GUIStyle ToggleButton
		{
			get
			{
				if (toggleButton == null)
				{
					toggleButton = new GUIStyle(EditorStyles.miniButton);
				}

				return toggleButton;
			}
		}
	}
}