using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	// ReSharper disable once UnusedType.Global 
	public class Patch_StacktraceUtility
	{
		public class ExtractFormatted : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				yield return AccessTools.Method(typeof(StackTraceUtility), "ExtractFormattedStackTrace");
			}

			// will append Unity side of stacktrace to exceptions
			// NOTE: Unity changed the parameter name in StackTraceUtility.ExtractFormattedStackTrace
			// between Unity 6000.0.51f1 and 6000.0.54f1:
			// - Unity 6000.0.51f1 and earlier: parameter named "stackTrace" 
			// - Unity 6000.0.54f1 and later: parameter named "stackFrames"
			// Using __0 (first parameter) to work regardless of the parameter name
			// See: https://github.com/needle-tools/needle-console/issues/33
			private static bool Prefix(StackTrace __0, ref string __result)
			{
				__result = new EnhancedStackTrace(__0).ToString();
				Hyperlinks.FixStacktrace(ref __result);
				StacktraceMarkerUtil.AddMarker(ref __result);
				return false;
			}
		}


		public class Extract : PatchBase
		{
			protected override IEnumerable<MethodBase> GetPatches()
			{
				yield return AccessTools.Method(typeof(StackTraceUtility), "ExtractStackTrace");
			}

			// support for Debug.Log, Warning, Error
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
					    || method.DeclaringType?.Name == "DebugLogHandler"
					    || method.Name == nameof(Prefix)
					    )
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
				__result = __result.TrimEnd('\r').TrimEnd('\n');
				return false;
			}
		}
	}
}