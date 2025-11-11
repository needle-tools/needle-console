using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class JsonParser
	{
		public static async Task<ParsedConfig> ParseAsync(string json)
		{
			Debug.Log("JsonParser: parsing response body");
			await Task.Delay(UnityEngine.Random.Range(10, 30));
			// We intentionally do not use a JSON lib to keep the sample self-contained
			return new ParsedConfig
			{
				Flags = new List<FlagDescriptor>
				{
					new FlagDescriptor("feature_a", true),
					new FlagDescriptor("feature_b", false),
				}
			};
		}
	}

	public struct ParsedConfig
	{
		public List<FlagDescriptor> Flags;
	}

	public struct FlagDescriptor
	{
		public string Key;
		public bool On;
		public FlagDescriptor(string key, bool on)
		{
			Key = key;
			On = on;
		}
	}
}
