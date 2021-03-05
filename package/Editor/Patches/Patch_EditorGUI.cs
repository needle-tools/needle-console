using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[HarmonyPatch]
	public class Patch_EditorGUI
	{
		private static CodePreview.Window window;
		private static double lastTimeFoundKey;
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(EditorGUI), "DoTextField")]
		private static void DoTextField(TextEditor editor, Rect position, ref string text)
		{
			var settings = DemystifySettings.instance;
			if (!settings.AllowCodePreview) return;
			
			if (!Patch_Console.IsDrawingConsole) return;
			if (!Patch_Console.ConsoleWindow) return;
			var evt = Event.current;

			if (evt.type == EventType.Repaint && Patch_Console.ConsoleWindow)
			{
				EditorUtility.SetDirty(Patch_Console.ConsoleWindow);
				Patch_Console.ConsoleWindow.Repaint();
			}

			if (settings.CodePreviewKeyCode != KeyCode.None)
			{
				var time = DateTime.Now.TimeOfDay.TotalSeconds;
				if (Event.current.keyCode == settings.CodePreviewKeyCode)
				{
					lastTimeFoundKey = time;
				}

				var diff = time - lastTimeFoundKey;
				if (diff > .3f)
				{
					if (window && window.Text != null)
					{
						window.Text = null;
						EditorUtility.SetDirty(window);
						window.Repaint();
					}
					return;
				}
			}
			
			if (evt == null || (evt.type != EventType.Repaint && evt.type != EventType.MouseMove)) return;
			var mouse = evt.mousePosition;
			if (!position.Contains(mouse)) return;

			var prevSelection = editor.selectIndex;
			var prevSelectionEnd = editor.cursorIndex;
			editor.MoveCursorToPosition(mouse);
			editor.SelectCurrentParagraph();
			var selection = editor.SelectedText;
			editor.selectIndex = prevSelection;
			editor.cursorIndex = prevSelectionEnd;
			
			var matchCollection = new Regex("(?<=\\b=\")[^\"]*").Matches(selection);
			if (matchCollection.Count == 2)
			{
				var path = matchCollection[0].Value;
				if (int.TryParse(matchCollection[1].Value, out var line))
				{
					var prev = CodePreview.GetPreviewText(path, line, out var lines);
					if (!string.IsNullOrEmpty(prev) && !window)
					{
						window = CodePreview.Window.Create();
					}

					if (window)
					{
						var pos = GUIUtility.GUIToScreenPoint(mouse);
						window.Text = prev;
						window.Mouse = pos;
						window.ShowPopup();
						var cornerTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0, 0));
						var consoleWindow = Patch_Console.ConsoleWindow.position;
						var rect = new Rect(Vector2.zero, new Vector2(Screen.width - 2, EditorGUIUtility.singleLineHeight * lines));
						pos.x = cornerTopLeft.x;
						// const int linesDistance = 3;
						// pos.y -= rect.height + EditorGUIUtility.singleLineHeight * linesDistance;
						pos.y = cornerTopLeft.y - rect.height - 1;
						// pos.y = consoleWindow.y;// cornerTopLeft.y - rect.height - 1;
						// rect.height = GUIUtility.GUIToScreenPoint(consoleWindow.position).y - cornerTopLeft.y;
						// var headerHeight = EditorGUIUtility.singleLineHeight * 2;
						// rect.height -= headerHeight;
						// pos.y += headerHeight;
						rect.position = pos;
						window.position = rect;
						EditorUtility.SetDirty(window);
						window.Repaint();
					}

					return;
				}
			}

			if (window)
			{
				window.Text = null;
			}
		}
	}
}