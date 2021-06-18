using System.IO;
using UnityEngine;

namespace Needle.Console
{
	public static class ConsoleUtils
	{
		internal static bool TryMakeProjectRelative(string path, out string result)
		{
			var fp = Path.GetFullPath(path);
			fp = fp.Replace("\\", "/");
			var project = Path.GetFullPath(Application.dataPath + "/../").Replace("\\", "/");
			if (!fp.StartsWith(project))
			{
				if (PackageFilter.TryGetPackage(fp, out var package))
				{
					var rel = "Packages/" + package.name + "/" + fp.Substring(package.resolvedPath.Length);
					result = rel;
					return File.Exists(result);
				}
			}
			result = fp.Substring(project.Length);
			return File.Exists(result);
		}
	}
}