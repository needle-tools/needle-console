using System;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.DemystifyTestCode
{
	public static class LogInfoSample
	{
		[MenuItem("Test/Log")]
		private static void Log()
		{
			var sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
			Debug.Log($"Init: {Application.productName} started. Unity={Application.unityVersion} Platform={Application.platform} Session={sessionId}");
		}
	}
}
