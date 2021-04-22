using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Demystify._Tests
{
	public class Logging
	{
		
		[MenuItem("Test/Log")]
		private static void Log()
		{
			Debug.Log("hello");
		}
		
		[MenuItem("Test/LogWarning")]
		private static void Warning()
		{
			Debug.LogWarning("hello");
		}
		
		
		[MenuItem("Test/LogError")]
		private static void Error()
		{
			Debug.LogError("hello");
		}
		
		[InitializeOnLoadMethod]
		[MenuItem("Test/LogEmptyMessage")]
		private static void LogEmpty()
		{
			Debug.Log(string.Empty);
			Debug.LogError(null);
		}
	}
}