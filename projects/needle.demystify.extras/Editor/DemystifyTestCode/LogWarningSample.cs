using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogWarningSample
	{
		[MenuItem("Test/LogWarning")]
		private static void Warning()
		{
			Debug.LogWarning("Importer: Deprecated shader property '_Glossiness' found in 'Assets/Materials/Car.mat' (line 27). Consider upgrading to URP/Lit.");
		}
	}
}
