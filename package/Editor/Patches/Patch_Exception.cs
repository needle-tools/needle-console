#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Diagnostics;
using HarmonyLib;

namespace Needle.Demystify
{
	[HarmonyPatch(typeof(Exception))]
	// ReSharper disable once UnusedType.Global
	public class Patch_Exception
	{
		// capture exception stacktrace.
		// unity is internally looping stackFrames instead of using GetStackTrace
		// but we capture all other cases in other patches
		[HarmonyPostfix]
		[HarmonyPatch("GetStackTrace")]
		private static void Postfix(object __instance, ref string __result)
		{
			if (__instance is Exception ex)
			{
				__result = ex.ToStringDemystified();
				Hyperlinks.FixStacktrace(ref __result);
				StacktraceMarkerUtil.AddMarker(ref __result);
			}
		}
		
	}
}