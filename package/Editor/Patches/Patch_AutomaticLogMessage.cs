using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Needle.Demystify
{
	internal class Patch_LogMessage : PatchBase
	{
		protected override IEnumerable<MethodBase> GetPatches()
		{
			var method = AccessTools.Method(typeof(Logger), "GetString", new[] {typeof(object)});
			yield return method;
		}
			
		// ReSharper disable once UnusedMember.Local
		private static void Postfix(ref string __result)
		{
			if (__result != null && __result.Length <= 0)// || __result == "Null")
			{
				var stacktrace = new StackTrace();
				var frame = stacktrace.GetFrame(4);
				var methodName = frame.GetMethod().FullDescription();
				if (!string.IsNullOrEmpty(methodName))
				{
					__result = string.Empty;
					SyntaxHighlighting.AddSyntaxHighlighting(ref methodName);
					__result += methodName;
				}
				// __result += "\n" + frame.GetFileName();
			}

			// for (var i = 0; i < stacktrace.FrameCount; i++)
			// {
			// 	__result += i + ": " + stacktrace.GetFrame(i).GetMethod() + "\n";
			// }

		}
	}
}