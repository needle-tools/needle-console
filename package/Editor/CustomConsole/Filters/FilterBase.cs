using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Demystify
{
	[Serializable]
	public abstract class FilterBase<T> : IConsoleFilter
	{
		private bool _isEnabled = true;
		
		public bool Enabled
		{
			get => _isEnabled;
			set
			{
				if (value == _isEnabled) return;
				_isEnabled = value;
				if(ConsoleFilter.Contains(this))
					ConsoleFilter.MarkDirty();
			}
		}

		private List<T> excluded = new List<T>();
		private List<bool> active = new List<bool>();

		public int Count => excluded.Count;
		public T this[int index] => excluded[index];
		public bool IsActiveAtIndex(int index) => active[index];

		public bool IsActive(T element)
		{
			for (var i = 0; i < excluded.Count; i++)
			{
				if (excluded[i].Equals(element))
					return active[i];
			}

			return false;
		}

		public int GetActiveCount() => active.Count(e => e);

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

		public void SetActiveAtIndex(int index, bool active)
		{
			if (this.active[index] != active)
			{
				this.active[index] = active;
				NotifyConsole(active);
			}
		}

		public void SetActive(T element, bool active)
		{
			if (TryGetIndex(element, out var i))
			{
				this.active[i] = active;
				NotifyConsole(active);
			}
		}

		public virtual void Add(T entry, bool isActive = true)
		{
			if (!excluded.Contains(entry))
			{
				excluded.Add(entry);
				active.Add(isActive);
				OnChanged();
				NotifyConsole(isActive);
			}
			else if (isActive && TryGetIndex(entry, out var index))
			{
				active[index] = true;
				NotifyConsole(true);
			}
		}

		public virtual void Remove(int index)
		{
			excluded.RemoveAt(index);
			active.RemoveAt(index);
			OnChanged();
			NotifyConsole(false);
		}

		public virtual void Clear()
		{
			if (Count > 0)
			{
				excluded.Clear();
				active.Clear();
				OnChanged();
				NotifyConsole(false);
			}
		}

		public abstract bool Exclude(string message, int mask, int row, LogEntryInfo info);

		public abstract void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog);

		protected virtual void OnChanged()
		{
		}


		public void OnGUI()
		{
			var header = ObjectNames.NicifyVariableName(GetType().Name);
			var key = "ConsoleFilter" + header;

			header += " (" + GetActiveCount() + "/" + Count + ")";
			if (!Enabled) header += " - Disabled";
			var foldout = SessionState.GetBool(key, true);
			foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, new GUIContent(header), EditorStyles.foldoutHeader, ShowOptionsContextMenu);

			void ShowOptionsContextMenu(Rect r)
			{
				var menu = new GenericMenu();
				if (Enabled)
					menu.AddItem(new GUIContent("Disable"), true, () => Enabled = false);
				else 
					menu.AddItem(new GUIContent("Enable"), false, () => Enabled = true);
				menu.AddSeparator(string.Empty);
				menu.AddItem(new GUIContent("Clear"), false, Clear);
				menu.DropDown(r);
			}
			if(Event.current.type == EventType.MouseDown)
			{
				var lr = GUILayoutUtility.GetLastRect();
				if (Event.current.button == 1 && lr.Contains(Event.current.mousePosition))
				{
					var r = new Rect(Event.current.mousePosition, Vector2.zero);
					ShowOptionsContextMenu(r);
				}
			}
			
			SessionState.SetBool(key, foldout);

			if (foldout)
			{
				EditorGUI.indentLevel++;
				for (var index = 0; index < Count; index++)
				{
					var file = this[index];
					var label = GetLabel(index);
					using (new GUILayout.HorizontalScope())
					{
						var ex = EditorGUILayout.ToggleLeft(new GUIContent(label, file.ToString()), IsActiveAtIndex(index));
						SetActiveAtIndex(index, ex);
						if (GUILayout.Button("x", GUILayout.Width(20)))
						{
							Remove(index);
							index -= 1;
						}
					}
				}

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private void NotifyConsole(bool activateFilteringIfDisabled)
		{
			if (Enabled && ConsoleFilter.Contains(this))
			{
				ConsoleFilter.MarkDirty();
				if (activateFilteringIfDisabled && !ConsoleFilter.enabled)
					ConsoleFilter.enabled = true;
			}
		}
	}
}