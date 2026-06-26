using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[Serializable]
#if UNITY_6000_4_OR_NEWER		
	public class ObjectIdFilter : FilterBase<EntityId>
#else
	public class ObjectIdFilter : FilterBase<int>
#endif
	{
		public ObjectIdFilter(ref List<FilterEntry> ids) : base(ref ids)
		{
		}

		public override string GetLabel(int index)
		{
			var id = this[index];
#if UNITY_6000_4_OR_NEWER		
			var obj = EditorUtility.EntityIdToObject(id);
			return obj ? (obj.GetType().Name + " on " + obj.name) : "Missing Object? EntityId=" + id; 
#else
			var obj = EditorUtility.InstanceIDToObject(id);
			return obj ? (obj.GetType().Name + " on " + obj.name) : "Missing Object? InstanceId=" + id; 
#endif
		} 

#if UNITY_6000_4_OR_NEWER
		protected override (FilterResult result, int index) OnFilter(string message, int mask, int row, LogEntryInfo info)
		{
			if (!info.entityId.IsValid()) return (FilterResult.Keep, -1);
			return base.OnFilter(message, mask, row, info);
		}

		protected override bool MatchFilter(EntityId entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry == info.entityId;
		}
		
#else
		protected override (FilterResult result, int index) OnFilter(string message, int mask, int row, LogEntryInfo info)
		{
			if (info.instanceID == 0) return (FilterResult.Keep, -1);
			return base.OnFilter(message, mask, row, info);
		}

		protected override bool MatchFilter(int entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry == info.instanceID;
		}
#endif

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
#if UNITY_6000_4_OR_NEWER
			if (!clickedLog.entityId.IsValid()) return;
			var obj = EditorUtility.EntityIdToObject(clickedLog.entityId);
			if (!obj) return;
			var text = "Instance " + obj.GetType().Name + " on " + obj.name;
			AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, clickedLog.entityId);
			AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, clickedLog.entityId);
#else
			if (clickedLog.instanceID == 0) return;
			var obj = EditorUtility.InstanceIDToObject(clickedLog.instanceID);
			if (!obj) return;
			var text = "Instance " + obj.GetType().Name + " on " + obj.name;
			AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, clickedLog.instanceID);
			AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, clickedLog.instanceID);
#endif
		}
	}
}