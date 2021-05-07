using System;
using System.Collections.Generic;
using UnityEditor;

namespace Needle.Demystify
{
	[Serializable]
	public abstract class BaseFilterWithActiveState<T> : IConsoleFilter
	{
		private List<T> excluded = new List<T>();
		private List<bool> active = new List<bool>();

		public int Count => excluded.Count;
		public T this[int index] => excluded[index];
		public bool IsActive(int index) => active[index];

		public abstract string GetLabel(int index);

		public bool TryGetIndex(T element, out int index)
		{
			for (var i = 0; i < excluded.Count; i++)
			{
				if (excluded[i].Equals(element))
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public bool Contains(T element) => excluded.Contains(element);

		public bool IsActive(T element)
		{
			for (var i = 0; i < excluded.Count; i++)
			{
				if (excluded[i].Equals(element))
					return active[i];
			}

			return false;
		}

		public void SetActive(int index, bool active)
		{
			if (this.active[index] != active)
			{
				this.active[index] = active;
				if (ConsoleFilter.Contains(this))
					ConsoleFilter.MarkDirty();
			}
		}

		public virtual void Add(T entry, bool isActive = true)
		{
			if (!excluded.Contains(entry))
			{
				excluded.Add(entry);
				active.Add(isActive);
				OnChanged();
				if (ConsoleFilter.Contains(this))
					ConsoleFilter.MarkDirty();
			}
		}

		public virtual void Remove(int index)
		{
			excluded.RemoveAt(index);
			active.RemoveAt(index);
			OnChanged();
			if (ConsoleFilter.Contains(this))
				ConsoleFilter.MarkDirty();
		}

		public abstract bool Exclude(string message, int mask, int row, LogEntryInfo info);

		public abstract void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog);

		protected virtual void OnChanged(){}
	}
}