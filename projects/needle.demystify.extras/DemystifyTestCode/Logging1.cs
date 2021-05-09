using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
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

		// [InitializeOnLoadMethod]
		[MenuItem("Test/LogEmptyMessage")]
		private static void LogEmpty()
		{
			Debug.Log(string.Empty);
			Debug.LogWarning("");
			Debug.LogError(null);
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "123");
		}

		[MenuItem("Test/Log Many")]
		private static void LogMany()
		{
			for (var i = 0; i < 100; i++)
			{
				var fn = Path.GetRandomFileName();
				for (var k = 0; k < 1 + (int) (Random.value * 5); k++)
					Debug.Log(fn);
			}
		}
	}
}