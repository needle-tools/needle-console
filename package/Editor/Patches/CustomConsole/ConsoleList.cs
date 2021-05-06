using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class ConsoleList
	{

		private static string filter;

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
				filter = EditorGUILayout.TextField(filter);

				if (EditorGUI.EndChangeCheck())
				{
					InternalEditorUtility.RepaintAllViews();
				}
			}
		}
		
		
		
		private static Vector2 scroll;
		private static readonly List<(LogEntry, string str)> currentEntries = new List<(LogEntry, string)>();

		internal static void OnDrawList()
		{
			// scroll = EditorGUILayout.BeginScrollView(scroll);
			var count = 0;

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

						var entry = new LogEntry();
						LogEntries.GetEntryInternal(i, entry);

						if (!string.IsNullOrWhiteSpace(filter) && !preview.Contains(filter))
						{
							// preview += " filtered";
							continue;
						}
						
						
						var c = currentEntries.Count;
						currentEntries.Add((entry, preview + ", " + c));
					}
				}
				finally
				{
					LogEntries.EndGettingEntries();
				}
			}

			var yTop = EditorGUIUtility.singleLineHeight + 3;
			var lineHeight = EditorGUIUtility.singleLineHeight * 1.2f;
			count = currentEntries.Count;
			var contentHeight = count * lineHeight;
			var scrollArea = new Rect(0, yTop, Screen.width - 3, Screen.height * .5f);
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
			strRect.height -= position.height * .18f;
			try
			{
				LogEntries.StartGettingEntries();
				for (var k = 0; k < currentEntries.Count; k++)
				{
					if (Event.current.type == EventType.Repaint)
					{
						var row = k;

						if (row % 2 == 0)
						{
							var prevCol = GUI.color;
							GUI.color = new Color(0, 0, 0, .1f);
							GUI.DrawTexture(position, Texture2D.whiteTexture);
							GUI.color = prevCol;
						}

						element.row = row;
						element.position = position;
						var entry = currentEntries[k];
						var preview = entry.str;
						ConsoleText.ModifyText(element, ref preview);
						GUI.Label(strRect, preview, style);
						position.y += lineHeight;
						strRect.y += lineHeight;
					}
				}
			}
			finally
			{
				LogEntries.EndGettingEntries();
			}

			// EditorGUILayout.EndScrollView();
			GUI.EndScrollView();
		}
	}
}