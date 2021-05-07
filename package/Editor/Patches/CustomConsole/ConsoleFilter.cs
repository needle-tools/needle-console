using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	public interface IConsoleFilter
	{
		bool Enabled { get; set; }
		bool Exclude(string message, int mask, int row, LogEntryInfo info);
		void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog);
	}

	internal struct CachedConsoleInfo
	{
		public LogEntryInfo entry;
		public string str;
		public int row;
	}

	public struct LogEntryInfo
	{
		public string message;
		public string file;
		public int line;
		public int instanceID;
		public int mode;

		internal LogEntryInfo(LogEntry entry)
		{
			this.message = entry.message;
			this.file = entry.file;
			this.line = entry.line;
			this.instanceID = entry.instanceID;
			this.mode = entry.mode;
		}
	}

	public static class ConsoleFilter
	{
		internal static bool enabled {
			set
			{
				var _enabled = EditorPrefs.GetBool("ConsoleFilter_Enabled", true);
				if (value == _enabled) return;
				EditorPrefs.SetBool("ConsoleFilter_Enabled", value);
				MarkDirty();
			}
			get => EditorPrefs.GetBool("ConsoleFilter_Enabled", true);
		}
		private static bool isDirty = true;
		private static readonly List<IConsoleFilter> registeredFilters = new List<IConsoleFilter>();
		private static readonly Dictionary<string, bool> cachedLogResultForMask = new Dictionary<string, bool>();
		private static readonly List<LogEntry> logEntries = new List<LogEntry>();
		
		internal static int filteredCount { get; private set; }

		public static void MarkDirty()
		{
			isDirty = true;
		}

		public static bool Contains(IConsoleFilter filter)
		{
			return registeredFilters.Contains(filter);
		}
		
		public static void AddFilter(IConsoleFilter filter)
		{
			if (!registeredFilters.Contains(filter))
			{
				registeredFilters.Add(filter);
				MarkDirty();
			}
		}

		public static void RemoveFilter(IConsoleFilter filter)
		{
			if (registeredFilters.Contains(filter))
			{
				registeredFilters.Remove(filter);
				MarkDirty();
			}
		}

		internal static void AddMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			foreach (var fil in registeredFilters)
				fil.AddLogEntryContextMenuItems(menu, clickedLog);
		}

		private static int _prevCount, _lastFlags;

		internal static bool ShouldUpdate(int logCount)
		{
			if (_prevCount != logCount) return true;
			if (LogEntries.consoleFlags != _lastFlags)
			{
				return true;
			}
			return isDirty;
		}

		internal static void HandleUpdate(int count, List<CachedConsoleInfo> entries)
		{
			using (new ProfilerMarker("ConsoleFilter.HandleUpdate").Auto())
			{
				var start = _prevCount;

				var cleared = count < _prevCount;
				var flagsChanged = LogEntries.consoleFlags != _lastFlags;
				if (isDirty || cleared || flagsChanged)
				{
					start = 0;
					entries.Clear();
					cachedLogResultForMask.Clear();
					logEntries.Clear();
					filteredCount = 0;
				}
				
				isDirty = false;
				_prevCount = count;
				_lastFlags = LogEntries.consoleFlags;
				

				for (var i = start; i < count; i++)
				{
					LogEntry entry = null;
					using (new ProfilerMarker("GetEntryInternal").Auto())
					{
						if (logEntries.Count > i)
						{
							entry = logEntries[i];
						}
						else
						{
							entry = new LogEntry();
							LogEntries.GetEntryInternal(i, entry);
							logEntries.Add(entry);
						}
					}
					
					var mask = 0;
					var preview = default(string);
					using (new ProfilerMarker("GetLinesAndModeFromEntryInternal").Auto())
					{
						LogEntries.GetLinesAndModeFromEntryInternal(i, ConsoleWindow.Constants.LogStyleLineCount, ref mask, ref preview);
					}

					bool isCached = false, cacheRes = false;
					using (new ProfilerMarker("Filter.GetCachedValue").Auto())
					{
						isCached = cachedLogResultForMask.TryGetValue(preview, out cacheRes);
					}

					if (isCached)
					{
						using (new ProfilerMarker("Filter Skip From Cache").Auto())
						{
							if (cacheRes)
								continue;
						}
					}
					else if(enabled)
					{
						LogEntryInfo info = new LogEntryInfo(entry);
						var skip = false;
						foreach (var filter in registeredFilters)
						{
							if (!filter.Enabled) continue;
							using (new ProfilerMarker("Filter Exclude").Auto())
							{
								if (filter.Exclude(preview, mask, i, info))
								{
									filteredCount += 1;
									skip = true;
									break;
								}
							}
						}

						cachedLogResultForMask.Add(preview, skip);
						if (skip) continue;
					}


					// preview += " #" + i;

					entries.Add(new CachedConsoleInfo()
					{
						entry = new LogEntryInfo(entry),
						row = i,
						str = preview
					});
				}
			}
		}
	}
}