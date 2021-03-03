using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

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
			const int skip = 1;
			const int skipNoise = 4;//4;
			StackTrace trace = new StackTrace(skip + skipNoise, true);
			trace = new EnhancedStackTrace(trace);
			__result = trace.ToString();
			Hyperlinks.FixStacktrace(ref __result);
			StacktraceMarkerUtil.AddMarker(ref __result);
			return false;
		}

	}
}