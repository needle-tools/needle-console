using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading.Tasks;
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

		[MenuItem("Test/LogAsync")]
		private static async void LogAsync()
		{
			await Task.Delay(100);
			Debug.Log("A log from async method");
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
			var words = new[] { "Unity", "Console", "Log", "this", "is", "hello", "Ok", "does", "like", "repeat", "random" };
			for (var i = 0; i < 50; i++)
			{
				var str = string.Empty;
				for (var k = 0; k < 2 + (int)(Random.value * 50); k++)
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
			Warning,
			LogWithLongerStacktrace
		};

		[MenuItem("Test/Log Random")]
		private static void LogRandom()
		{
			for (var i = 0; i < 5; i++)
				randomLogMethods[Mathf.FloorToInt(randomLogMethods.Length * Random.value)]();
		}

		[MenuItem("Test/Log Long Stacktrace")]
		private static void LogWithLongerStacktrace()
		{
			LogWithLongerStacktraceInternal(0);
		}

		private static void LogWithLongerStacktraceInternal(int level = 0)
		{
			if (level > 10 || Random.value > .9)
			{
				randomLogMethods[Mathf.FloorToInt(randomLogMethods.Length * Random.value)]();
			}
			else LogWithLongerStacktraceInternal(level + 1);
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

		[MenuItem("Test/My Console Log")]
		private static void LogWithMyConsole()
		{
			Console.MyConsole.Log("MyGameClass Start.");
		}
	}

}

namespace Console
{
	public static class MyConsole
	{
		public static void Log(string message)
		{
			Debug.Log("[MyConsole] " + message);
		}
	}
}