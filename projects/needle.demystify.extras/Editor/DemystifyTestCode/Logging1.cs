using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

		private static Action[] randomLogMethods = {
			LogEmpty,
			Exception,
			Error,
			Log,
			Log,
			Log,
			Warning
		};

		[MenuItem("Test/Log Random")]
		private static void LogRandom()
		{
			for (var i = 0; i < 5; i++)
				randomLogMethods[Mathf.FloorToInt(randomLogMethods.Length * Random.value)]();
		}
		
		
		// Taken from https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Logger.html
		public class MyLogHandler : ILogHandler
		{
			public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
			{
				Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
			}

			public void LogException(Exception exception, UnityEngine.Object context)
			{
				Debug.unityLogger.LogException(exception, context);
			}
		}

		[MenuItem("Test/Log With Logger")]
		private static void LogWithLogger()
		{
			var myLogger = new Logger(new MyLogHandler());
			myLogger.Log("MyGameTag", "MyGameClass Start.");
		}
	}
}