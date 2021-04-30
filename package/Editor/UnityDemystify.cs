using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using needle.EditorPatching;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	public static class UnityDemystify
	{
		[InitializeOnLoadMethod]
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static async void Init()
		{
			var settings = DemystifySettings.instance;

			var projectSettings = DemystifyProjectSettings.instance;
			if (projectSettings.FirstInstall)
			{
				async void InstalledLog()
				{
					await Task.Delay(100);
					Enable(true);
					projectSettings.FirstInstall = false;
					projectSettings.Save();
					Debug.Log("Thanks for installing Demystify. You can find Settings under Edit/Preferences Needle/Demystify");
				}

				InstalledLog();
			}

			if (settings.CurrentTheme != null)
			{
				settings.CurrentTheme.EnsureEntries();
				settings.CurrentTheme.SetActive();
			}

			while (!PatchManager.IsInitialized) await Task.Delay(1);
			Enable(false);

			if (!Patches().All(PatchManager.IsPersistentEnabled) && Patches().Any(PatchManager.IsPersistentEnabled))
			{
				Debug.LogWarning("Not all Demystify patches are enabled. Go to " + DemystifySettingsProvider.SettingsPath +
				                 " to enable or disable Demystify.\n" +
				                 "Patches:\n" +
				                 string.Join("\n", Patches().Select(p => p + ": " + (PatchManager.IsPersistentEnabled(p) ? "enabled" : "<b>disabled</b>"))) + "\n"
				);
			}
		}

		internal static IEnumerable<string> Patches()
		{
			yield return typeof(Patch_Exception).FullName;
			yield return typeof(Patch_StacktraceUtility).FullName;
			yield return typeof(Patch_Console).FullName;
			yield return typeof(Patch_EditorGUI).FullName;
			yield return typeof(Patch_AutomaticLogMessage).FullName;
			var settings = DemystifySettings.instance;
			if (settings.ShowFileName)
			{
				yield return "Needle.Demystify.Patch_ConsoleWindowListView";
				yield return "Needle.Demystify.Patch_ConsoleWindowMenuItem";
			}
		}

		public static void Enable(bool force = false)
		{
			foreach (var p in Patches())
			{
				if(force || !PatchManager.HasPersistentSetting(p))
					PatchManager.EnablePatch(p);
			}
		}

		public static void Disable()
		{
			foreach (var p in Patches())
				PatchManager.DisablePatch(p, false);
		}

		private static readonly StringBuilder builder = new StringBuilder();
		public static void Apply(ref string stacktrace)
		{
			try
			{
				using (new ProfilerMarker("Demystify.Apply").Auto())
				{
					string[] lines = null;
					using(new ProfilerMarker("Split Lines").Auto())
						lines = stacktrace.Split('\n');
					var settings = DemystifySettings.instance;
					var foundPrefix = false;
					foreach (var t in lines)
					{
						var line = t;

						using (new ProfilerMarker("Remove Markers").Auto())
						{
							if (StacktraceMarkerUtil.IsPrefix(line))
							{
								StacktraceMarkerUtil.RemoveMarkers(ref line);
								if(!string.IsNullOrEmpty(settings.Separator))
									builder.AppendLine(settings.Separator);
								foundPrefix = true;
							}
						}

						if (foundPrefix && settings.UseSyntaxHighlighting)
							SyntaxHighlighting.AddSyntaxHighlighting(ref line);

						var l = line.Trim();
						if (!string.IsNullOrEmpty(l))
						{
							if (!l.EndsWith("\n"))
								builder.AppendLine(l);
							else 
								builder.Append(l);
						}
					}

					var res = builder.ToString();
					if (!string.IsNullOrWhiteSpace(res))
					{
						stacktrace = res;
					}
					builder.Clear();
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