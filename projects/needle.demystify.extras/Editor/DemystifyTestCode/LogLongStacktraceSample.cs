using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.DemystifyTestCode
{
	public static class LogLongStacktraceSample
	{
		[MenuItem("Test/Log Long Stacktrace")]
		private static void LogWithLongerStacktrace()
		{
			LogWithLongerStacktraceInternal(0);
		}

		private static void LogWithLongerStacktraceInternal(int level)
		{
			if (level > 10 || Random.value > .9f)
			{
				Debug.Log("Generated a deeper stacktrace sample");
			}
			else LogWithLongerStacktraceInternal(level + 1);
		}
	}
}
