using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogEmptyMessageSample
	{
		[MenuItem("Test/LogEmptyMessage")]
		private static void LogEmpty()
		{
			Debug.Log(string.Empty);
			Debug.LogWarning("");
			Debug.LogError(null);
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "123");
		}
	}
}
