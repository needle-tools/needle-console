using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using needle.EditorPatching;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public class Patch_Console : EditorPatchProvider
	{
		//  internal static string StacktraceWithHyperlinks(string stacktraceText)
		// void SetActiveEntry(LogEntry entry)

		public override string DisplayName { get; }
		public override string Description { get; }

		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new ConsolePatch());
		}

		private class ConsolePatch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var console = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityEditor.ConsoleWindow");
				// var method = console?.GetMethod("StacktraceWithHyperlinks", (BindingFlags)~0, null, new[] {typeof(string)}, null);
				var method = console?.GetMethod("SetActiveEntry", (BindingFlags) ~0);
				// if (DemystifySettings.instance.DevelopmentMode)
					Debug.Assert(method != null, "Could not find console window method. Console?: " + console);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			private static object active;
			
			// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L670
			/*
			 * if (m_ListView.selectionChanged || !m_ActiveText.Equals(entry.message))
                    {
                        SetActiveEntry(entry);
                    }
			 * 
			 */

			private static bool Prefix(object __instance, object entry)
			{
				if (entry != null && !entry.Equals(active))
				{
					active = entry;
					Debug.Log(entry);
				}
				return true;
			}

			//           private static bool Prefix(object __instance, ref string __result, string stacktraceText)
			//           {
			//            StringBuilder textWithHyperlinks = new StringBuilder();
			//            __result = str;
			// 		UnityDemystify.Apply(ref __result, true);
			// 		return false;
			// 	}
		}
	}
}