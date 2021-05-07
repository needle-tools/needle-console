using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class FileFilter : BaseFilterWithActiveState<string>
	{
		public EditorWindow window;

		public override string GetLabel(int index)
		{
			var file = Path.GetFileName(this[index]);
			if (string.IsNullOrEmpty(file))
				file = "Editor Message";
			return file;
		}

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			for (var index = 0; index < Count; index++)
			{
				var ex = this[index];
				if (IsActive(index) && ex == info.file)
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
				fileName = "No-File";
			
			menu.AddItem(new GUIContent("Exclude " + fileName), false, () =>
			{
				Add(clickedLog.file);
				if (window) window.Repaint();
			});
		}
	}
}