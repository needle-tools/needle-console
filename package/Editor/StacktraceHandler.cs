using System;
using UnityEngine;

namespace Demystify.DebugPatch
{
	public static class StacktraceHandler
	{
		public static void ApplyModules(ref string stacktrace)
		{
			try
			{
				var str = "";
				var lines = stacktrace.Split('\n');
				for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];
					var path = Hyperlinks.Fix(ref line);
					SyntaxHighlighting.AddSyntaxHighlighting(ref line);
					str += line;
					str += ")" + path;
					str += "\n";
				}

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