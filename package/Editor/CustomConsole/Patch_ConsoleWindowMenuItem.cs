using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{	
	internal class Patch_ConsoleWindowMenuItem : PatchBase
	{
		protected override IEnumerable<MethodBase> GetPatches()
		{
			var method = Patch_Console.ConsoleWindowType.GetMethod("AddItemsToMenu", BindingFlags.Public | BindingFlags.Instance);
			yield return method;
		}

		private static void Postfix(GenericMenu menu)
		{
			const string prefix = "Demystify/";
			menu.AddItem(new GUIContent(prefix + "Code Preview"), DemystifySettings.instance.AllowCodePreview,
				data => { DemystifySettings.instance.AllowCodePreview = !DemystifySettings.instance.AllowCodePreview; }, null);
			menu.AddItem(new GUIContent(prefix + "Short Hyperlinks"), DemystifySettings.instance.ShortenFilePaths,
				data => { DemystifySettings.instance.ShortenFilePaths = !DemystifySettings.instance.ShortenFilePaths; }, null);
			menu.AddItem(new GUIContent(prefix + "Show Filename"), DemystifySettings.instance.ShowFileName,
				data => { DemystifySettings.instance.ShowFileName = !DemystifySettings.instance.ShowFileName; }, null);
			menu.AddItem(new GUIContent(prefix + "Custom List"), DemystifySettings.instance.CustomList,
				data => { DemystifySettings.instance.CustomList = !DemystifySettings.instance.CustomList; }, null);
		}
	}
}