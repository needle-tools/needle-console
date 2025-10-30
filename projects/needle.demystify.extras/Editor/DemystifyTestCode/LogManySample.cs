using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.DemystifyTestCode
{
	public static class LogManySample
	{
		[MenuItem("Test/Log Many")]
		private static void LogMany()
		{
			var assets = new[]
			{
				"Assets/Textures/Wood.png",
				"Assets/Prefabs/Player.prefab",
				"Assets/Scenes/Main.unity",
				"Assets/Scripts/PlayerController.cs",
				"Assets/Materials/Metal.mat"
			};
			var bundles = new[] { "env_hd.bundle", "characters.bundle", "ui.bundle", "audio.bundle" };
			var devices = new[] { "Xbox Controller", "Keyboard & Mouse", "DualShock 4", "Touchscreen" };
			var clips = new[] { "SFX_Click", "Music_Theme", "Footstep_Grass", "Explosion_Large" };
			var gos = new[] { "Player", "Enemy_Bot_01", "Main Camera", "NavMeshSurface", "Water" };
			var savePath = "C:/Users/User/AppData/Local/MyGame/settings.json";

			for (var i = 0; i < 50; i++)
			{
				var pickAsset = assets[Mathf.FloorToInt(Random.value * assets.Length)];
				var pickBundle = bundles[Mathf.FloorToInt(Random.value * bundles.Length)];
				var pickDevice = devices[Mathf.FloorToInt(Random.value * devices.Length)];
				var pickClip = clips[Mathf.FloorToInt(Random.value * clips.Length)];
				var pickGo = gos[Mathf.FloorToInt(Random.value * gos.Length)];
				var choice = Random.Range(0, 9);
				switch (choice)
				{
					case 0:
						Debug.Log($"Import: Reimported asset '{pickAsset}' in {Random.Range(20, 250)} ms");
						break;
					case 1:
						Debug.LogWarning($"Network latency detected: {Random.Range(120, 500)} ms to api.example.com");
						break;
					case 2:
						Debug.LogError($"Addressables: Failed to download bundle '{pickBundle}' (HTTP {Random.Range(400, 503)}). Retrying…");
						break;
					case 3:
						Debug.Log($"Input: Device connected: {pickDevice} (id={Random.Range(1, 6)})");
						break;
					case 4:
						Debug.Log($"Profiler: FrameTime={Random.Range(9f, 22f):0.00} ms | Batches={Random.Range(50, 220)} | Tris={Random.Range(100000, 2000000):N0}");
						break;
					case 5:
						Debug.Log($"Save: Saved {Random.Range(2, 12)} settings to '{savePath}' in {Random.Range(1, 12)} ms");
						break;
					case 6:
						Debug.Log($"Audio: Playing clip '{pickClip}' at volume {Random.Range(.2f, 1f):0.00}");
						break;
					case 7:
						Debug.LogWarning($"Physics: Non-convex MeshCollider with non-kinematic Rigidbody is no longer supported on '{pickGo}'");
						break;
					default:
						Debug.Log($"Analytics: Event queued: level_start level={Random.Range(1, 6)} build={Application.version}");
						break;
				}
			}
		}
	}
}
