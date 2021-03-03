namespace Needle.Demystify
{
	/// <summary>
	/// Add a marker to stacktraces for highlighting in console ignoring everything before
	/// </summary>
	internal static class StacktraceMarkerUtil
	{
		public const string Marker = "STACKTRACE_BEGIN";
		
		public static void AddMarker(ref string stacktrace)
		{
			stacktrace = Marker  + "\n" + stacktrace;
		}
		
		public static void RemoveMarkers(ref string stacktrace)
		{
			stacktrace = stacktrace.Replace(Marker + "\n", "");
		}
		
		
		public static bool IsPrefix(string line)
		{
			return line == Marker;
		}
	}
}