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
			get => SessionState.GetBool("ConsoleListCustom", false);
			set => SessionState.SetBool("ConsoleListCustom", value);
		}


		private static Vector2 scroll, scrollStacktrace;
		private static readonly List<CachedConsoleInfo> currentEntries = new List<CachedConsoleInfo>();
		private static SplitterState spl = SplitterState.FromRelative(new float[] {70, 30}, new float[] {32, 32}, null);

		private static int selectedRow = -1, previouslySelectedRow = -2, rowDoubleClicked = -1;
		private static string selectedText;

		private static int collapsedFlag = 1 << 0;

		private static bool wasAtBottom, logsCountChanged;
		private static int previousLogsCount;

		private static bool HasFlag(int flags) => (LogEntries.consoleFlags & (int) flags) != 0;
		private static bool HasMode(int mode, ConsoleWindow.Mode modeToCheck) => (uint) ((ConsoleWindow.Mode) mode & modeToCheck) > 0U;

		internal static bool OnDrawList(ConsoleWindow console)
		{
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
				previouslySelectedRow = selectedRow;
				selectedRow = int.MaxValue;
			}
			

			SplitterGUILayout.BeginVerticalSplit(spl);

			var lineCount = ConsoleWindow.Constants.LogStyleLineCount;
			var xOffset = ConsoleWindow.Constants.LogStyleLineCount == 1 ? 2 : 14;
			var yTop = EditorGUIUtility.singleLineHeight + 3;
			var lineHeight = EditorGUIUtility.singleLineHeight * 1f * lineCount;
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
			var style = new GUIStyle(ConsoleWindow.Constants.LogSmallStyle);
			style.alignment = TextAnchor.MiddleLeft;
			var strRect = position;
			strRect.x += xOffset;
			strRect.height -= position.height * .15f;
			var tempContent = new GUIContent();
			var collapsed = HasFlag(collapsedFlag);
			

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
							var entryIsSelected = item.row == selectedRow;
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
							if (entryIsSelected)
							{
								DrawBackground(new Color(.15f, .4f, .5f));
							}
							else if (HasMode(entry.mode, ConsoleWindow.Mode.ScriptCompileError))
							{
								DrawBackground(IsOdd() ? new Color(1,0,0,.2f) : new Color(1,.2f,.25f,.25f));
							}
							else if (HasMode(entry.mode, ConsoleWindow.Mode.ScriptingError | ConsoleWindow.Mode.Error | ConsoleWindow.Mode.StickyError | ConsoleWindow.Mode.AssetImportError))
							{
								DrawBackground(IsOdd() ? new Color(1,0,0,.1f) : new Color(1,.2f,.25f,.15f));
							}
							else if (HasMode(entry.mode, ConsoleWindow.Mode.ScriptingWarning | ConsoleWindow.Mode.AssetImportWarning | ConsoleWindow.Mode.ScriptCompileWarning))
							{
								DrawBackground(IsOdd() ? new Color(.5f,.5f,0, .1f) : new Color(1, 1f, .1f, .07f));
							}
							else if(IsOdd())
							{
								DrawBackground(new Color(0, 0, 0, .1f));
							}
							
							// draw icon
							GUIStyle iconStyle = ConsoleWindow.GetStyleForErrorMode(entry.mode, true, ConsoleWindow.Constants.LogStyleLineCount == 1);
							Rect iconRect = position;
							iconRect.y += 2;
							iconStyle.Draw(iconRect, false, false, entryIsSelected, false);

							// draw text
							var preview = item.str + " - " + item.entry.mode;
							strRect.x = xOffset;
							ConsoleText.ModifyText(element, ref preview);
							GUI.Label(strRect, preview, style);

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
									selectedRow = currentEntries[k].row;
									selectedText = currentEntries[k].entry.message;
									if (previouslySelectedRow == selectedRow)
										rowDoubleClicked = selectedRow;
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
									ConsoleFilter.AddMenuItems(menu, item.entry);
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
	}
}