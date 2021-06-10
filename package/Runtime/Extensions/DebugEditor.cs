using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	public static class DebugEditor
	{
		public static string Separator = null;
		
		public static void Log(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.Log(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}

		public static void Log(params object[] obj)
		{
#if UNITY_EDITOR
			Debug.Log(string.Join(Separator ?? string.Empty, obj));
			Separator = null;
#endif
		}
		
		public static void LogWarning(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.LogWarning(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}
		
		public static void LogError(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.LogError(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}
	}
}