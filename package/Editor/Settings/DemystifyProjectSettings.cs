using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[FilePath("ProjectSettings/DemystifySettings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class DemystifyProjectSettings : ScriptableSingleton<DemystifyProjectSettings>
	{
		internal void Save()
		{
			Undo.RegisterCompleteObjectUndo(this, "Save Demystify Project Settings");
			base.Save(true);
		}
		
		[SerializeField]
		internal bool FirstInstall = true;
	}
}