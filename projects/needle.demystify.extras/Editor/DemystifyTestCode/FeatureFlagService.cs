using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class FeatureFlagService
	{
		public static async Task<int> ApplyAsync(ParsedConfig config)
		{
			var flags = config.Flags ?? new List<FlagDescriptor>();
			Debug.Log($"FeatureFlagService: applying {flags.Count} flags");

			var tasks = flags.Select(ApplySingleAsync).ToArray();
			await Task.WhenAll(tasks);
			var count = flags.Count;
			Debug.Log($"FeatureFlagService: applied {count} flags");
			return count;
		}

		private static async Task ApplySingleAsync(FlagDescriptor flag)
		{
			await Task.Delay(Random.Range(5, 20));
			Debug.Log($"FeatureFlagService: {(flag.On ? "ENABLED" : "DISABLED")} {flag.Key}");
		}
	}
}
