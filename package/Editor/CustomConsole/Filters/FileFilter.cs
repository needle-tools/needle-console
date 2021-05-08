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

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			for (var index = 0; index < Count; index++)
			{
				var ex = this[index];
				if (IsActiveAtIndex(index) && ex == info.file)
				{
					return true;
				}
			}

			return false;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			var fileName = Path.GetFileName(clickedLog.file);
			
			if (string.IsNullOrWhiteSpace(fileName))
				fileName = "All logs without file";
			
			var active = IsActive(clickedLog.file);
			menu.AddItem(new GUIContent("Exclude File " + fileName), active, () =>
			{
				if (!active)
				{
					Add(clickedLog.file);
				}
				else
					SetActive(clickedLog.file, false);
			});
		}
	}
}