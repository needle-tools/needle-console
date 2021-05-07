using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[SerializeField]
	public class LineFilter : BaseFilterWithActiveState<(string file, int line)>
	{
		public override string GetLabel(int index)
		{
			return this[index].file + ":" + this[index].line;
		}

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			for (var i = 0; i < Count; i++)
			{
				if (!IsActive(i)) continue;
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
				Add((clickedLog.file, clickedLog.line));
			});
		}
	}
}