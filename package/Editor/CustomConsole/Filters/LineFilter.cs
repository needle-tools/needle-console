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

		protected override bool MatchFilter(FileLine entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry.line == info.line && entry.file == info.file;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (string.IsNullOrEmpty(clickedLog.file)) return;
			if (clickedLog.line <= 0) return;
			var fileName = Path.GetFileName(clickedLog.file);
			var fl = new FileLine {file = clickedLog.file, line = clickedLog.line};
			AddContextMenuItem(menu, "Exclude Line " + fileName + ":" + clickedLog.line, fl);
		}
	}
}