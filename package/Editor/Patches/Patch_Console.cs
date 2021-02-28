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
			patches.Add(new ConsolePatch());
		}

		private class ConsolePatch : EditorPatch
		{
			private static Type console;
			
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				console = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				var method = console?.GetMethod("StacktraceWithHyperlinks", (BindingFlags) ~0, null, new[] {typeof(string)}, null);
				// if (DemystifySettings.instance.DevelopmentMode)
					Debug.Assert(method != null, "Could not find console window method. Console?: " + console);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static string lastText;
			private static string lastResult;

			public ConsolePatch()
			{
				DemystifySettingsProvider.ThemeEditedOrChanged += () =>
				{
					lastText = null;
					// if (console != null)
					// {
					// 	var window = EditorWindow.GetWindow(console);
					// 	window.Repaint();
					// }
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
				Hyperlinks.ApplyHyperlinkColor(ref __result);
			}
		}
	}
}