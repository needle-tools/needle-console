using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	/// <summary>
	/// Tracks the frame number for each log message by arrival order.
	/// Uses a row offset to handle domain reloads where existing logs
	/// are already in the console before tracking starts.
	/// </summary>
	internal static class LogFrameTracker
	{
		private static readonly Dictionary<int, int> rowToFrame = new Dictionary<int, int>();
		private static int previousCount;
		private static int nextRow;

		[InitializeOnLoadMethod]
		static void Init()
		{
			Application.logMessageReceived -= OnLogMessage;
			Application.logMessageReceived += OnLogMessage;
			EditorApplication.update -= CheckForClear;
			EditorApplication.update += CheckForClear;
			// After domain reload, existing logs are already in the console
			// Start tracking from the current count onwards
			nextRow = LogEntries.GetCount();
			previousCount = nextRow;
			rowToFrame.Clear();
		}

		static void CheckForClear()
		{
			var count = LogEntries.GetCount();
			if (count < previousCount)
			{
				rowToFrame.Clear();
				nextRow = 0;
			}
			previousCount = count;
		}

		static void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			rowToFrame[nextRow] = Time.frameCount;
			nextRow++;
		}

		/// <summary>
		/// Try to get the frame number for a log entry by its row index.
		/// </summary>
		internal static bool TryGetFrame(int row, out int frame)
		{
			return rowToFrame.TryGetValue(row, out frame);
		}
	}
}
