using System.Diagnostics;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Needle.Demystify
{
	[HarmonyPatch(typeof(StackTraceUtility))]
	// ReSharper disable once UnusedType.Global 
	public class Patch_StacktraceUtility
	{
		// will append Unity side of stacktrace to exceptions
		[HarmonyPrefix]
		[HarmonyPatch("ExtractFormattedStackTrace")]
		private static bool Prefix(StackTrace stackTrace, ref string __result)
		{
			__result = new EnhancedStackTrace(stackTrace).ToString();
			Hyperlinks.FixStacktrace(ref __result);
			StacktraceMarkerUtil.AddMarker(ref __result);
			return false;
		}

		// support for Debug.Log, Warning, Error
		[HarmonyPrefix]
		[HarmonyPatch("ExtractStackTrace")]
		private static bool Prefix(ref string __result)
		{
			const int baseSkip = 1;
			// const int skipNoise = 4;//4;

			var skipCalls = 0;
			var rawStacktrace = new StackTrace();
			for (var i = 0; i < rawStacktrace.FrameCount; i++)
			{
				var frame = rawStacktrace.GetFrame(i);
				var method = frame.GetMethod();
				if (method == null) break;
				if (method.DeclaringType == typeof(Patch_StacktraceUtility) 
				    || method.DeclaringType == typeof(StackTraceUtility)
				    || method.DeclaringType == typeof(Debug)
				    || method.DeclaringType?.Name == "DebugLogHandler")
				{
					skipCalls += 1;
					continue;
				}
				break;
			}

			var trace = new StackTrace(baseSkip + skipCalls, true);
			trace = new EnhancedStackTrace(trace);
			__result = trace.ToString();
			Hyperlinks.FixStacktrace(ref __result);
			StacktraceMarkerUtil.AddMarker(ref __result);
			return false;
		}

	}
}