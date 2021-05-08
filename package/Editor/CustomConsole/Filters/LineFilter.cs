using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public struct FileLine
	{
		public string file;
		public int line;

		public override string ToString()
		{
			return file + ":" + line;
		}
	}
	
	[Serializable]
	public class LineFilter : FilterBase<FileLine>
	{
		public override string GetLabel(int index)
		{
			var e = this[index];
			return Path.GetFileName(e.file) + ":" + e.line;
		}

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			for (var i = 0; i < Count; i++)
			{
				if (!IsActiveAtIndex(i)) continue;
				var entry = this[i];
				if (entry.line == info.line && info.file == entry.file)
					return true;
			}

			return false;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (string.IsNullOrEmpty(clickedLog.file)) return;
			if (clickedLog.line <= 0) return;
			
			var fileName = Path.GetFileName(clickedLog.file);
			
			menu.AddItem(new GUIContent("Exclude Line " + fileName + ":" + clickedLog.line), false, () =>
			{
				Add(new FileLine {file = clickedLog.file, line = clickedLog.line});
			});
		}
	}
}