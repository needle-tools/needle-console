using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEditor;
using UnityEditor.WindowsStandalone;
using UnityEngine;

namespace Needle.Demystify
{
	[HarmonyPatch]
	public class Patch_EditorGUI
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(EditorGUI), "DoTextField")]
		private static void DoTextField(TextEditor editor, Rect position)
		{
			if (!Patch_Console.IsDrawingConsole) return;
			if (!Patch_Console.ConsoleWindow) return;
			var evt = Event.current;
			if (evt == null || (evt.type != EventType.Repaint && evt.type != EventType.MouseMove)) return;
			var mouse = evt.mousePosition;
			if (!position.Contains(mouse)) return;

			editor.MoveCursorToPosition(mouse);
			editor.SelectCurrentParagraph();
			var selection = editor.SelectedText;
			editor.SelectNone();
			var matchCollection = new Regex("(?<=\\b=\")[^\"]*").Matches(selection);
			if (matchCollection.Count == 2)
			{
				var path = matchCollection[0].Value;
				if (int.TryParse(matchCollection[1].Value, out var line) && File.Exists(path))
				{
					var txt = File.ReadAllLines(path);
					if (!window) window = PopupWindow.Init();
					var windowText = GetText(txt, line-1, 7, out var count);
					if (windowText == null) Debug.Log(line);
					window.Text = windowText;
					window.ShowPopup();
					var rect = new Rect(Vector2.zero, new Vector2(800, EditorGUIUtility.singleLineHeight * count));
					var pos = GUIUtility.GUIToScreenPoint(mouse);
					pos.y -= rect.height + EditorGUIUtility.singleLineHeight * 2;
					rect.position = pos;
					window.position = rect;

					return;
				}
			}

			if (window)
			{
				Debug.Log(evt + ", " + position);
				window.Close();
			}
		}

		private static string GetText(string[] lines, int line, int padding, out int lineCount)
		{
			lineCount = 0;
			if (lines == null || lines.Length <= 0) return null;
			padding = Mathf.Max(0, padding);
			var from = Mathf.Max(0, line - padding);
			var to = Mathf.Min(lines.Length - 1, line + padding);
			var str = string.Empty;
			for (var index = from; index < to; index++)
			{
				var l = lines[index];
				if (index == line) l = "<color=#dddd00>" + l + "</color>";
				str += l + "\n";
				lineCount += 1;
			}
			return str;
		}

		private static PopupWindow window;

		private class PopupWindow : EditorWindow
		{
			[InitializeOnLoadMethod]
			private static async void InitStart()
			{
				await Task.Delay(100);
				foreach (var prev in Resources.FindObjectsOfTypeAll<PopupWindow>())
				{
					if (prev != null)
						prev.Close();
				}
			}

			internal static PopupWindow Init()
			{
				var window = CreateInstance<PopupWindow>();
				window.ShowPopup();
				style = EditorStyles.wordWrappedLabel;
				style.richText = true;
				return window;
			}

			public string Text;

			private static GUIStyle style;
			
			private void OnGUI()
			{
				if (string.IsNullOrEmpty(Text))
				{
					Close();
					return;
				}

				EditorGUILayout.LabelField(Text, style);
			}
		}
	}
}