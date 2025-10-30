using System;
using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogWithLoggerSample
	{
		private class MyLogHandler : ILogHandler
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
			myLogger.Log("Gameplay", $"Level {UnityEngine.Random.Range(1, 6)} loaded in {UnityEngine.Random.Range(800, 2500)} ms.");
		}
	}
}
