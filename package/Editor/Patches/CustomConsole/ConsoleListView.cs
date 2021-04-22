using System;
using System.IO;
using UnityEditor;

namespace Needle.Demystify
{
	internal static class ConsoleListView
	{
		private static readonly LogEntry tempEntry = new LogEntry();
		
		// called from console list with current list view element and console text
		internal static void ModifyText(ListViewElement element, ref string text)
		{
			if (!DemystifySettings.instance.ShowFileName) return;
			
			// LogEntries.SetFilteringText("PatchManager");
			if (LogEntries.GetEntryInternal(element.row, tempEntry)) 
			{
				var file = tempEntry.file;
				if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
				{
					var fileName = Path.GetFileNameWithoutExtension(file);

					string GetText()
					{
						return "[" + fileName + "]";
					}

					var endTimeIndex = text.IndexOf("] ", StringComparison.InvariantCulture);
					// no time:
					if (endTimeIndex == -1)
					{
						text = $"{GetText()} {text}";
					}
					// contains time:
					else
					{
						text = $"{text.Substring(0, endTimeIndex + 1)} {GetText()}{text.Substring(endTimeIndex + 1)}"; 
					}
				}
			}
		}
	}
}