using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.DemystifyTestCode
{
	public static class LogRandomSample
	{
		[MenuItem("Test/Log Random")]
		private static void LogRandom()
		{
			for (var i = 0; i < 5; i++)
			{
				var c = Random.Range(0, 3);
				switch (c)
				{
					case 0: Debug.Log("Random info message"); break;
					case 1: Debug.LogWarning("Random warning message"); break;
					default: Debug.LogError("Random error message"); break;
				}
			}
		}
	}
}
