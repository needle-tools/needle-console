using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public static class UnityDemystify
	{
		[InitializeOnLoadMethod]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Init()
		{
			var instance = DemystifySettings.instance;
			instance.CurrentTheme.EnsureEntries();
			instance.CurrentTheme.SetActive();

			if (instance.FirstInstall)
			{
				async void InstalledLog()
				{
					await Task.Delay(100);
					instance.FirstInstall = false;
					instance.Save();
					Enable();
					Debug.Log("Thanks for installing Demystify. You can find Settings under Edit/Project Settings/Needle/Demystify");
				}
				InstalledLog();
			}
			
			if (!Patches().All(PatchManager.IsPersistentEnabled) && Patches().Any(PatchManager.IsPersistentEnabled))
			{
				Debug.LogWarning("Not all Demystify patches are enabled. Go to " + DemystifySettingsProvider.SettingsPath +
				                 " to enable or disable Demystify.\n" +
				                 "Patches:\n" +
				                 string.Join("\n", Patches().Select(p => p + ": " + (PatchManager.IsPersistentEnabled(p) ? "enabled" : "disabled"))) + "\n"
				);
			}
		}

		internal static IEnumerable<string> Patches()
		{
			yield return typeof(Patch_Exception).FullName;
			yield return typeof(Patch_StacktraceUtility).FullName;
			yield return typeof(Patch_Console).FullName;
		}

		public static void Enable()
		{
			foreach (var p in Patches())
				PatchManager.EnablePatch(p);
		}

		public static void Disable()
		{
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

					Filepaths.TryMakeRelative(ref str);

					if(!str.EndsWith("\n"))
						str += "\n";
				}

				if(!string.IsNullOrWhiteSpace(str))
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