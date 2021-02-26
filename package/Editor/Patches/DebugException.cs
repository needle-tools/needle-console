#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

namespace Demystify.DebugPatch
{
	[HarmonyPatch(typeof(Exception))]
	// ReSharper disable once UnusedType.Global
	public class Patch_Exception
	{
		[HarmonyPostfix]
		[HarmonyPatch("GetStackTrace")]
		private static void Postfix(object __instance, ref string __result)
		{
			if (__instance is Exception ex)
			{
				__result = ex.ToStringDemystified();
			}
			
			UnityDemystify.Apply(ref __result);
		}
		
	}
	
	[HarmonyPatch(typeof(StackTraceUtility))]
	// ReSharper disable once UnusedType.Global 
	public class Patch_StacktraceUtility
	{
		[HarmonyPostfix]
		[HarmonyPatch("ExtractFormattedStackTrace")]
		private static void Postfix(StackTrace stackTrace, ref string __result)
		{
			__result = new EnhancedStackTrace(stackTrace).ToString();
			UnityDemystify.Apply(ref __result);
		}
		
	}
}