using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class CopyLogsForAI
	{
		[InitializeOnLoadMethod]
		static void Init()
		{
			ConsoleList.LogEntryContextMenu += OnContextMenu;
		}

		static void OnContextMenu(GenericMenu menu, int itemIndex)
		{
			menu.AddItem(new GUIContent("Copy/This Log for AI"), false, () => CopySingleLog(itemIndex));
			menu.AddItem(new GUIContent("Copy/All Visible Logs for AI"), false, CopyAllLogs);
		}

		static bool IsError(int mode) =>
			ConsoleList.HasMode(mode, ConsoleWindow.Mode.Assert |
			                          ConsoleWindow.Mode.ScriptingError | ConsoleWindow.Mode.Error |
			                          ConsoleWindow.Mode.StickyError | ConsoleWindow.Mode.AssetImportError |
			                          ConsoleWindow.Mode.ScriptCompileError | ConsoleWindow.Mode.GraphCompileError);

		static bool IsWarning(int mode) =>
			ConsoleList.HasMode(mode, ConsoleWindow.Mode.ScriptingWarning |
			                          ConsoleWindow.Mode.AssetImportWarning | ConsoleWindow.Mode.ScriptCompileWarning);

		static readonly Regex RichTextTags = new Regex(@"<\/?(?:b|i|color|size|a)[^>]*>", RegexOptions.Compiled);

		static string StripRichText(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			return RichTextTags.Replace(text, "");
		}

		static string GetPrefix(int mode)
		{
			if (IsError(mode)) return "[ERROR] ";
			if (IsWarning(mode)) return "[WARN] ";
			return "[LOG] ";
		}

		/// <summary>
		/// Returns true if a file path points to Unity internals (not user code).
		/// </summary>
		static bool IsInternalLocation(string file)
		{
			if (string.IsNullOrEmpty(file)) return true;
			if (file.Contains("Debug.bindings")) return true;
			if (file.Contains("\\build\\output\\unity\\")) return true;
			return false;
		}

		/// <summary>
		/// Unity internal stacktrace lines that are noise for AI debugging.
		/// </summary>
		static readonly string[] InternalStacktracePatterns =
		{
			"UnityEngine.UnitySynchronizationContext",
			"UnityEngine.Debug.",
			"UnityEngine.Logger.",
			"UnityEngine.DebugLogHandler",
			"UnityEngine.UIElements.IMGUIContainer",
			"UnityEngine.UIElements.EventDispatcher",
			"UnityEngine.UIElements.UIElementsUtility",
			"UnityEngine.UIElements.UIEventRegistration",
			"UnityEngine.UIElements.BaseVisualElementPanel",
			"UnityEngine.UIElements.CallbackEventHandler",
			"UnityEngine.UIElements.MouseCaptureDispatchingStrategy",
			"UnityEngine.GUIUtility.ProcessEvent",
			"UnityEditor.HostView.",
			"UnityEditor.DockArea.",
			"System.Runtime.CompilerServices.AsyncVoidMethodBuilder",
		};

		/// <summary>
		/// Cleans a stacktrace by removing Unity internal lines.
		/// For "Copy All", limits to maxLines. For single log copy, keeps all relevant lines.
		/// </summary>
		static string CleanStacktrace(string stacktrace, int maxLines = 0)
		{
			if (string.IsNullOrEmpty(stacktrace)) return null;

			var sb = new StringBuilder();
			var lines = stacktrace.Split('\n');
			var count = 0;
			foreach (var line in lines)
			{
				var trimmed = line.TrimStart();
				if (string.IsNullOrWhiteSpace(trimmed)) continue;

				var isInternal = false;
				foreach (var pattern in InternalStacktracePatterns)
				{
					if (trimmed.Contains(pattern))
					{
						isInternal = true;
						break;
					}
				}
				if (isInternal) continue;

				if (sb.Length > 0) sb.Append('\n');
				sb.Append(line);
				count++;
				if (maxLines > 0 && count >= maxLines) break;
			}
			return sb.Length > 0 ? sb.ToString() : null;
		}

		static string GetLocation(LogEntryInfo entry)
		{
			if (string.IsNullOrEmpty(entry.file) || IsInternalLocation(entry.file))
				return "";
			return $" ({entry.file}:{entry.line})";
		}

		static void FormatEntry(StringBuilder sb, LogEntryInfo entry, int stacktraceMaxLines = 0)
		{
			var message = entry.message;
			if (string.IsNullOrEmpty(message)) return;

			message = StripRichText(message);

			var firstNewline = message.IndexOf('\n');
			var firstLine = firstNewline >= 0 ? message.Substring(0, firstNewline) : message;
			var rawStacktrace = firstNewline >= 0 ? message.Substring(firstNewline + 1).TrimEnd() : null;
			var stacktrace = CleanStacktrace(rawStacktrace, stacktraceMaxLines);

			var location = GetLocation(entry);

			// Add frame info if available
			if (LogFrameTracker.TryGetFrame(message, out var frame))
				sb.Append("F").Append(frame).Append(" ");

			sb.Append(GetPrefix(entry.mode)).Append(firstLine).AppendLine(location);
			if (!string.IsNullOrEmpty(stacktrace))
				sb.Append("  ").AppendLine(stacktrace.Replace("\n", "\n  "));
		}

		static void AppendHeader(StringBuilder sb, string title)
		{
			sb.Append("## ").AppendLine(title);
			sb.Append("Project: ").Append(Application.productName)
				.Append(" | Unity ").AppendLine(Application.unityVersion);
			sb.Append("Project path: ").AppendLine(Application.dataPath.Replace("/Assets", ""));
			sb.Append("Full log: ").AppendLine(Application.consoleLogPath);
		}

		static void CopySingleLog(int itemIndex)
		{
			var entries = ConsoleList.CurrentEntries;
			if (entries == null || itemIndex < 0 || itemIndex >= entries.Count)
			{
				EditorGUIUtility.systemCopyBuffer = "(No log selected)";
				return;
			}

			var sb = new StringBuilder();
			AppendHeader(sb, "Unity Console Log");
			sb.AppendLine();
			FormatEntry(sb, entries[itemIndex].entry); // no stacktrace limit for single log

			EditorGUIUtility.systemCopyBuffer = sb.ToString();
			// No log — clipboard being filled is sufficient feedback
		}

		static void CopyAllLogs()
		{
			var entries = ConsoleList.CurrentEntries;
			if (entries == null || entries.Count == 0)
			{
				EditorGUIUtility.systemCopyBuffer = "(No logs)";
				return;
			}

			var errors = new StringBuilder();
			var warnings = new StringBuilder();
			var logs = new StringBuilder();
			var errorCount = 0;
			var warningCount = 0;
			var logCount = 0;
			const int maxEntries = 200;
			var total = Mathf.Min(entries.Count, maxEntries);

			for (var i = 0; i < total; i++)
			{
				var entry = entries[i].entry;
				var message = entry.message;
				if (string.IsNullOrEmpty(message)) continue;

				message = StripRichText(message);

				var firstNewline = message.IndexOf('\n');
				var firstLine = firstNewline >= 0 ? message.Substring(0, firstNewline) : message;
				var rawStacktrace = firstNewline >= 0 ? message.Substring(firstNewline + 1).TrimEnd() : null;

				var location = GetLocation(entry);

				var framePrefix = "";
				if (LogFrameTracker.TryGetFrame(entry.message, out var frame))
					framePrefix = $"F{frame} ";

				if (IsError(entry.mode))
				{
					var stacktrace = CleanStacktrace(rawStacktrace);
					errors.Append(framePrefix).Append("[ERROR] ").Append(firstLine).AppendLine(location);
					if (!string.IsNullOrEmpty(stacktrace))
						errors.Append("  ").AppendLine(stacktrace.Replace("\n", "\n  "));
					errorCount++;
				}
				else if (IsWarning(entry.mode))
				{
					// Limit warning stacktraces to 5 meaningful lines
					var stacktrace = CleanStacktrace(rawStacktrace, 5);
					warnings.Append(framePrefix).Append("[WARN] ").Append(firstLine).AppendLine(location);
					if (!string.IsNullOrEmpty(stacktrace))
						warnings.Append("  ").AppendLine(stacktrace.Replace("\n", "\n  "));
					warningCount++;
				}
				else
				{
					logs.Append(framePrefix).Append("[LOG] ").Append(firstLine).AppendLine(location);
					logCount++;
				}
			}

			var sb = new StringBuilder();
			AppendHeader(sb, "Unity Console Logs");

			if (errorCount > 0)
			{
				sb.Append("\n### Errors (").Append(errorCount).AppendLine(")");
				sb.Append(errors);
			}

			if (warningCount > 0)
			{
				sb.Append("\n### Warnings (").Append(warningCount).AppendLine(")");
				sb.Append(warnings);
			}

			if (logCount > 0)
			{
				sb.Append("\n### Logs (").Append(logCount).AppendLine(")");
				sb.Append(logs);
			}

			if (entries.Count > maxEntries)
				sb.Append("\n(Showing ").Append(maxEntries).Append(" of ").Append(entries.Count).AppendLine(" entries)");

			EditorGUIUtility.systemCopyBuffer = sb.ToString();
			// No log — clipboard being filled is sufficient feedback
		}
	}
}
