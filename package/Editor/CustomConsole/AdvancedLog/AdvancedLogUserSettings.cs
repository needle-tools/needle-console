using System.Collections.Generic;
using UnityEditor;

namespace Needle.Demystify
{
	[FilePath("UserSettings/ConsoleLogAdvancedUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class ConsoleLogAdvancedUserSettings : ScriptableSingleton<ConsoleLogAdvancedUserSettings>
	{
		public void Save() => Save(true);

		public List<AdvancedLogEntry> selections = new List<AdvancedLogEntry>();
	}
}