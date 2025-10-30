using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogErrorSample
	{
		[MenuItem("Test/LogError")]
		private static void Error()
		{
			Debug.LogError("Assets: Failed to load texture at 'Assets/Textures/Wood.png' (GUID: 12ab34cd56ef). File not found or unreadable.");
		}
	}
}
