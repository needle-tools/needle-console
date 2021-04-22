using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Needle.Demystify
{
	// ReSharper disable once UnusedType.Global
	public class Patch_AutomaticLogMessage : EditorPatchProvider 
	{ 
		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new Patch_LogMessage());
		}

		// TODO: does not work with LogFormat yet
		
		private class Patch_LogMessage : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				
				targetMethods.Add(AccessTools.Method(typeof(Logger), "GetString", new[] {typeof(object)}));
				// targetMethods.Add(AccessTools.Method(typeof(Debug), "Log", new[] {typeof(string), typeof(Object)}));
				return Task.CompletedTask; 
			}

			// ReSharper disable once UnusedMember.Local
			private static void Postfix(ref string __result)
			{
				if (__result != null && __result.Length <= 0)// || __result == "Null")
				{
					__result = string.Empty;
					var stacktrace = new StackTrace();
					var frame = stacktrace.GetFrame(4);
					var methodName = frame.GetMethod().FullDescription();
					SyntaxHighlighting.AddSyntaxHighlighting(ref methodName);
					__result += methodName;
					// __result += "\n" + frame.GetFileName();
				}

				// for (var i = 0; i < stacktrace.FrameCount; i++)
				// {
				// 	__result += i + ": " + stacktrace.GetFrame(i).GetMethod() + "\n";
				// }

			}
		}
	}
}