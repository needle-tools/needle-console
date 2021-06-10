using System.Diagnostics;
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
	}
}