using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	/// <summary>
	/// Tracks the frame number for each log message via Application.logMessageReceived.
	/// Used to display frame counts in the console prefix.
	/// </summary>
	internal static class LogFrameTracker
	{
		// Key: first line of the log message, Value: frame count when logged
		// We use a ring buffer approach: when the dictionary gets too large, we clear it.
		private static readonly Dictionary<string, int> messageToFrame = new Dictionary<string, int>();
		private const int MaxEntries = 5000;

		[InitializeOnLoadMethod]
		static void Init()
		{
			Application.logMessageReceived += OnLogMessage;
		}

		static void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			if (messageToFrame.Count > MaxEntries)
				messageToFrame.Clear();

			var key = GetKey(condition);
			messageToFrame[key] = Time.frameCount;
		}

		/// <summary>
		/// Try to get the frame number for a log message.
		/// </summary>
		internal static bool TryGetFrame(string message, out int frame)
		{
			if (string.IsNullOrEmpty(message))
			{
				frame = -1;
				return false;
			}
			var key = GetKey(message);
			return messageToFrame.TryGetValue(key, out frame);
		}

		static string GetKey(string message)
		{
			// Use first line only as key to match against LogEntry.message which includes stacktrace
			var newline = message.IndexOf('\n');
			return newline >= 0 ? message.Substring(0, newline) : message;
		}
	}
}
