using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		public int Hidden { get; private set; }

		internal void Add(FilterResult res)
		{
			switch (res)
			{
				case FilterResult.Keep:
					break;
				case FilterResult.Exclude:
					Hidden += 1;
					break;
				case FilterResult.Solo:
					break;
			}
		}

		internal void Clear()
		{
			Hidden = 0;
		}
	}

	public interface IConsoleFilter
	{
		bool Enabled { get; set; }
		bool HasAnySolo();
		void BeforeFilter();
		FilterResult Filter(string message, int mask, int row, LogEntryInfo info);
		void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview);
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
			EditorApplication.update += () => { name = Undo.GetCurrentGroupName(); };
			Undo.undoRedoPerformed += () =>
			{
				if (name.EndsWith(UndoPostfix))
					MarkDirty();
			};
		}

		public const string UndoPostfix = "(Console Filter)";

		public static void RegisterUndo(Object obj, string name)
		{
			Undo.RegisterCompleteObjectUndo(obj, $"{name} {UndoPostfix}");
		}

		internal static bool enabled
		{
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

		private static readonly Dictionary<(string preview, int instanceId), bool> cachedLogResultForMask =
			new Dictionary<(string preview, int instanceId), bool>();

		private static readonly List<LogEntry> logEntries = new List<LogEntry>();

		internal static int HiddenCount => Global.Hidden;

		public static IReadOnlyList<IConsoleFilter> RegisteredFilter => registeredFilters;

		public static void MarkDirty()
		{
			isDirty = true;
			ConsoleList.RequestRepaint();
		}

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
			if (anyActive) MarkDirty();
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

		internal static void AddMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			foreach (var fil in registeredFilters)
				fil.AddLogEntryContextMenuItems(menu, clickedLog, preview);
		}

		private static int _prevCount, _lastFlags;
		internal static bool HasAnyFilterSolo { get; private set; }

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
					Global = new Stats();
					ClearGrouping();
				}

				isDirty = false;
				_prevCount = count;
				_lastFlags = LogEntries.consoleFlags;

				HasAnyFilterSolo = registeredFilters.Any(f => f.HasAnySolo());

				// reset stats
				for (var index = 0; index < registeredFiltersStats.Count; index++)
				{
					var stat = registeredFiltersStats[index];
					stat.Clear();
					registeredFiltersStats[index] = stat;
					registeredFilters[index].BeforeFilter();
				}


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

					bool isCached;
					bool cacheRes;
					using (new ProfilerMarker("Filter.GetCachedValue").Auto())
					{
						isCached = cachedLogResultForMask.TryGetValue((preview, entry.instanceID), out cacheRes);
					}

					if (isCached)
					{
						using (new ProfilerMarker("Filter Skip From Cache").Auto())
						{
							if (cacheRes)
							{
								if (enabled)
								{
									AddGlobalStat(FilterResult.Exclude);
									continue;
								}
							}
						}
					}

					// else if(enabled)
					LogEntryInfo info = new LogEntryInfo(entry);
					var skip = false;
					for (var index = 0; index < registeredFilters.Count; index++)
					{
						var filter = registeredFilters[index];
						if (!filter.Enabled) continue;
						using (new ProfilerMarker("Filter Exclude").Auto())
						{
							var res = filter.Filter(preview, mask, i, info);

							void AddResultToStat()
							{
								var stats = registeredFiltersStats[index];
								stats.Add(res);
								registeredFiltersStats[index] = stats;
							}

							if (HasAnyFilterSolo)
							{
								skip = true;
								if (res == FilterResult.Solo)
								{
									skip = false;
									AddResultToStat();
									break;
								}
							}
							else
							{
								if (res == FilterResult.Exclude)
								{
									skip = true;
									AddResultToStat();
									break;
								}
							}
						}
					}

					if (skip)
					{
						AddGlobalStat(FilterResult.Exclude);
					}

					var key = (preview, entry.instanceID);
					if (!cachedLogResultForMask.ContainsKey(key))
						cachedLogResultForMask.Add(key, skip);
					if (enabled && skip) continue;

					if (!HandleGrouping(entry, i, preview, entries))
					{
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

		private static void AddGlobalStat(FilterResult fr)
		{
			var glob = Global;
			glob.Add(fr);
			Global = glob;
		}

		private static readonly Dictionary<string, int> groupedLogs = new Dictionary<string, int>();
		private static void ClearGrouping() => groupedLogs.Clear();
		private static StringBuilder builder = new StringBuilder();
		// number matcher https://regex101.com/r/D0dFIj/1/
		// non number matcher https://regex101.com/r/VRXwpC/1/
		private static Regex noNumberMatcher = new Regex(@"[^-\d.]+", RegexOptions.Compiled | RegexOptions.Multiline);
		private static bool HandleGrouping(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			var text = preview;
			const string marker = "<group>";
			var start = text.IndexOf(marker, StringComparison.InvariantCulture);
			if (start <= 0) return false;
			const string timestampEnd = "] ";
			var timestampIndex = text.IndexOf(timestampEnd, StringComparison.Ordinal);
			var timestamp = string.Empty;
			if (timestampIndex < start)
			{
				timestamp = text.Substring(0, timestampIndex + timestampEnd.Length);
			}
			var match = noNumberMatcher.Match(text);
			text = text.Substring(start + marker.Length).TrimStart();
			if (match.Success)
			{
				builder.Clear();
				var key = builder.Append(entry.file).Append("::").Append(entry.line).Append("::").Append(match.Value).ToString();
				builder.Clear();
				text = builder.Append(timestamp).Append(text).ToString();
				var newEntry = new CachedConsoleInfo()
				{
					entry = new LogEntryInfo(entry),
					row = row,
					str = text
				};
				if (groupedLogs.TryGetValue(key, out var val))
				{
					var ex = entries[val];
					newEntry.row = ex.row;
					entries[val] = newEntry;
				}
				else
				{
					groupedLogs.Add(key, entries.Count);
					entries.Add(newEntry);
				}

				return true;
			}


			// https://regex101.com/r/D0dFIj/1
			// bool isInNumberSequence;
			// for (var index = 0; index < text.Length; index++)
			// {
			// 	var c = text[index];
			// 	// if(int.TryParse(c, out _
			// }

			// if (float.TryParse(text, out var res))  
			// {
			// 	
			// 	return true; 
			// }

			return false;
		}
	}
}