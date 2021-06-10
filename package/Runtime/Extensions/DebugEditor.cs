using System;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Needle.Console
{
	public static class DebugEditor
	{
		public static string Separator = null;

		/// <summary>
		/// Use to log only in UnityEditor. Avoid string concatenations when calling this method - if you dont this call will have no performance impact on your build.
		/// Call it like DebugEditor.Log(this, MyClass, SomeString, SomeOtherString); 
		/// </summary>
		public static void Log(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.Log(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}

		/// <summary>
		/// Use to log only in UnityEditor. Avoid string concatenations when calling this method - if you dont this call will have no performance impact on your build.
		/// Call it like DebugEditor.Log(MyClass, SomeString, SomeOtherString); 
		/// </summary>
		public static void Log(params object[] obj)
		{
#if UNITY_EDITOR
			Debug.Log(string.Join(Separator ?? string.Empty, obj));
			Separator = null;
#endif
		}
		
		/// <summary>
		/// Use to log only in UnityEditor. Avoid string concatenations when calling this method - if you dont this call will have no performance impact on your build.
		/// Call it like DebugEditor.Log(this, MyClass, SomeString, SomeOtherString); 
		/// </summary>
		public static void LogWarning(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.LogWarning(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}
		
		/// <summary>
		/// Use to log only in UnityEditor. Avoid string concatenations when calling this method - if you dont this call will have no performance impact on your build.
		/// Call it like DebugEditor.Log(this, MyClass, SomeString, SomeOtherString); 
		/// </summary>
		public static void LogError(Object context, params object[] obj)
		{
#if UNITY_EDITOR
			Debug.LogError(string.Join(Separator ?? string.Empty, obj), context);
			Separator = null;
#endif
		}
		
		
	}
}