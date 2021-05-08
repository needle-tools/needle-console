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
		
		protected override bool MatchFilter(string entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return info.message.StartsWith(entry);
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			var message = clickedLog.message;
			message = message.Substring(0, Mathf.Min(message.Length, MaxLenght));
			var text = "Exclude Exact Message: \"" + message.Replace('/', '_') + "\"";
			AddContextMenuItem(menu, text, message);
		}
	}
}