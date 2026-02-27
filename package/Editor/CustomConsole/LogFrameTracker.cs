using System.Collections.Concurrent;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	/// <summary>
	/// Tracks the frame number for each log message.
	/// Used to display frame counts in the console prefix and AI copy output.
	/// </summary>
	internal static class LogFrameTracker
	{
		// ConcurrentDictionary for thread safety since logMessageReceivedThreaded fires on any thread
		private static readonly ConcurrentDictionary<string, int> messageToFrame = new ConcurrentDictionary<string, int>();
		private const int MaxEntries = 5000;
		private static volatile int currentFrame;

		[InitializeOnLoadMethod]
		static void Init()
		{
			// Unsubscribe first to avoid duplicate subscriptions across domain reloads
			Application.logMessageReceivedThreaded -= OnLogMessage;
			Application.logMessageReceivedThreaded += OnLogMessage;
			EditorApplication.update -= TrackFrame;
			EditorApplication.update += TrackFrame;
		}

		static void TrackFrame()
		{
			currentFrame = Time.frameCount;
		}

		static void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			if (messageToFrame.Count > MaxEntries)
				messageToFrame.Clear();

			messageToFrame[condition] = currentFrame;
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

			// Try the first line (LogEntry.message includes stacktrace after first newline)
			var newline = message.IndexOf('\n');
			var key = newline >= 0 ? message.Substring(0, newline) : message;
			return messageToFrame.TryGetValue(key, out frame);
		}
	}
}
