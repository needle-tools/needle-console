using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleList
	{
		internal static bool DrawCustom
		{
			get => DemystifySettings.instance.CustomList;
			set => DemystifySettings.instance.CustomList = value;
		}

		internal static IReadOnlyList<CachedConsoleInfo> CurrentEntries => currentEntries;

		internal delegate void LogEntryContextMenuDelegate(GenericMenu menu, int itemIndex);

		internal static event LogEntryContextMenuDelegate LogEntryContextMenu;

		private static readonly List<ICustomLogDrawer> customDrawers = new List<ICustomLogDrawer>();

		internal static void RegisterCustomDrawer(ICustomLogDrawer drawer)
		{
			if (drawer == null) return;
			if (customDrawers.Contains(drawer)) return;
			customDrawers.Add(drawer);
		}

		internal static void UnregisterCustomDrawer(ICustomLogDrawer drawer)
		{
			if (drawer == null) return;
			if (!customDrawers.Contains(drawer)) return;
			customDrawers.Remove(drawer);
		}

		private static Vector2 scroll
		{
			get => SessionState.GetVector3("ConsoleList-Scroll", Vector3.zero);
			set => SessionState.SetVector3("ConsoleList-Scroll", value);
		}

		private static Vector2 scrollStacktrace;
		private static readonly List<CachedConsoleInfo> currentEntries = new List<CachedConsoleInfo>();
		private static readonly List<Rect> currentEntriesRects = new List<Rect>();
		private static readonly SplitterState spl = SplitterState.FromRelative(new float[] {70, 30}, new float[] {32, 32}, null);

		private static int selectedRowIndex = -1, previouslySelectedRowIndex = -2, rowDoubleClicked = -1;
		private static int selectedRowNumber = -1;
		private static string selectedText;

		private static int collapsedFlag = 1 << 0;

		/// <summary>
		/// if scrollbar was at bottom, signal to continue scroll to bottom when logs change
		/// can and should be interrupted by focus or click or manual scroll
		/// </summary>
		private static bool isAutoScrolling;

		private static bool logsCountChanged, logsAdded;
		private static int previousLogsCount, logCountDiff;
		private static DateTime lastClickTime;
		private static GUIStyle logStyle;

		private static bool HasFlag(int flags) => (LogEntries.consoleFlags & (int) flags) != 0;
		private static bool HasMode(int mode, ConsoleWindow.Mode modeToCheck) => (uint) ((ConsoleWindow.Mode) mode & modeToCheck) > 0U;

		private static ConsoleWindow _consoleWindow;
		private static bool shouldScrollToSelectedItem;
		private static GUIContent tempContent;
		private static Rect strRect;
		private static ListViewElement element;
		private static int xOffset;
		private static float lineHeight;

		internal static void RequestRepaint()
		{
			if (_consoleWindow) _consoleWindow.Repaint();
		}

		internal static bool IsCollapsed() => HasFlag(collapsedFlag);

		internal static bool OnDrawList(ConsoleWindow console)
		{
			_consoleWindow = console;

			if (!DrawCustom)
				return true;

			// scroll = EditorGUILayout.BeginScrollView(scroll);
			int count;

			if (Event.current.type == EventType.Repaint)
			{
				try
				{
					LogEntries.StartGettingEntries();
					count = LogEntries.GetCount();
					logCountDiff = count - previousLogsCount;
					logsAdded = count > previousLogsCount;
					logsCountChanged = count != previousLogsCount;
					previousLogsCount = count;
					if (count <= 0)
					{
						selectedText = null;
						selectedRowIndex = -1;
						previouslySelectedRowIndex = -1;
					}

					var shouldUpdateLogs = ConsoleFilter.ShouldUpdate(count);
					if (shouldUpdateLogs)
					{
						if (selectedRowIndex >= 0 && !logsAdded)
						{
							shouldScrollToSelectedItem = true;
						}

						ConsoleFilter.HandleUpdate(count, currentEntries);
					}
				}
				finally
				{
					LogEntries.EndGettingEntries();
				}
			}

			if (Event.current.type == EventType.MouseDown)
			{
				rowDoubleClicked = -1;
				previouslySelectedRowIndex = selectedRowIndex;
				// selectedRowIndex = -1;
			}


			SplitterGUILayout.BeginVerticalSplit(spl);

			var lineCount = ConsoleWindow.Constants.LogStyleLineCount;
			xOffset = ConsoleWindow.Constants.LogStyleLineCount == 1 ? 2 : 14;
			var yTop = EditorGUIUtility.singleLineHeight + 3;
			lineHeight = EditorGUIUtility.singleLineHeight * lineCount + 3;
			count = currentEntries.Count;
			var scrollAreaHeight = Screen.height - spl.realSizes[1] - 44;

			var contentHeight = count * lineHeight;
			if (Event.current.type == EventType.Repaint)
			{
				if (customDrawers.Any())
				{
					contentHeight = 0;
					var linesDrawnDefault = currentEntries.Count;
					foreach (var dr in customDrawers)
					{
						contentHeight += dr.GetContentHeight(lineHeight, currentEntries.Count, out var linesHandled);
						linesDrawnDefault -= (int) linesHandled;
					}

					contentHeight += lineHeight * Mathf.Max(0, linesDrawnDefault);
				}
			}

			var scrollArea = new Rect(0, yTop, Screen.width - 3, scrollAreaHeight);
			var width = Screen.width - 3;
			if (contentHeight > scrollArea.height)
				width -= 13;
			var contentSize = new Rect(0, 0, width, contentHeight);

			// scroll to bottom if logs changed and it was at the bottom previously
			if (!shouldScrollToSelectedItem)
			{
				if (isAutoScrolling && logsAdded)
				{
					SetScroll(Mathf.Max(0, contentHeight - scrollAreaHeight));
				}
				else if (contentHeight < scrollAreaHeight)
				{
					SetScroll(0);
				}
			}


			scroll = GUI.BeginScrollView(scrollArea, scroll, contentSize);

			var position = new Rect(0, 0, width, lineHeight);
			element = new ListViewElement();
			if (logStyle == null)
			{
				logStyle = new GUIStyle(ConsoleWindow.Constants.LogSmallStyle);
				logStyle.alignment = TextAnchor.UpperLeft;
			}

			tempContent = new GUIContent();
			var evt = Event.current;
			if (evt.type == EventType.Repaint || evt.type == EventType.MouseUp)
			{
				try
				{
					LogEntries.StartGettingEntries();

					if (evt.type == EventType.Repaint)
						currentEntriesRects.Clear();

					for (var k = 0; k < currentEntries.Count; k++)
					{
						var isVisible = IsVisible(position);
						bool IsVisible(Rect r) => r.y + r.height >= scroll.y && r.y <= scroll.y + scrollAreaHeight;

						if (Event.current.type == EventType.Repaint)
						{
							void RegisterRect(Rect _rect)
							{
								currentEntriesRects.Add(_rect);
							}

							position.height = lineHeight;
							strRect = position;
							strRect.x += xOffset;
							strRect.y -= 1;
							strRect.height -= position.height * .15f;

							var handledByCustomDrawer = false;
							foreach (var drawer in customDrawers)
							{
								if (drawer.OnDrawEntry(k, selectedRowIndex == k, position, isVisible, out var res))
								{
									position.height = res;
									RegisterRect(position);
									position.y += res;
									handledByCustomDrawer = true;
									break;
								}
							}

							if (handledByCustomDrawer) continue;
							RegisterRect(position);
							position.y += DrawDefaultRow(k, position);
						}

						var rect = currentEntriesRects[k];
						if (Event.current.type == EventType.MouseUp && IsVisible(rect))
						{
							if (Event.current.button == 0)
							{
								if (rect.Contains(Event.current.mousePosition))
								{
									var entry = currentEntries[k].entry;
									isAutoScrolling = false;
									SelectRow(k);

									if (previouslySelectedRowIndex == selectedRowIndex)
									{
										var td = (DateTime.Now - lastClickTime).Seconds;
										if (td < 1)
											rowDoubleClicked = currentEntries[selectedRowIndex].row;
									}
									else
									{
										if (entry.instanceID != 0)
											EditorGUIUtility.PingObject(entry.instanceID);
									}

									lastClickTime = DateTime.Now;
									Event.current.Use();
									console.Repaint();
									break;
								}
							}
							else if (Event.current.button == 1 && IsVisible(rect))
							{
								if (rect.Contains(Event.current.mousePosition))
								{
									var item = currentEntries[k];
									var menu = new GenericMenu();
									if (ConsoleFilter.RegisteredFilter.Count > 0)
									{
										ConsoleFilter.AddMenuItems(menu, item.entry, item.str);
									}

									AddConfigMenuItems(menu, k);
									menu.ShowAsContext();
									Event.current.Use();
									break;
								}
							}
						}

						if (shouldScrollToSelectedItem && selectedRowNumber == currentEntries[k].row)
						{
							shouldScrollToSelectedItem = false;
							var scrollTo = position.y;
							if (contentHeight > scrollAreaHeight)
							{
								scrollTo -= scrollAreaHeight * .5f - lineHeight;
							}

							SetScroll(scrollTo);
							RequestRepaint();
						}
					}
				}
				finally
				{
					LogEntries.EndGettingEntries();
				}
			}

			if (Event.current.type == EventType.ScrollWheel)
			{
				isAutoScrolling = false;
			}

			if (selectedRowIndex >= 0 && Event.current.type == EventType.KeyDown)
			{
				HandleKeyboardInput(position, console, scrollAreaHeight, lineHeight);
			}

			if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
			{
				var menu = new GenericMenu();
				AddConfigMenuItems(menu, selectedRowIndex);
				menu.ShowAsContext();
			}

			if (rowDoubleClicked >= 0)
			{
				LogEntries.RowGotDoubleClicked(rowDoubleClicked);
				rowDoubleClicked = -1;
				previouslySelectedRowIndex = -1;
			}

			GUI.EndScrollView();

			if (Event.current.type == EventType.Repaint && selectedRowIndex < 0)
			{
				var diffToBottom = (contentHeight - scrollAreaHeight) - scroll.y;
				isAutoScrolling = diffToBottom <= (lineHeight * Mathf.Max(0, logCountDiff)) || contentHeight < scrollAreaHeight;
			}

			// Display active text (We want word wrapped text with a vertical scrollbar)
			scrollAreaHeight += 2;
			GUILayout.Space(scrollAreaHeight);
			scrollStacktrace = GUILayout.BeginScrollView(scrollStacktrace, ConsoleWindow.Constants.Box);
			SeparatorLine.Draw(scrollStacktrace.y);

			var didDrawStacktrace = false;
			var text = selectedText ?? string.Empty;
			scrollAreaHeight += 1;
			var stacktraceContentRect = new Rect(0, scrollAreaHeight, width, Screen.height - scrollAreaHeight);
			try
			{
				foreach (var drawer in customDrawers)
				{
					if (drawer.OnDrawStacktrace(selectedRowIndex, text, stacktraceContentRect))
					{
						didDrawStacktrace = true;
						break;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			if (!didDrawStacktrace)
			{
				DrawDefaultStacktrace(text);
			}

			GUILayout.EndScrollView();
			SplitterGUILayout.EndVerticalSplit();
			return false;
		}

		internal static void DrawDefaultStacktrace(string message)
		{
			var stackWithHyperlinks = ConsoleWindow.StacktraceWithHyperlinks(message);
			var stacktraceHeight = ConsoleWindow.Constants.MessageStyle.CalcHeight(GUIContent.Temp(stackWithHyperlinks), Screen.width);
			DrawDefaultStacktrace(stackWithHyperlinks, stacktraceHeight);
		}

		internal static void DrawDefaultStacktrace(string stacktraceWithHyperlinks, float height)
		{
			try
			{
				EditorGUILayout.SelectableLabel(stacktraceWithHyperlinks,
					ConsoleWindow.Constants.MessageStyle,
					GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true),
					GUILayout.MinHeight(height + EditorGUIUtility.singleLineHeight * 2));
			}
			catch 
				// (ArgumentException ex)
			{
				// Debug.LogException(ex);
			}
		}

		internal static float DrawDefaultRow(int index, Rect rect)
		{
			var row = index;
			var item = currentEntries[index];
			var entryIsSelected = selectedRowNumber == item.row;
			var entry = item.entry;
			element.row = item.row;
			element.position = rect;

			// draw background
			void DrawBackground(Color col)
			{
				var prevCol = GUI.color;
				GUI.color = col;
				GUI.DrawTexture(rect, Texture2D.whiteTexture);
				GUI.color = prevCol;
			}

			bool IsOdd() => row % 2 != 0;
			var allowColors = DemystifySettings.instance.RowColors;
			if (entryIsSelected)
			{
				DrawBackground(new Color(.2f, .5f, .8f, .5f));
			}
			else if (allowColors && HasMode(entry.mode, ConsoleWindow.Mode.ScriptCompileError))
			{
				DrawBackground(IsOdd() ? new Color(1, 0, 0, .2f) : new Color(1, .2f, .25f, .25f));
			}
			else if (allowColors && HasMode(entry.mode,
				ConsoleWindow.Mode.ScriptingError | ConsoleWindow.Mode.Error | ConsoleWindow.Mode.StickyError |
				ConsoleWindow.Mode.AssetImportError))
			{
				DrawBackground(IsOdd() ? new Color(1, 0, 0, .1f) : new Color(1, .2f, .25f, .15f));
			}
			else if (allowColors && HasMode(entry.mode,
				ConsoleWindow.Mode.ScriptingWarning | ConsoleWindow.Mode.AssetImportWarning | ConsoleWindow.Mode.ScriptCompileWarning))
			{
				DrawBackground(IsOdd() ? new Color(.5f, .5f, 0, .08f) : new Color(1, 1f, .1f, .04f));
			}
			else if (IsOdd())
			{
				DrawBackground(new Color(0, 0, 0, .1f));
			}

			// draw icon
			GUIStyle iconStyle = ConsoleWindow.GetStyleForErrorMode(entry.mode, true, ConsoleWindow.Constants.LogStyleLineCount == 1);
			Rect iconRect = rect;
			iconRect.y += 2;
			iconStyle.Draw(iconRect, false, false, entryIsSelected, false);

			// draw text
			var preview = item.str; // + " - " + item.entry.mode;
			strRect.x = xOffset;
			ConsoleTextPrefix.ModifyText(element, ref preview);
			// preview += item.entry.instanceID;
			GUI.Label(strRect, preview, logStyle);

			// draw badge
			var collapsed = IsCollapsed();
			var isGrouped = item.collapseCount > 0;
			if (collapsed || isGrouped)
			{
				var badgeRect = element.position;
				badgeRect.height = lineHeight;
				var entryCount = collapsed ? LogEntries.GetEntryCount(item.row) : 0;
				entryCount += item.collapseCount;
				// if (collapsed && item.groupSize > 0) entryCount -= 1;
				
				tempContent.text = entryCount.ToString(CultureInfo.InvariantCulture);
				var badgeSize = ConsoleWindow.Constants.CountBadge.CalcSize(tempContent);
				if (ConsoleWindow.Constants.CountBadge.fixedHeight > 0)
					badgeSize.y = ConsoleWindow.Constants.CountBadge.fixedHeight;
				badgeRect.xMin = badgeRect.xMax - badgeSize.x;
				badgeRect.yMin += ((badgeRect.yMax - badgeRect.yMin) - badgeSize.y) * 0.5f;
				badgeRect.x -= 5f;
				GUI.Label(badgeRect, tempContent, ConsoleWindow.Constants.CountBadge);
			}

			return rect.height;
		}

		private static void HandleKeyboardInput(Rect position, ConsoleWindow console, float scrollAreaHeight, float lineHeight)
		{
			switch (Event.current.keyCode)
			{
				case KeyCode.Escape:
					selectedRowIndex = -1;
					selectedRowNumber = -1;
					selectedText = null;
					isAutoScrolling = false;
					break;

				// auto-scroll
				case KeyCode.B:
					isAutoScrolling = true;
					break;

				case KeyCode.F:
					shouldScrollToSelectedItem = true;
					isAutoScrolling = false;
					RequestRepaint();
					break;

				case KeyCode.PageDown:
				case KeyCode.D:
				case KeyCode.RightArrow:
					if (selectedRowIndex >= 0)
					{
						var newIndex = selectedRowIndex + (int) (scrollAreaHeight / lineHeight);
						newIndex = Mathf.Clamp(newIndex, 0, currentEntries.Count);
						if (newIndex >= 0 && (newIndex) < currentEntries.Count)
						{
							SetScroll(scroll.y + (newIndex - selectedRowIndex) * lineHeight);
							SelectRow(newIndex);
							console.Repaint();
						}
					}

					break;

				case KeyCode.PageUp:
				case KeyCode.A:
				case KeyCode.LeftArrow:
					if (selectedRowIndex >= 0)
					{
						var newIndex = selectedRowIndex - (int) (scrollAreaHeight / lineHeight);
						newIndex = Mathf.Clamp(newIndex, 0, currentEntries.Count);
						if (newIndex >= 0 && (newIndex) < currentEntries.Count)
						{
							SetScroll(scroll.y + (newIndex - selectedRowIndex) * lineHeight);
							SelectRow(newIndex);
							console.Repaint();
						}
					}

					break;

				case KeyCode.S:
				case KeyCode.DownArrow:
					if (selectedRowIndex >= 0 && (selectedRowIndex + 1) < currentEntries.Count)
					{
						SetScroll(scroll.y + lineHeight);
						SelectRow(selectedRowIndex + 1);
						// if(selectedRow * lineHeight > scroll.y + (contentHeight - scrollAreaHeight))
						// 	scrollArea
						console.Repaint();
					}

					break;
				case KeyCode.W:
				case KeyCode.UpArrow:
					if (currentEntries.Count > 0 && selectedRowIndex > 0 && selectedRowIndex < currentEntries.Count)
					{
						SelectRow(selectedRowIndex - 1);
						SetScroll(scroll.y - lineHeight);
						if (scroll.y < 0) SetScroll(0);
						console.Repaint();
					}

					break;
				case KeyCode.Space:
					if (selectedRowIndex >= 0 && currentEntries.Count > 0)
					{
						var menu = new GenericMenu();
						if (ConsoleFilter.RegisteredFilter.Count > 0)
						{
							var info = currentEntries[selectedRowIndex];
							ConsoleFilter.AddMenuItems(menu, info.entry, info.str);
						}

						AddConfigMenuItems(menu, selectedRowIndex);
						var rect = position;
						rect.y = selectedRowIndex * lineHeight;
						menu.DropDown(rect);
					}

					break;
				case KeyCode.Return:
					if (selectedRowIndex > 0)
					{
						rowDoubleClicked = currentEntries[selectedRowIndex].row;
					}

					break;
			}
		}

		private static void SelectRow(int index)
		{
			if (index >= 0 && index < currentEntries.Count)
			{
				selectedRowIndex = index;
				var i = currentEntries[index];
				selectedRowNumber = i.row;
				selectedText = i.entry.message;
			}
		}

		private static void SetScroll(float y)
		{
			var s = scroll;
			s.y = y;
			scroll = s;
		}

		// private static bool IsVisible(float y, float scrollPos, float contentHeight)
		// {
		// 	return y >= scrollPos && y <= scrollPos + contentHeight;
		// }

		private static void AddConfigMenuItems(GenericMenu menu, int itemIndex)
		{
			try
			{
				LogEntryContextMenu?.Invoke(menu, itemIndex);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			// var content = new GUIContent("Console Filter");
			// menu.AddItem(content, ConsoleFilter.enabled, () => ConsoleFilter.enabled = !ConsoleFilter.enabled);
			// menu.AddSeparator(string.Empty);

			// if (ConsoleFilterPreset.AllConfigs.Count > 0)
			// {
			// 	if (menu.GetItemCount() > 0)
			// 		menu.AddSeparator(string.Empty);
			// 	
			// 	foreach (var config in ConsoleFilterPreset.AllConfigs)
			// 	{
			// 		menu.AddItem(new GUIContent("Presets/Apply " + config.name), false, () =>
			// 		{
			// 			ConsoleFilter.enabled = true;
			// 			config.Apply();
			// 		});
			// 	}
			// }
			// 	menu.AddSeparator("Configs/");

			// if (ConsoleFilterPreset.AllConfigs.Count <= 0)
			// {
			// 	menu.AddItem(new GUIContent("New Preset"), false, () =>
			// 	{
			// 		var config = ConsoleFilterPreset.CreateAsset();
			// 		if(config) config.Apply();
			// 	});
			// }
		}
	}
}