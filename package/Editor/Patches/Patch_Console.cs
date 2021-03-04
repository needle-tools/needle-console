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
			patches.Add(new ConsoleGUIPatch());
		}


		internal static bool IsDrawingConsole { get; private set; }
		internal static Type ConsoleWindowType { get; private set; }
		internal static EditorWindow ConsoleWindow { get; private set; }

		private class ConsoleGUIPatch : EditorPatch
		{
			private static Type console;

			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				console = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				var method = console?.GetMethod("OnGUI", (BindingFlags) ~0);
				Debug.Assert(method != null, "Could not find console OnGUI method. Console?: " + console);
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
				if(ConsoleWindowType == null)
					ConsoleWindowType = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				var method = ConsoleWindowType?.GetMethod("StacktraceWithHyperlinks", (BindingFlags) ~0, null, new[] {typeof(string)}, null);
				Debug.Assert(method != null, "Could not find console stacktrace method. Console?: " + ConsoleWindowType);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static string lastText;
			private static string lastResult;

			public StacktracePatch()
			{
				void Init()
				{
					EditorApplication.update -= Init;
					ConsoleWindow = EditorWindow.GetWindow(ConsoleWindowType);
				}

				EditorApplication.update += Init;
				
				DemystifySettingsProvider.ThemeEditedOrChanged += () =>
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