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

		private static bool HasFlag(int flags)
		{
			return (LogEntries.consoleFlags & (int) flags) != 0;
		}

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
							element.row = row;
							element.position = position;

							// draw background
							if (row % 2 == 0 || entryIsSelected)
							{
								var prevCol = GUI.color;
								GUI.color = entryIsSelected ? new Color(.15f, .4f, .5f) : new Color(0, 0, 0, .1f);
								GUI.DrawTexture(position, Texture2D.whiteTexture);
								GUI.color = prevCol;
							}

							// draw icon
							GUIStyle iconStyle = ConsoleWindow.GetStyleForErrorMode(entry.mode, true, ConsoleWindow.Constants.LogStyleLineCount == 1);
							Rect iconRect = position;
							iconRect.y += 2;
							iconStyle.Draw(iconRect, false, false, entryIsSelected, false);

							// draw text
							var preview = item.str;
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