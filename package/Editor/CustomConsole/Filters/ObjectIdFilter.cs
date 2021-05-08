using System;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[Serializable]
	public class ObjectIdFilter : FilterBase<int>
	{
		public override string GetLabel(int index)
		{
			var id = this[index];
			var obj = EditorUtility.InstanceIDToObject(id);
			return obj ? (obj.GetType().Name + " on " + obj.name) : "Missing Object? InstanceId=" + id;
		} 

		public override FilterResult Filter(string message, int mask, int row, LogEntryInfo info)
		{
			if (info.instanceID == 0) return FilterResult.Keep;
			for (var index = 0; index < Count; index++)
			{
				if (IsActiveAtIndex(index) || IsSoloAtIndex(index))
				{
					if(this[index] == info.instanceID) 
						return IsSoloAtIndex(index) ? FilterResult.Solo : FilterResult.Exclude;
				}
			}
			return FilterResult.Keep;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (clickedLog.instanceID == 0) return;
			var obj = EditorUtility.InstanceIDToObject(clickedLog.instanceID);
			if (!obj) return;
			menu.AddItem(new GUIContent("Exclude Instance " + obj.GetType().Name + " on " + obj.name), false, () =>
			{
				Add(clickedLog.instanceID);
			});
		}
	}
}