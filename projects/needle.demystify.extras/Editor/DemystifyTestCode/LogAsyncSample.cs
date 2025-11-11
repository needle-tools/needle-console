using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class LogAsyncSample
	{
		[MenuItem("Test/RemoteConfig Fetch")]
		private static async void LogAsync()
		{
			// Entry point for a realistic, deeply-stacked async sample that logs from many sources
			var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
			Debug.Log($"RemoteConfig: Fetching configuration (request={requestId})");

			var sw = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var result = await RemoteConfigClient.FetchAndApplyAsync(requestId);
				sw.Stop();
				Debug.Log($"RemoteConfig: Fetch completed in {sw.ElapsedMilliseconds} ms (request={requestId}, etag=\"{result.ETag}\", flagsApplied={result.FlagsApplied})");
			}
			catch (Exception ex)
			{
				sw.Stop();
				Debug.LogError($"RemoteConfig: Fetch failed after {sw.ElapsedMilliseconds} ms (request={requestId})\n{ex}");
			}
		}
	}
}
