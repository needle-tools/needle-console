using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Demystify
{
	public enum FilterResult
	{
		Keep = 0,
		Exclude = 1,
		Solo = 2,
	}

	public struct Stats
	{
		public int Excluded { get; private set; }
			
		internal void Add(FilterResult res)
		{
			switch (res)
			{
				case FilterResult.Keep:
					break;
				case FilterResult.Exclude:
					Excluded += 1;
					break;
				case FilterResult.Solo:
					break;
			}
		}
			
		internal void Clear()
		{
			Excluded = 0;
		}
	}
	
	public interface IConsoleFilter
	{
		bool Enabled { get; set; }
		bool HasAnySolo();
		void BeforeFilter();
		FilterResult Filter(string message, int mask, int row, LogEntryInfo info);
		void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog);
		void OnGUI();
		int Count { get; }
		event Action<IConsoleFilter> WillChange, HasChanged;
		int GetExcluded(int index);
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
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var name = Undo.GetCurrentGroupName();
			EditorApplication.update += () =>
			{
				name = Undo.GetCurrentGroupName();
			};
			Undo.undoRedoPerformed += () =>
			{
				if(name.EndsWith(UndoPostfix))
					MarkDirty();
			};
		}

		public const string UndoPostfix = "(Console Filter)";
		
		public static void RegisterUndo(Object obj, string name)
		{
			Undo.RegisterCompleteObjectUndo(obj, $"{name} {UndoPostfix}");
		}

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
		private static readonly List<Stats> registeredFiltersStats = new List<Stats>();
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
				registeredFiltersStats.Add(new Stats());
				MarkDirty();
			}
		}

		public static void RemoveAllFilter()
		{
			if (registeredFilters.Count <= 0) return;
			var anyActive = registeredFilters.Any(r => r.Enabled);
			registeredFilters.Clear();
			registeredFiltersStats.Clear();
			if(anyActive) MarkDirty();
		}

		public static bool RemoveFilter(IConsoleFilter filter)
		{
			if (registeredFilters.TryFindIndex(filter, out var index))
			{
				registeredFilters.RemoveAt(index);
				registeredFiltersStats.RemoveAt(index);
				MarkDirty();
				return true;
			}
			return false;
		}

		public static Stats GetStats(IConsoleFilter filter)
		{
			if (registeredFilters.TryFindIndex(filter, out var i))
				return registeredFiltersStats[i];
			return new Stats();
		}
		
		public static Stats Global { get; private set; }

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

				// reset stats
				for (var index = 0; index < registeredFiltersStats.Count; index++)
				{
					var stat = registeredFiltersStats[index];
					stat.Clear();
					registeredFiltersStats[index] = stat;
					registeredFilters[index].BeforeFilter();
				}
				Global = new Stats();

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
						for (var index = 0; index < registeredFilters.Count; index++)
						{
							var filter = registeredFilters[index];
							if (!filter.Enabled) continue;
							using (new ProfilerMarker("Filter Exclude").Auto())
							{
								var res = filter.Filter(preview, mask, i, info);

								void RegisterStat()
								{
									var stats = registeredFiltersStats[index];
									stats.Add(res);
									registeredFiltersStats[index] = stats;
									
									var glob = Global;
										glob.Add(res);
										Global = glob;
								}
								
								if (anySolo)
								{
									skip = true;
									if (res == FilterResult.Solo)
									{
										skip = false;
										RegisterStat();
										break;
									}
								}
								else
								{
									if (res == FilterResult.Exclude)
									{
										filteredCount += 1;
										skip = true;
										RegisterStat();
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