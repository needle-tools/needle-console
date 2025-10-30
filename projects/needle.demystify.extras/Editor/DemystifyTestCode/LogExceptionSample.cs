using System;
using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogExceptionSample
	{
		[MenuItem("Test/LogException")]
		private static void Exception()
		{
			var path = "Assets/Data/settings.json";
			var innerMost = new System.IO.IOException($"The process cannot access the file '{path}' because it is being used by another process.");
			var inner = new UnauthorizedAccessException("Access to the path is denied.", innerMost);
			var ex = new InvalidOperationException("Failed to apply user settings: read configuration.", inner);
			Debug.LogException(ex);
		}
	}
}
