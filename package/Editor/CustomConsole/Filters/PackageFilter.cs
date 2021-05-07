using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SearchService;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Needle.Demystify
{
	[Serializable]
	public class PackageFilter : FilterBase<string>
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
			if (Count <= 0) return false;

			if (!filePackageDict.TryGetValue(info.file, out var index))
			{
				index = -1;
				if (TryGetPackage(info.file, out var package))
				{
					for (var i = 0; i < Count; i++)
					{
						var filtered = this[i];
						if (filtered == package.name)
						{
							index = i;
							break;
						}
					}
				}

				filePackageDict.Add(info.file, index);
			}

			if (index == -1) return false;
			return IsActive(index);
		}

		private bool TryGetPackage(string path, out PackageInfo package)
		{
			if (allPackages == null)
			{
				allPackages = PackageInfo.GetAll();
				if (allPackages == null)
				{
					package = null;
					return false;
				}
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				package = null;
				return false;
			}

			path = Path.GetFullPath(path);
			foreach (var pack in allPackages)
			{
				var pp = pack.resolvedPath;
				if (path.StartsWith(pp))
				{
					package = pack;
					return true;
				}
			}

			package = null;
			return false;
		}


		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog)
		{
			if (TryGetPackage(clickedLog.file, out var pack))
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
			}
		}
	}
}