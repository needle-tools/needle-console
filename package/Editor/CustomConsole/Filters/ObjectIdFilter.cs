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

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			if (info.instanceID == 0) return false;
			for (var i = 0; i < Count; i++)
			{
				if(this.IsActive(i) && this[i] == info.instanceID) 
					return true;
			}
			return false;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (clickedLog.instanceID == 0) return;
			var obj = EditorUtility.InstanceIDToObject(clickedLog.instanceID);
			menu.AddItem(new GUIContent("Exclude Instance " + obj.GetType().Name + " on " + obj.name), false, () =>
			{
				Add(clickedLog.instanceID);
			});
		}
	}
}