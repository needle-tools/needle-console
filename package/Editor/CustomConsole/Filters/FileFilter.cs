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

		public override FilterResult Filter(string message, int mask, int row, LogEntryInfo info)
		{
			for (var index = 0; index < Count; index++)
			{
				var ex = this[index];
				if ((IsActiveAtIndex(index) || IsSoloAtIndex(index)) && ex == info.file)
				{
					var res = IsSoloAtIndex(index) ? FilterResult.Solo : FilterResult.Exclude;
					// Debug.Log((res == FilterResult.Solo) + ", " + Path.GetFileName(info.file));
					return res;
				}
			}

			return FilterResult.Keep;
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
}