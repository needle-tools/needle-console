using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Needle.Demystify
{
	[Serializable]
	public class PackageFilter : BaseFilterWithActiveState<string>
	{
		private static PackageInfo[] allPackages;
		private Dictionary<string, int> filePackageDict = new Dictionary<string, int>();

		protected override void OnChanged()
		{
			base.OnChanged();
			filePackageDict.Clear();
		}

		public override string GetLabel(int index)
		{
			return this[index];
		}

		public override bool Exclude(string message, int mask, int row, LogEntryInfo info)
		{
			if (allPackages == null) allPackages = PackageInfo.GetAll();
			if (allPackages == null) return false;
			if (!filePackageDict.TryGetValue(info.file, out var index))
			{
				index = -1;
				for (var i = 0; i < Count; i++)
				{
					if (index >= 0) break;
					var filtered = this[i];
					Debug.Log(filtered);
					foreach (var pack in allPackages)
					{
						if (filtered == pack.name)
						{
							index = i;
							break;
						}
					}
				}
			}

			if (index == -1) return false;
			return IsActive(index);
		}


		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (allPackages == null) allPackages = PackageInfo.GetAll();

			var path = clickedLog.file;
			path = Path.GetFullPath(path);
			// path.Replace("\\", "/");

			foreach (var pack in allPackages)
			{
				var pp = pack.resolvedPath;
				if (path.StartsWith(pp))
				{
					var str = pack.name;
					var contains = TryGetIndex(str, out var index);
					var isActive = contains && IsActive(str);
					menu.AddItem(new GUIContent("Exclude Package " + str), contains && isActive, func: () =>
					{
						if (!contains)
							Add(str);
						else
							SetActive(index, true);
					});
					return;
				}
			}


			// Debug.Log(path);
			// // abs = "Packages/" +  abs.Substring(Application.dataPath.Length + 1);
			// var packageInfo = PackageInfo.FindForAssetPath(path);
			// var isPackage = Folders.IsPackagedAssetPath(path);
			//
			//
			// // Debug.Log(abs);
			// if (packageInfo == null)
			// {
			// 	Debug.Log("No package " + isPackage + "\n" + String.Join("\n", PackageInfo.GetAll().Select(p => p.resolvedPath)));
			// 	return;
			// }
			// Debug.Log(packageInfo.name);
			//
			// menu.AddItem(new GUIContent("Exclude Package " + packageInfo.displayName), false, () =>
			// {
			// 	Add(clickedLog.file);
			// });
		}
	}
}