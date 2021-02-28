using System.Text.RegularExpressions;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class Hyperlinks
	{
		private static readonly Regex hyperlinks = new Regex(@"((?<brackets>\))?(?<prefix> in) (?<file>.*?):line (?<line>\d+)(?<post>.*))",
			RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// parse demystify path format and reformat to unity hyperlink format
		/// </summary>
		/// <param name="line"></param>
		/// <returns>hyperlink that must be appended to line once further processing is done</returns>
		public static string Fix(ref string line)
		{
			var match = hyperlinks.Match(line);
			if (match.Success)
			{
				var file = match.Groups["file"];
				var lineNr = match.Groups["line"];
				var brackets = match.Groups["brackets"].ToString();
				if (string.IsNullOrEmpty(brackets)) brackets = ")";
				var post = match.Groups["post"];

				var end = line.Substring(match.Index, line.Length - match.Index);
				line = line.Remove(match.Index, end.Length) + brackets;
				var newString = " (at " + file + ":" + lineNr + ")\n";
				if (post.Success)
					newString += post;
				return newString;
			}

			return string.Empty;
		}


		public static void ApplyHyperlinkColor(ref string stacktrace)
		{
			var str = stacktrace;
			const string pattern = @"(?<pre><a href=.*?>)(?<path>.*)(?<post><\/a>)";
			str = Regex.Replace(str, pattern, m =>
				{
					if (m.Success && SyntaxHighlighting.CurrentTheme.TryGetValue("link", out var col))
					{
						var pre = m.Groups["pre"].Value;
						var post = m.Groups["post"].Value;
						var path = m.Groups["path"].Value;
						var res = $"{pre}<color={col}>{path}</color>{post}";
						return res;
					}
					return m.Value;
				},
				RegexOptions.Compiled);
			stacktrace = str;
		}
	}
}