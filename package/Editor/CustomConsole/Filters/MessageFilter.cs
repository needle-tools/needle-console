using System;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class MessageFilter : FilterBase<string>
	{
		public const int MaxLenght = 100;
		
		public override string GetLabel(int index)
		{
			var msg = (string)this[index];
			var lbi = msg.IndexOf("\n", StringComparison.Ordinal);
			if (lbi < 0) lbi = 50;
			return msg.Substring(0, Mathf.Min(msg.Length, lbi));
		}

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			for (var i = 0; i < Count; i++)
			{
				if (IsActive(i) && info.message.StartsWith((string)this[i]))
					return true;
			}

			return false;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			var msg = clickedLog.message;
			msg = msg.Substring(0, Mathf.Min(msg.Length, MaxLenght));
			menu.AddItem(new GUIContent("Exclude Message \"" + msg.Replace('/', '_') + "\""), false, func: () =>
			{
				Add(msg);
			});
		}
	}
}