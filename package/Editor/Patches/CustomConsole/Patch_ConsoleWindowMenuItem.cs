using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using needle.EditorPatching;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	// ReSharper disable once UnusedType.Global
	public class Patch_ConsoleWindowMenuItem : EditorPatchProvider
	{
		protected override void OnGetPatches(List<EditorPatch> patches)
		{
			patches.Add(new Patch());
		}

		private class Patch : EditorPatch
		{
			protected override Task OnGetTargetMethods(List<MethodBase> targetMethods)
			{
				var method = Patch_Console.ConsoleWindowType.GetMethod("AddItemsToMenu", BindingFlags.Public | BindingFlags.Instance);
				targetMethods.Add(method);
				return Task.CompletedTask;
			}

			// ReSharper disable once UnusedMember.Local
			private static void Postfix(GenericMenu menu)
			{
				const string prefix = "Demystify/";
				menu.AddItem(new GUIContent(prefix + "Code Preview"), DemystifySettings.instance.AllowCodePreview,
					data => { DemystifySettings.instance.AllowCodePreview = !DemystifySettings.instance.AllowCodePreview; }, null);
				menu.AddItem(new GUIContent(prefix + "Short Hyperlinks"), DemystifySettings.instance.ShortenFilePaths,
					data => { DemystifySettings.instance.ShortenFilePaths = !DemystifySettings.instance.ShortenFilePaths; }, null);
				menu.AddItem(new GUIContent(prefix + "Show Filename"), DemystifySettings.instance.ShowFileName,
					data => { DemystifySettings.instance.ShowFileName = !DemystifySettings.instance.ShowFileName; }, null);
				menu.AddItem(new GUIContent(prefix + "Auto Filter"), DemystifySettings.instance.AutoFilter,
					data => { DemystifySettings.instance.AutoFilter = !DemystifySettings.instance.AutoFilter; }, null);
			}
		}
	}
}