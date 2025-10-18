using UnityEngine;

namespace Needle.Console
{
	internal static class SeparatorLine
	{
		internal static readonly Color Color = new Color(.4f, .4f, .4f);

		internal static void Draw(float y)
		{
			Draw(y, Color);
		}
		
		internal static void Draw(float y, Color color)
		{
			using (new GUIColorScope(color))
			{
				GUI.DrawTexture(new Rect(0, y, 10000, 1), Texture2D.whiteTexture);
			}
		}

		internal static void DrawHorizontal(float x, float y)
		{
			DrawHorizontal(x, y, Color);
		}
		
		internal static void DrawHorizontal(float x, float y, Color color)
		{
			using (new GUIColorScope(color))
			{
				GUI.DrawTexture(new Rect(x, y, 1, 10000), Texture2D.whiteTexture);
			}
		}
	}
}
