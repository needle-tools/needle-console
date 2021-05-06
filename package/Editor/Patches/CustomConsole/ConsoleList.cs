using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleList
	{
		private static string filter;
		private static List<string> excludeFiles = new List<string>();

		private static bool custom
		{
			get => SessionState.GetBool("ConsoleListCustom", false);
			set => SessionState.SetBool("ConsoleListCustom", value);
		}

		private class TempFilterWindow : EditorWindow
		{
			[MenuItem("Console/FilterWindow")]
			private static void Open()
			{
				var window = CreateWindow<TempFilterWindow>();
				window.Show();
			}
			
			private void OnGUI()
			{
				EditorGUI.BeginChangeCheck();

				custom = EditorGUILayout.Toggle("Draw Custom", custom);
				
				filter = EditorGUILayout.TextField(filter);

				if (EditorGUI.EndChangeCheck())
				{
					InternalEditorUtility.RepaintAllViews();
				}
			}
		}
		
		
		
		private static Vector2 scroll, scrollStacktrace;
		private static readonly List<(LogEntry entry, string str, int row)> currentEntries = new List<(LogEntry, string, int)>();
		private static SplitterState spl = SplitterState.FromRelative(new float[] {70, 30}, new float[] {32, 32}, null);

		private static int selectedRow = -1, previouslySelectedRow = -2, rowDoubleClicked = -1;
		private static string selectedText;

		private static int collapsedFlag = 1 << 0;
		private static bool HasFlag(int flags) { return (LogEntries.consoleFlags & (int)flags) != 0; }

		internal static bool OnDrawList(ConsoleWindow console)
		{
			if(!custom)
				return true;
			
			// scroll = EditorGUILayout.BeginScrollView(scroll);
			var count = 0;
			var collapsed = HasFlag(collapsedFlag);

			if (Event.current.type == EventType.Repaint)
			{
				currentEntries.Clear();
				try
				{
					LogEntries.StartGettingEntries();
					count = LogEntries.GetCount();
					for (var i = 0; i < count; i++)
					{
						var mask = 0;
						var preview = default(string);
						LogEntries.GetLinesAndModeFromEntryInternal(i, ConsoleWindow.Constants.LogStyleLineCount, ref mask, ref preview);


						if (!string.IsNullOrWhiteSpace(filter) && !preview.Contains(filter))
						{
							continue;
						}
						
						var entry = new LogEntry();
						LogEntries.GetEntryInternal(i, entry); 

						if (excludeFiles.Count > 0 && excludeFiles.Contains(entry.file))
						{
							continue;
						}
						
						currentEntries.Add((entry, preview, i));
					}
				}
				finally
				{
					LogEntries.EndGettingEntries();
				}
			}
			
			SplitterGUILayout.BeginVerticalSplit(spl);

			var yTop = EditorGUIUtility.singleLineHeight + 3;
			var lineHeight = EditorGUIUtility.singleLineHeight * 1.1f;
			count = currentEntries.Count;
			var scrollHeight = Screen.height - spl.realSizes[1] - 44;
			var contentHeight = count * lineHeight;
			var scrollArea = new Rect(0, yTop, Screen.width - 3, scrollHeight);
			var width = Screen.width - 3;
			if(contentHeight > scrollArea.height)
				width -= 13;
			var contentSize = new Rect(0, 0, width, contentHeight);
			
			scroll = GUI.BeginScrollView(scrollArea, scroll, contentSize);
			var position = new Rect(0, 0, width, lineHeight);
			var element = new ListViewElement();
			var style = new GUIStyle(ConsoleWindow.Constants.LogSmallStyle);
			style.alignment = TextAnchor.MiddleLeft;
			var strRect = new Rect(position);
			strRect.height -= position.height * .15f;
			var tempContent = new GUIContent();
			
			try
			{
				if (Event.current.type == EventType.MouseDown)
				{
					rowDoubleClicked = -1;
					previouslySelectedRow = selectedRow;
					selectedRow = int.MaxValue;
				}
				
				LogEntries.StartGettingEntries();
				for (var k = 0; k < currentEntries.Count; k++)
				{
					if (Event.current.type == EventType.Repaint)
					{
						var row = k;
						var entry = currentEntries[k];
						var isSelected = entry.row == selectedRow;

						if (row % 2 == 0 || isSelected)
						{
							var prevCol = GUI.color;
							GUI.color = isSelected ? new Color(.15f, .4f, .5f) : new Color(0, 0, 0, .1f);
							GUI.DrawTexture(position, Texture2D.whiteTexture);
							GUI.color = prevCol;
						}

						element.row = row;
						element.position = position;
						var preview = entry.str;
						// preview += spl.realSizes[0] + ", " + spl.realSizes[1];
						ConsoleText.ModifyText(element, ref preview);
						GUI.Label(strRect, preview, style);
						

						if (collapsed)
						{
							Rect badgeRect = element.position;
							tempContent.text = LogEntries.GetEntryCount(entry.row)
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
								console.Repaint();
							}
						}
						else if (Event.current.button == 1)
						{
							if (position.Contains(Event.current.mousePosition))
							{
								var item = currentEntries[k];
								var menu = new GenericMenu();
								var fileName = Path.GetFileName(item.entry.file);
								menu.AddItem(new GUIContent("Exclude " + fileName), false, () =>
								{
									excludeFiles.Add(item.entry.file);
									console.Repaint();
								});
								menu.ShowAsContext();
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
			GUILayout.Space(scrollHeight+2);
			scrollStacktrace = GUILayout.BeginScrollView(scrollStacktrace, ConsoleWindow.Constants.Box);

			var stackWithHyperlinks = ConsoleWindow.StacktraceWithHyperlinks(selectedText ?? string.Empty);
			var height = ConsoleWindow.Constants.MessageStyle.CalcHeight(GUIContent.Temp(stackWithHyperlinks), position.width);
			EditorGUILayout.SelectableLabel(stackWithHyperlinks, ConsoleWindow.Constants.MessageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(height + 10));

			GUILayout.EndScrollView();
			
			SplitterGUILayout.EndVerticalSplit();

			return false;
		}
	}
}