namespace needle.demystify
{
	public static class UnityDemystify
	{
		public static void Apply(ref string stacktrace)
		{
			try
			{
				var str = "";
				var lines = stacktrace.Split('\n');
				var settings = DemystifySettings.instance;
				foreach (var t in lines)
				{
					var line = t;
					// hyperlinks capture 
					var path = settings.FixHyperlinks ? Hyperlinks.Fix(ref line) : null;
					
					// additional processing
					if (settings.UseSyntaxHighlighting)
						SyntaxHighlighting.AddSyntaxHighlighting(ref line);
					str += line;
					
					// hyperlinks apply
					if (settings.FixHyperlinks && !string.IsNullOrEmpty(path))
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