using System.Threading.Tasks;

namespace Editor.DemystifyTestCode
{
	public static class RemoteConfigClient
	{
		public static async Task<RemoteConfigResult> FetchAndApplyAsync(string requestId)
		{
			// Simulate a layered call stack: HttpClient -> RetryHandler -> JsonParser -> FeatureFlagService
			var response = await RetryHandler.ExecuteAsync(() => HttpClient.FetchAsync(requestId));

			var parsed = await JsonParser.ParseAsync(response.Body);
			var appliedFlags = await FeatureFlagService.ApplyAsync(parsed);

			return new RemoteConfigResult
			{
				ETag = response.ETag,
				FlagsApplied = appliedFlags
			};
		}
	}

	public struct RemoteConfigResult
	{
		public string ETag;
		public int FlagsApplied;
	}
}
