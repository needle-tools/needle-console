using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Demystify._Tests
{
	public class Logging
	{
		[MenuItem("Test/Log")]
		private static void Log()
		{
			Debug.Log("A normal log message");
		}

		[MenuItem("Test/LogWarning")]
		private static void Warning()
		{
			Debug.LogWarning("A warning log");
		}

		[MenuItem("Test/LogError")]
		private static void Error()
		{
			Debug.LogError("An error log");
		}

		[MenuItem("Test/LogException")]
		private static void Exception()
		{
			Debug.LogException(new Exception("An exception log", new Exception("a inner exception")));
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
			var words = new[] {"Unity", "Console", "Log", "this", "is", "hello", "Ok", "does", "like", "repeat", "random"};
			for (var i = 0; i < 50; i++)
			{
				var str = string.Empty;
				for (var k = 0; k < 2 + (int) Random.value; k++)
				{
					str += words[Mathf.FloorToInt(Random.value * words.Length)] + " ";
				}
				Debug.Log(str);
			}
		}
	}
}