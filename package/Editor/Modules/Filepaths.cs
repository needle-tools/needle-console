using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Needle.Console
{
	internal static class Filepaths
	{
		// pattern: match absolute disc path for cs files
		private const string Pattern = @" \(at (?<filepath>\w{1}\:\/.*\.cs)\:\d{1,}";
		private static readonly Regex Regex = new Regex(Pattern, RegexOptions.Compiled);
		
		public static void TryMakeRelative(ref string line)
		{
			// unity sometimes fails to make paths relative to the project (e.g. when logging from local packages)
			try
			{
				var match = Regex.Match(line);
				if (!match.Success) return;
				var pathGroup = match.Groups["filepath"];
				if (!pathGroup.Success) return;
				var filePath = new Uri(pathGroup.Value, UriKind.RelativeOrAbsolute);
				if (filePath.IsAbsoluteUri)
				{
					var appPath = new Uri(Application.dataPath, UriKind.Absolute);
					// TODO: MakeRelativeUri produces quite a lot of garbage
					var relativePath = appPath.MakeRelativeUri(filePath).ToString();
					relativePath = WebUtility.UrlDecode(relativePath);
					// relativePath = relativePath.Replace("%20", " ");
					// if (makeHyperlink) relativePath = "<a href=\"" + pathGroup.Value + "\">" + relativePath + "</a>";
					line = line.Replace(pathGroup.Value, relativePath);
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