using UnityEditor;

namespace Editor.DemystifyTestCode
{
	public static class MyConsoleLogSample
	{
		[MenuItem("Test/My Console Log")]
		private static void LogWithMyConsole()
		{
			MyConsole.Log("Player 'Alice' signed in successfully.");
		}
	}
}
