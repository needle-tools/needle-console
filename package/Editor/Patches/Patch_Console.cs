using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public class Patch_Console : EditorPatchProvider
	{
		public override string DisplayName { get; }
		public override string Description => "Applies syntax highlighting to demystified stacktraces";

		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new StacktracePatch());
			patches.Add(new ConsoleDrawingEvent());
		}


		internal static bool IsDrawingConsole { get; private set; }
		private static Type _consoleWindowType;
		internal static Type ConsoleWindowType
		{
			get
			{
				if(_consoleWindowType == null) _consoleWindowType = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				return _consoleWindowType;
			}
			set => _consoleWindowType = value;
		}

		private static EditorWindow _consoleWindow;

		internal static EditorWindow ConsoleWindow
		{
			get
			{
				if (!_consoleWindow)
				{
					if(ConsoleWindowType != null)
						_consoleWindow = EditorWindow.GetWindow(ConsoleWindowType);
				}
				return _consoleWindow;
			}
			private set => _consoleWindow = value;
		}

		private static readonly Type SplitterStateType = typeof(Editor).Assembly.GetType("UnityEditor.SplitterState");
		private static readonly FieldInfo SplitterState = ConsoleWindowType.GetField("spl", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo SplitterRealSizes = SplitterStateType.GetField("realSizes", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo TextScroll = ConsoleWindowType.GetField("m_TextScroll", BindingFlags.NonPublic | BindingFlags.Instance);

		public static Rect GetStackScrollViewRect()
		{
			var rect = ConsoleWindow.position;

			var splitState = SplitterState.GetValue(ConsoleWindow);
#if UNITY_2020_2_OR_NEWER
			var splitRealSizes = (float[]) SplitterRealSizes.GetValue(splitState);
#else
			var splitRealSizes = (int[]) SplitterRealSizes.GetValue(splitState);
#endif

			var stackViewSize = splitRealSizes[1];

			rect.x = 0;
			rect.y = GetStackTextScroll().y;
			rect.height = stackViewSize;
			
			return rect;
		}
		
		private static Vector2 GetStackTextScroll()
		{
			return (Vector2)TextScroll.GetValue(ConsoleWindow);
		}
		
		private class ConsoleDrawingEvent : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = ConsoleWindowType?.GetMethod("OnGUI", (BindingFlags) ~0);
				Debug.Assert(method != null, "Could not find console OnGUI method. Console?: " + ConsoleWindowType);
				targetMethods.Add(method);
				return Task.CompletedTask; 
			}

			private static void Prefix()
			{
				IsDrawingConsole = true;
			}

			private static void Postfix()
			{
				IsDrawingConsole = false;
			}
		}

		private class StacktracePatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = ConsoleWindowType?.GetMethod("StacktraceWithHyperlinks", (BindingFlags) ~0, null, new[] {typeof(string)}, null);
				Debug.Assert(method != null, "Could not find console stacktrace method. Console?: " + ConsoleWindowType);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static string lastText;
			private static string lastResult;

			public StacktracePatch()
			{
				DemystifySettingsProvider.ThemeEditedOrChanged += Repaint;

				void Repaint()
				{
					lastText = null;
					if (ConsoleWindowType != null)
					{
						if(ConsoleWindow)
							ConsoleWindow.Repaint();
					}
				};
			}

			private static bool Prefix(ref string stacktraceText)
			{
				
				var textChanged = lastText != stacktraceText;
				if (textChanged)
				{
					lastText = stacktraceText;
					UnityDemystify.Apply(ref stacktraceText);
					lastResult = stacktraceText;
				}

				stacktraceText = lastResult;
				return true;
			}

			private static void Postfix(ref string __result)
			{
				if (DemystifySettings.instance.SyntaxHighlighting != Highlighting.None)
					Hyperlinks.ApplyHyperlinkColor(ref __result);
			}
		}
	}
}
