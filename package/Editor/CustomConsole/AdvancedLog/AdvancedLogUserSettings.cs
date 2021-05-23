using System.Collections.Generic;
using UnityEditor;

namespace Needle.Demystify
{
	[FilePath("UserSettings/ConsoleAdvancedUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class AdvancedLogUserSettings : ScriptableSingleton<AdvancedLogUserSettings>
	{
		public void Save() => Save(true);

		public List<AdvancedLogEntry> selections = new List<AdvancedLogEntry>();
	}
}