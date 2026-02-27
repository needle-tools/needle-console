using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	/// <summary>
	/// Tracks the frame number for each log message by arrival order.
	/// Index in the list corresponds to Unity's internal log entry row index.
	/// </summary>
	internal static class LogFrameTracker
	{
		private static readonly List<int> frames = new List<int>();
		private static int previousCount;

		[InitializeOnLoadMethod]
		static void Init()
		{
			Application.logMessageReceived -= OnLogMessage;
			Application.logMessageReceived += OnLogMessage;
			EditorApplication.update -= CheckForClear;
			EditorApplication.update += CheckForClear;
		}

		static void CheckForClear()
		{
			// Detect when the console is cleared (log count goes down)
			var count = LogEntries.GetCount();
			if (count < previousCount)
				frames.Clear();
			previousCount = count;
		}

		static void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			frames.Add(Time.frameCount);
		}

		/// <summary>
		/// Try to get the frame number for a log entry by its row index.
		/// </summary>
		internal static bool TryGetFrame(int row, out int frame)
		{
			if (row >= 0 && row < frames.Count)
			{
				frame = frames[row];
				return true;
			}
			frame = -1;
			return false;
		}
	}
}
