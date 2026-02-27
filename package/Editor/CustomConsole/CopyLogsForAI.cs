using System.Text;
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
			menu.AddItem(new GUIContent("Copy Logs for AI"), false, CopyLogs);
		}

		static bool IsError(int mode) =>
			ConsoleList.HasMode(mode, ConsoleWindow.Mode.Assert |
			                          ConsoleWindow.Mode.ScriptingError | ConsoleWindow.Mode.Error |
			                          ConsoleWindow.Mode.StickyError | ConsoleWindow.Mode.AssetImportError |
			                          ConsoleWindow.Mode.ScriptCompileError | ConsoleWindow.Mode.GraphCompileError);

		static bool IsWarning(int mode) =>
			ConsoleList.HasMode(mode, ConsoleWindow.Mode.ScriptingWarning |
			                          ConsoleWindow.Mode.AssetImportWarning | ConsoleWindow.Mode.ScriptCompileWarning);

		static void CopyLogs()
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

				// First line is the log message, rest is stacktrace
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
					warningCount++;
				}
				else
				{
					logs.Append("[LOG] ").Append(firstLine).AppendLine(location);
					logCount++;
				}
			}

			var sb = new StringBuilder();
			sb.Append("## Unity Console Logs\nProject: ").Append(Application.productName)
				.Append(" | Unity ").AppendLine(Application.unityVersion);

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
