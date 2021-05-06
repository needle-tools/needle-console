using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Profiling;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	public class TempFilterWindow : EditorWindow
	{
		[MenuItem("Console/FilterWindow")]
		private static void Open()
		{
			var window = CreateWindow<TempFilterWindow>();
			window.Show();
		}

		[SerializeField] private FileFilter filter = new FileFilter();

		private void OnEnable()
		{
			filter.window = this;
			ConsoleFilter.RegisterFilter(filter);
		}


		[Serializable]
		public class FileFilter : IConsoleFilter
		{
			internal List<string> excludedFiles = new List<string>();
			internal List<bool> active = new List<bool>();
			public EditorWindow window;


			public bool Exclude(string message, int mask, int row, LogEntryInfo info)
			{
				for (var index = 0; index < excludedFiles.Count; index++)
				{
					var ex = excludedFiles[index];
					if (active[index] && ex == info.file)
					{
						return true;
					}
				}

				return false;
			}

			public void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
			{
				var fileName = Path.GetFileName(clickedLog.file);
				menu.AddItem(new GUIContent("Exclude " + fileName), false, () =>
				{
					excludedFiles.Add(clickedLog.file);
					active.Add(true);
					ConsoleFilter.MarkDirty();
					if (window)
						window.Repaint();
				});
			}
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			ConsoleList.DrawCustom = EditorGUILayout.Toggle("Draw Custom", ConsoleList.DrawCustom);
			for (var index = 0; index < filter.excludedFiles.Count; index++)
			{
				var file = filter.excludedFiles[index];
				var fileName = Path.GetFileName(file);
				if (index >= filter.active.Count) filter.active.Add(true);
				using (new GUILayout.HorizontalScope())
				{
					var ex = EditorGUILayout.ToggleLeft(new GUIContent(fileName, file), filter.active[index]);
					filter.active[index] = ex;
					if (GUILayout.Button("x", GUILayout.Width(30)))
					{
						filter.excludedFiles.RemoveAt(index);
						filter.active.RemoveAt(index);
						index -= 1;
					}
				}
			}

			if (EditorGUI.EndChangeCheck())
			{
				ConsoleFilter.MarkDirty();
				InternalEditorUtility.RepaintAllViews();
			}
		}
	}

	public interface IConsoleFilter
	{
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

	public class ConsoleFilter
	{
		private static bool isDirty = true;
		private static readonly List<IConsoleFilter> registeredFilters = new List<IConsoleFilter>();
		private static readonly Dictionary<string, bool> cachedLogResultForMask = new Dictionary<string, bool>();
		private static readonly Dictionary<int, LogEntry> logEntries = new Dictionary<int, LogEntry>();

		internal static void MarkDirty()
		{
			isDirty = true;
		}
		
		internal static void RegisterFilter(IConsoleFilter filter)
		{
			if (!registeredFilters.Contains(filter))
				registeredFilters.Add(filter);
		}

		internal static void AddMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			foreach (var fil in registeredFilters)
				fil.AddLogEntryContextMenuItems(menu, clickedLog);
		}

		private static int _prevCount;

		internal static bool ShouldUpdate(int logCount)
		{
			if (_prevCount != logCount) isDirty = true;
			_prevCount = logCount;
			return isDirty;
		}

		internal static void HandleUpdate(int count, List<CachedConsoleInfo> entries)
		{
			using (new ProfilerMarker("ConsoleFilter.HandleUpdate").Auto())
			{
				isDirty = false;
				entries.Clear();
				LogEntry entry = new LogEntry();
				cachedLogResultForMask.Clear();

				for (var i = 0; i < count; i++)
				{
					LogEntry GetEntry()
					{
						if (logEntries.TryGetValue(i, out var entry))
							return entry;
						var ne = new LogEntry();
						LogEntries.GetEntryInternal(i, ne);
						logEntries.Add(i, ne);
						return ne;
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

							entry = GetEntry();
						}
					}
					else
					{
						LogEntryInfo info;
						using (new ProfilerMarker("Filter GetEntryInternal").Auto())
						{
							entry = GetEntry();
							info = new LogEntryInfo(entry);
						}

						var skip = false;
						foreach (var filter in registeredFilters)
						{
							using (new ProfilerMarker("Filter Exclude").Auto())
							{
								if (filter.Exclude(preview, mask, i, info))
								{
									skip = true;
									break;
								}
							}
						}

						cachedLogResultForMask.Add(preview, skip);
						if (skip) continue;
					}
					

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