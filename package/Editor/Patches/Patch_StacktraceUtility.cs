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
		[HarmonyPostfix]
		[HarmonyPatch("ExtractFormattedStackTrace")]
		private static void Postfix(StackTrace stackTrace, ref string __result)
		{
			__result = new EnhancedStackTrace(stackTrace).ToString();
			UnityDemystify.Apply(ref __result);
		}

		// support for Debug.Log, Warning, Error
		[HarmonyPrefix]
		[HarmonyPatch("ExtractStackTrace")]
		private static bool Postfix(ref string __result)
		{
			const int skip = 1;
			const int skipNoise = 4;
			StackTrace trace = new StackTrace(skip + skipNoise, true);
			var enhance = new EnhancedStackTrace(trace);
			__result = enhance.ToString();
			// UnityDemystify.Apply(ref __result);
			return false;
		}

	}
}