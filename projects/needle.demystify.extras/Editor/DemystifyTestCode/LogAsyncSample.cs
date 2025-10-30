using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.DemystifyTestCode
{
	public static class LogAsyncSample
	{
		[MenuItem("Test/LogAsync")]
		private static async void LogAsync()
		{
			var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
			Debug.Log($"RemoteConfig: Fetching configuration (request={requestId})…");
			var sw = System.Diagnostics.Stopwatch.StartNew();
			await Task.Delay(Random.Range(120, 350));
			sw.Stop();
			Debug.Log($"RemoteConfig: Fetch completed in {sw.ElapsedMilliseconds} ms (request={requestId}, etag=\"{Guid.NewGuid().ToString("N").Substring(0, 6)}\")");
		}
	}
}
