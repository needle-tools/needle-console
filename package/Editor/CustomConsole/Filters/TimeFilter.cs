using System;
using System.Globalization;
using UnityEditor;


namespace Needle.Demystify
{
    public class TimeFilter : FilterBase<DateTime>
    { 
        public override string GetLabel(int index)
        {
            return this[index].ToString("hh:mm:ss");
        }

        protected override bool MatchFilter(DateTime entry, int index, string message, int mask, int row, LogEntryInfo info)
        {
            return false;
        }

        public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
        {
            var start = preview.IndexOf("[", StringComparison.InvariantCulture);
            if (start < 0) return;
            var end = preview.IndexOf("]", StringComparison.InvariantCulture);
            if (end < 0 || end <= start) return;
            var time = preview.Substring(start+1, end - 1);
            if (string.IsNullOrEmpty(time)) return;
            if (DateTime.TryParse(time, out var dt))
            {
                AddContextMenuItem_Hide(menu, HideMenuItemPrefix + dt.ToString("hh:mm:ss"), dt);
            }
        }

        private bool TryGetTime(string str, out DateTime dt)
        {
            var start = str.IndexOf("[", StringComparison.InvariantCulture);
            if (start < 0)
            {
                dt = DateTime.MaxValue;
                return false;
            }
            var end = str.IndexOf("]", StringComparison.InvariantCulture);
            if (end < 0 || end <= start)
            {
                dt = DateTime.MaxValue;
                return false;
            }
            var time = str.Substring(start+1, end - 1);
            if (string.IsNullOrEmpty(time))
            {
                dt = DateTime.MaxValue;
                return false;
            }
            return DateTime.TryParse(time, out dt);
        }
    }
}
