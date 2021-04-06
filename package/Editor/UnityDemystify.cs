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
			var settings = DemystifySettings.instance;

			var projectSettings = DemystifyProjectSettings.instance;
			if (projectSettings.FirstInstall)
			{
				async void InstalledLog()
				{
					await Task.Delay(100);
					Enable();
					projectSettings.FirstInstall = false;
					projectSettings.Save();
					Debug.Log("Thanks for installing Demystify. You can find Settings under Edit/Preferences Needle/Demystify");
				}

				InstalledLog();
			}

			if (!Patches().All(PatchManager.IsPersistentEnabled) && Patches().Any(PatchManager.IsPersistentEnabled))
			{
				Debug.LogWarning("Not all Demystify patches are enabled. Go to " + DemystifySettingsProvider.SettingsPath +
				                 " to enable or disable Demystify.\n" +
				                 "Patches:\n" +
				                 string.Join("\n", Patches().Select(p => p + ": " + (PatchManager.IsPersistentEnabled(p) ? "enabled" : "<b>disabled</b>"))) + "\n"
				);
			}

			if (settings.CurrentTheme != null)
			{
				settings.CurrentTheme.EnsureEntries();
				settings.CurrentTheme.SetActive();
			}
		}

		internal static IEnumerable<string> Patches()
		{
			yield return typeof(Patch_Exception).FullName;
			yield return typeof(Patch_StacktraceUtility).FullName;
			yield return typeof(Patch_Console).FullName;
			yield return typeof(Patch_EditorGUI).FullName;
		}

		public static void Enable()
		{
			foreach (var p in Patches())
				PatchManager.EnablePatch(p);
		}

		public static void Disable()
		{
			foreach (var p in Patches())
				PatchManager.DisablePatch(p, false);
		}

		public static void Apply(ref string stacktrace)
		{
			try
			{
				var str = "";
				var lines = stacktrace.Split('\n');
				var settings = DemystifySettings.instance;
				var foundPrefix = false;
				foreach (var t in lines)
				{
					var line = t;

					if (StacktraceMarkerUtil.IsPrefix(line))
					{
						StacktraceMarkerUtil.RemoveMarkers(ref line);
						if(!string.IsNullOrEmpty(settings.Separator))
							str += settings.Separator + "\n";
						foundPrefix = true;
					}

					if (foundPrefix && settings.UseSyntaxHighlighting)
						SyntaxHighlighting.AddSyntaxHighlighting(ref line);

					str += line.Trim();

					if (!str.EndsWith("\n"))
						str += "\n";
				}

				if (!string.IsNullOrWhiteSpace(str))
				{
					stacktrace = str;
				}
			}
			catch
				// (Exception e)
			{
				// ignore
			}
		}
	}
}