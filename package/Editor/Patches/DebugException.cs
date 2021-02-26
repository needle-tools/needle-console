#define UNITY_DEMYSTIFY_DEV
// #undef UNITY_DEMYSTIFY_DEV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace Demystify.DebugPatch
{
	[HarmonyPatch(typeof(Exception))]
	// ReSharper disable once UnusedType.Global
	public class DemystifyExceptions
	{
		[HarmonyPostfix]
		[HarmonyPatch("GetStackTrace")]
		private static void Postfix(object __instance, ref string __result)
		{
			if (__instance is Exception ex)
			{
				__result = ex.ToStringDemystified();
			}
		}
		
	}
}