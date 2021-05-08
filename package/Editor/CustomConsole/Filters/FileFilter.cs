using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class FileFilter : FilterBase<string>
	{
		public override string GetLabel(int index)
		{
			var file = Path.GetFileName(this[index]);
			if (string.IsNullOrEmpty(file))
				file = "Logs without file";
			return file;
		}

		protected override bool MatchFilter(string entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry == info.file;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			var fileName = default(string);
			try
			{
				fileName = Path.GetFileName(clickedLog.file);
				if (string.IsNullOrWhiteSpace(fileName))
					fileName = "All logs without file";
			}
			catch (ArgumentException)
			{
			}

			if (fileName != null)
			{
				AddContextMenuItem(menu, "Exclude File " + fileName, clickedLog.file);
			}
		}
	}
}