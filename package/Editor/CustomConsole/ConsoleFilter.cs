using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public enum FilterResult
	{
		Keep = 0,
		Exclude = 1,
		Solo = 2,
	}
	
	public interface IConsoleFilter
	{
		bool Enabled { get; set; }
		bool HasAnySolo();
		FilterResult Filter(string message, int mask, int row, LogEntryInfo info);
		void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog);
		void OnGUI();
		int Count { get; }
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
		private static readonly Dictionary<(string preview, int instanceId), bool> cachedLogResultForMask = new Dictionary<(string preview, int instanceId), bool>();
		private static readonly List<LogEntry> logEntries = new List<LogEntry>();
		
		internal static int filteredCount { get; private set; }

		public static IReadOnlyList<IConsoleFilter> RegisteredFilter => registeredFilters;

		public static void MarkDirty() => isDirty = true;

		public static bool Contains(IConsoleFilter filter) => registeredFilters.Contains(filter);

		public static void AddFilter(IConsoleFilter filter)
		{
			if (!registeredFilters.Contains(filter))
			{
				registeredFilters.Add(filter);
				MarkDirty();
			}
		}

		public static void RemoveAllFilter()
		{
			if (registeredFilters.Count <= 0) return;
			var anyActive = registeredFilters.Any(r => r.Enabled);
			registeredFilters.Clear();
			if(anyActive) MarkDirty();
		}

		public static bool RemoveFilter(IConsoleFilter filter)
		{
			if (registeredFilters.Contains(filter))
			{
				registeredFilters.Remove(filter);
				MarkDirty();
				return true;
			}
			return false;
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

				var anySolo = registeredFilters.Any(f => f.HasAnySolo());
				Debug.Log(anySolo);

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
						isCached = cachedLogResultForMask.TryGetValue((preview, entry.instanceID), out cacheRes);
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
								var res = filter.Filter(preview, mask, i, info);
								if (anySolo)
								{
									skip = true;
									if (res == FilterResult.Solo)
									{
										skip = false;
										break;
									}
								}
								else
								{
									if (res == FilterResult.Exclude)
									{
										filteredCount += 1;
										skip = true;
										break;
									}
								}
							}
						}

						cachedLogResultForMask.Add((preview, entry.instanceID), skip);
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