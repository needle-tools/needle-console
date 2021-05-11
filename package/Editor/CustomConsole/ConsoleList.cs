using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Needle.Demystify
{
	internal static class ConsoleList
	{
		internal static bool DrawCustom
		{
			get => DemystifySettings.instance.CustomList;
			set => DemystifySettings.instance.CustomList = value;
		}


		private static Vector2 scroll, scrollStacktrace;
		private static readonly List<CachedConsoleInfo> currentEntries = new List<CachedConsoleInfo>();
		private static SplitterState spl = SplitterState.FromRelative(new float[] {70, 30}, new float[] {32, 32}, null); 

		private static int selectedRowIndex = -1, previouslySelectedRow = -2, rowDoubleClicked = -1;
		private static string selectedText;

		private static int collapsedFlag = 1 << 0;

		private static bool wasAtBottom, logsCountChanged;
		private static int previousLogsCount;
		private static DateTime lastClickTime;
		private static GUIStyle logStyle;

		private static bool HasFlag(int flags) => (LogEntries.consoleFlags & (int) flags) != 0;
		private static bool HasMode(int mode, ConsoleWindow.Mode modeToCheck) => (uint) ((ConsoleWindow.Mode) mode & modeToCheck) > 0U;

		private static ConsoleWindow _consoleWindow;

		internal static void RequestRepaint()
		{
			if(_consoleWindow) _consoleWindow.Repaint();
		}

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
					logsCountChanged = count != previousLogsCount;
					previousLogsCount = count;
					if (count <= 0)
					{
						selectedText = null;
						selectedRowIndex = -1;
						previouslySelectedRow = -1;
					}
					if (ConsoleFilter.ShouldUpdate(count))
					{
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
				previouslySelectedRow = selectedRowIndex;
				selectedRowIndex = -1;
			}


			SplitterGUILayout.BeginVerticalSplit(spl);

			var lineCount = ConsoleWindow.Constants.LogStyleLineCount;
			var xOffset = ConsoleWindow.Constants.LogStyleLineCount == 1 ? 2 : 14;
			var yTop = EditorGUIUtility.singleLineHeight + 3;
			var lineHeight = EditorGUIUtility.singleLineHeight * 1.1f * lineCount;
			count = currentEntries.Count;
			var scrollAreaHeight = Screen.height - spl.realSizes[1] - 44;
			var contentHeight = count * lineHeight;
			var scrollArea = new Rect(0, yTop, Screen.width - 3, scrollAreaHeight);
			var width = Screen.width - 3;
			if (contentHeight > scrollArea.height)
				width -= 13;
			var contentSize = new Rect(0, 0, width, contentHeight);

			// scroll to bottom if logs changed and it was at the bottom previously
			if (wasAtBottom && logsCountChanged)
				scroll.y = Mathf.Max(0, contentHeight - scrollAreaHeight);
			else if (contentHeight < scrollAreaHeight)
				scroll.y = scrollAreaHeight;
			scroll = GUI.BeginScrollView(scrollArea, scroll, contentSize);

			var position = new Rect(0, 0, width, lineHeight);
			var element = new ListViewElement();
			if (logStyle == null)
			{
				logStyle = new GUIStyle(ConsoleWindow.Constants.LogSmallStyle);
				logStyle.alignment = TextAnchor.UpperLeft;
			}
			var strRect = position;
			strRect.x += xOffset;
			strRect.y -= 1;
			strRect.height -= position.height * .15f;
			var tempContent = new GUIContent();
			var collapsed = HasFlag(collapsedFlag);


			void SelectRow(int index)
			{
				selectedRowIndex = index;
				selectedText = currentEntries[index].entry.message;
			}

			try
			{
				LogEntries.StartGettingEntries();
				for (var k = 0; k < currentEntries.Count; k++)
				{
					if (position.y + position.height >= scroll.y && position.y <= scroll.y + scrollAreaHeight)
					{
						if (Event.current.type == EventType.Repaint)
						{
							var row = k;
							var item = currentEntries[k];
							var entryIsSelected = k == selectedRowIndex;
							var entry = item.entry;
							element.row = item.row;
							element.position = position;


							// draw background
							void DrawBackground(Color col)
							{
								var prevCol = GUI.color;
								GUI.color = col;
								GUI.DrawTexture(position, Texture2D.whiteTexture);
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
							Rect iconRect = position;
							iconRect.y += 2;
							iconStyle.Draw(iconRect, false, false, entryIsSelected, false);

							// draw text
							var preview = item.str; // + " - " + item.entry.mode;
							strRect.x = xOffset;
							ConsolePreviewText.ModifyText(element, ref preview);
							// preview += item.entry.instanceID;
							GUI.Label(strRect, preview, logStyle);

							// draw badge
							if (collapsed)
							{
								Rect badgeRect = element.position;
								tempContent.text = LogEntries.GetEntryCount(item.row)
									.ToString(CultureInfo.InvariantCulture);
								Vector2 badgeSize = ConsoleWindow.Constants.CountBadge.CalcSize(tempContent);

								if (ConsoleWindow.Constants.CountBadge.fixedHeight > 0)
									badgeSize.y = ConsoleWindow.Constants.CountBadge.fixedHeight;
								badgeRect.xMin = badgeRect.xMax - badgeSize.x;
								badgeRect.yMin += ((badgeRect.yMax - badgeRect.yMin) - badgeSize.y) * 0.5f;
								badgeRect.x -= 5f;
								GUI.Label(badgeRect, tempContent, ConsoleWindow.Constants.CountBadge);
							}
						}
						else if (Event.current.type == EventType.MouseUp)
						{
							if (Event.current.button == 0)
							{
								if (position.Contains(Event.current.mousePosition))
								{
									var entry = currentEntries[k].entry;
									SelectRow(k);
									
									if (previouslySelectedRow == selectedRowIndex)
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
							else if (Event.current.button == 1)
							{
								if (position.Contains(Event.current.mousePosition))
								{
									var item = currentEntries[k];
									var menu = new GenericMenu();
									if (ConsoleFilter.RegisteredFilter.Count > 0)
									{
										ConsoleFilter.AddMenuItems(menu, item.entry);
									}
									AddConfigMenuItems(menu);
									menu.ShowAsContext();
									Event.current.Use();
									break;
								}
							}
						}
					}

					position.y += lineHeight;
					strRect.y += lineHeight;
				}
			}
			finally
			{
				LogEntries.EndGettingEntries();
			}
			
			
			
			if (selectedRowIndex >= 0 && Event.current.type == EventType.KeyDown)
			{
				switch (Event.current.keyCode)
				{
					case KeyCode.PageDown:
					case KeyCode.D:
					case KeyCode.RightArrow:
						if (selectedRowIndex >= 0)
						{
							var newIndex = selectedRowIndex + (int) (scrollAreaHeight / lineHeight);
							newIndex = Mathf.Clamp(newIndex, 0, currentEntries.Count);
							if (newIndex >= 0 && (newIndex) < currentEntries.Count)
							{
								scroll.y += (newIndex - selectedRowIndex) * lineHeight;
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
								scroll.y += (newIndex - selectedRowIndex) * lineHeight;
								SelectRow(newIndex);
								console.Repaint();
							}
						}
						break;
					
					case KeyCode.S:
					case KeyCode.DownArrow:
						if (selectedRowIndex >= 0 && (selectedRowIndex + 1) < currentEntries.Count)
						{
							scroll.y += lineHeight;
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
							scroll.y -= lineHeight;
							if (scroll.y < 0) scroll.y = 0;
							console.Repaint();
						}
						break;
					case KeyCode.Space:
						if (selectedRowIndex >= 0 && currentEntries.Count > 0)
						{
							var menu = new GenericMenu();
							if (ConsoleFilter.RegisteredFilter.Count > 0)
							{
								ConsoleFilter.AddMenuItems(menu, currentEntries[selectedRowIndex].entry);
							}
							AddConfigMenuItems(menu);
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

			if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
			{
				var menu = new GenericMenu();
				AddConfigMenuItems(menu);
				menu.ShowAsContext();
			}

			if (rowDoubleClicked >= 0)
			{
				LogEntries.RowGotDoubleClicked(rowDoubleClicked);
				rowDoubleClicked = -1;
				previouslySelectedRow = -1;
			}

			GUI.EndScrollView();

			if (Event.current.type == EventType.Repaint)
			{
				var diffToBottom = (contentHeight - scrollAreaHeight) - scroll.y;
				wasAtBottom = diffToBottom < 1 || contentHeight < scrollAreaHeight;
			}

			// Display active text (We want word wrapped text with a vertical scrollbar)
			GUILayout.Space(scrollAreaHeight + 2);
			scrollStacktrace = GUILayout.BeginScrollView(scrollStacktrace, ConsoleWindow.Constants.Box);

			var stackWithHyperlinks = ConsoleWindow.StacktraceWithHyperlinks(selectedText ?? string.Empty);
			var height = ConsoleWindow.Constants.MessageStyle.CalcHeight(GUIContent.Temp(stackWithHyperlinks), position.width);
			EditorGUILayout.SelectableLabel(stackWithHyperlinks, ConsoleWindow.Constants.MessageStyle, GUILayout.ExpandWidth(true),
				GUILayout.ExpandHeight(true), GUILayout.MinHeight(height + 10));

			GUILayout.EndScrollView();

			SplitterGUILayout.EndVerticalSplit();

			return false;
		}

		private static void AddConfigMenuItems(GenericMenu menu)
		{
			// var content = new GUIContent("Console Filter");
			// menu.AddItem(content, ConsoleFilter.enabled, () => ConsoleFilter.enabled = !ConsoleFilter.enabled);
			// menu.AddSeparator(string.Empty);

			// if (ConsoleFilter.RegisteredFilter.Count <= 0)
			{
				if (menu.GetItemCount() > 0)
					menu.AddSeparator(string.Empty);
				
				foreach (var config in ConsoleFilterConfig.AllConfigs)
				{
					menu.AddItem(new GUIContent("Configs/" + config.name), config.IsActive, () =>
					{
						if (config.IsActive)
						{
							config.Deactivate();
						}
						else
						{
							ConsoleFilter.enabled = true;
							config.Activate();
						}
					});
				}
			}
			// if(menu.GetItemCount() > 0 && ConsoleFilterConfig.AllConfigs.Count > 0)
			// 	menu.AddSeparator("Configs/");
			// menu.AddItem(new GUIContent("Configs/New"), false, () =>
			// {
			// 	var config = ConsoleFilterConfig.CreateAsset();
			// 	if(config) config.Activate();
			// });
		}
	}
}