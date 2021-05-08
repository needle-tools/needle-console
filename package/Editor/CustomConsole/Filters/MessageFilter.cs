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

		public override FilterResult Filter(string message, int mask, int row, LogEntryInfo info)
		{
			for (var index = 0; index < Count; index++)
			{
				if (IsActiveAtIndex(index) || IsSoloAtIndex(index))
				{
					if (info.message.StartsWith(this[index]))
						return IsSoloAtIndex(index) ? FilterResult.Solo : FilterResult.Exclude;
				}
			}

			return FilterResult.Keep;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			var msg = clickedLog.message;
			msg = msg.Substring(0, Mathf.Min(msg.Length, MaxLenght));
			menu.AddItem(new GUIContent("Exclude Exact Message: \"" + msg.Replace('/', '_') + "\""), false, func: () =>
			{
				Add(msg);
			});
		}
	}
}