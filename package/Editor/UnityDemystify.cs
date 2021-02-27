using System;
using System.Collections.Generic;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public static class UnityDemystify
	{
		[InitializeOnLoadMethod]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			var settings = DemystifySettings.instance;
			if (!settings.IsEnabled)
			{
				Disable();
				return;
			}
			Enable();
		}

		internal static IEnumerable<string> Patches()
		{
			yield return typeof(Patch_Exception).FullName;
			yield return typeof(Patch_StacktraceUtility).FullName;
		}
		
		public static void Enable()
		{
			var settings = DemystifySettings.instance;
			settings.IsEnabled = true;
			foreach (var p in Patches())
				PatchManager.EnablePatch(p);
		}

		public static void Disable()
		{
			var settings = DemystifySettings.instance;
			settings.IsEnabled = false;
			foreach (var p in Patches())
				PatchManager.DisablePatch(p);
		}

		public static void Apply(ref string stacktrace)
		{
			try
			{
				var str = "";
				var lines = stacktrace.Split('\n');
				var settings = DemystifySettings.instance;
				var fixHyperlinks = settings.FixHyperlinks;
				// always fix hyperlinks in non development mode
				fixHyperlinks |= !settings.DevelopmentMode;
				foreach (var t in lines)
				{
					var line = t;
					// hyperlinks capture 
					var path = fixHyperlinks ? Hyperlinks.Fix(ref line) : null;

					// additional processing
					if (settings.UseSyntaxHighlighting)
						SyntaxHighlighting.AddSyntaxHighlighting(ref line);
					str += line;

					// hyperlinks apply
					if (fixHyperlinks && !string.IsNullOrEmpty(path))
						str += ")" + path;

					str += "\n";
				}

				stacktrace = str;
			}
			catch
				// (Exception e)
			{
				// IGNORE
				// Debug.LogWarning(e.Message);
			}
		}
	}
}