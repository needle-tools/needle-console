﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class FileFilter : FilterBase<string>
	{
		public FileFilter(List<FilterEntry> list = null) : base(list){}

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
			string fileName;
			try
			{
				fileName = Path.GetFileName(clickedLog.file);
				if (string.IsNullOrWhiteSpace(fileName))
					fileName = "All logs without file";
			}
			catch (ArgumentException)
			{
				// some logs have file paths with invalid characters and ids
				// these come from engine calls I think. they look like <56545453423>
				// we just catch the exception here and ignore those
				return;
			}

			AddContextMenuItem(menu, "Exclude File " + fileName, clickedLog.file);
		}
	}
}