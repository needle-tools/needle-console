using System;
using UnityEditor;

namespace Needle.Console
{
	[Serializable]
	public class AdvancedLogEntry : IEquatable<CachedConsoleInfo>, IEquatable<LogEntry>
	{
		public bool Active = true;
		public string File;
		public int Line;

		public bool Equals(CachedConsoleInfo other)
		{
			return Equals(other.entry);
		}

		bool IEquatable<LogEntry>.Equals(LogEntry other)
		{
			return File == other?.file && Line == other?.line;
		}
	}
}