#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
		
		
		private static bool didLog;
		[HarmonyPrefix]
		[HarmonyPatch("ExtractStackTrace")]
		private static bool Postfix(ref string __result)
		{
			// var log = !didLog;
			// didLog = true;
			StackTrace trace = new StackTrace(1, true);
			// if (log) Debug.Log("ORIGINAL: \n\n" + trace + "\nend\n\n\n");
			var enhance = new EnhancedStackTrace(trace);
			__result = enhance.ToString();
			// if (log) Debug.Log("ENHANCED: \n\n" + __result + "\nend\n\n\n");
			UnityDemystify.Apply(ref __result);
			__result += "\n\nDemystified";
			// if(log) Debug.Log(__result);
			return false;
		}

	}
	
	
	
	[HarmonyPatch(typeof(StackFrame))]
	// ReSharper disable once UnusedType.Global 
	public class Patch_StackFrame
	{
		private static bool once;
		
		[HarmonyPostfix]
		[HarmonyPatch("ToString")]
		private static void Postfix(object __instance, ref string __result)
		{
			if (!once)
			{
				once = true;
				Debug.Log("HELLO: " + __result);
			}
		}
		
	}
	//
	// internal static string StacktraceWithHyperlinks(string stacktraceText)
}