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
			menu.AddItem(new GUIContent("Copy Logs for AI"), false, CopyAllLogs);
			menu.AddItem(new GUIContent("Copy This Log for AI"), false, () => CopySingleLog(itemIndex));
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

		static void FormatEntry(StringBuilder sb, LogEntryInfo entry)
		{
			var message = entry.message;
			if (string.IsNullOrEmpty(message)) return;

			message = StripRichText(message);

			var firstNewline = message.IndexOf('\n');
			var firstLine = firstNewline >= 0 ? message.Substring(0, firstNewline) : message;
			var stacktrace = firstNewline >= 0 ? message.Substring(firstNewline + 1).TrimEnd() : null;

			var location = !string.IsNullOrEmpty(entry.file) ? $" ({entry.file}:{entry.line})" : "";

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
			FormatEntry(sb, entries[itemIndex].entry);

			EditorGUIUtility.systemCopyBuffer = sb.ToString();
			Debug.Log("Copied log entry for AI to clipboard.");
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
				var stacktrace = firstNewline >= 0 ? message.Substring(firstNewline + 1).TrimEnd() : null;

				var location = !string.IsNullOrEmpty(entry.file) ? $" ({entry.file}:{entry.line})" : "";

				if (IsError(entry.mode))
				{
					errors.Append("[ERROR] ").Append(firstLine).AppendLine(location);
					if (!string.IsNullOrEmpty(stacktrace))
						errors.Append("  ").AppendLine(stacktrace.Replace("\n", "\n  "));
					errorCount++;
				}
				else if (IsWarning(entry.mode))
				{
					warnings.Append("[WARN] ").Append(firstLine).AppendLine(location);
					if (!string.IsNullOrEmpty(stacktrace))
						warnings.Append("  ").AppendLine(stacktrace.Replace("\n", "\n  "));
					warningCount++;
				}
				else
				{
					logs.Append("[LOG] ").Append(firstLine).AppendLine(location);
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
			Debug.Log($"Copied {total} log entries for AI to clipboard.");
		}
	}
}
