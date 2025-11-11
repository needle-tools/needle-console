using System.Threading.Tasks;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class HttpClient
	{
		public static async Task<HttpResponse> FetchAsync(string requestId)
		{
			Debug.Log($"HttpClient: GET /config (request={requestId})");
			await Task.Delay(UnityEngine.Random.Range(20, 60));
			var etag = System.Guid.NewGuid().ToString("N").Substring(0, 8);
			Debug.Log($"HttpClient: 200 OK (etag={etag}, request={requestId})");
			return new HttpResponse
			{
				ETag = etag,
				Body = "{\"flags\":[{\"key\":\"feature_a\",\"on\":true},{\"key\":\"feature_b\",\"on\":false}],\"meta\":{\"updated\":\"2025-11-11T10:00:00Z\"}}"
			};
		}
	}

	public struct HttpResponse
	{
		public string ETag;
		public string Body;
	}
}
