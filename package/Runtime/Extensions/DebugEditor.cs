using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	public static class DebugEditor
	{
		public static void Log(object obj, Object context = null)
		{
#if UNITY_EDITOR
			Debug.Log(obj, context);
#endif
		}
		
		public static void LogWarning(object obj, Object context = null)
		{
#if UNITY_EDITOR
			Debug.LogWarning(obj, context);
#endif
		}
		
		public static void LogError(object obj, Object context = null)
		{
#if UNITY_EDITOR
			Debug.LogError(obj, context);
#endif
		}
	}
}