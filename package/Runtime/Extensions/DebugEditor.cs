using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Needle.Console
{
	public static class DebugEditor
	{
		[Conditional("UNITY_EDITOR")]
		public static void Log(object message, Object context = null)
		{
			Debug.Log(message, context);
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogWarning(object message, Object context = null)
		{
			Debug.LogWarning(message, context);
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogError(object message, Object context = null)
		{
			Debug.LogError(message, context);
		}
	}
}