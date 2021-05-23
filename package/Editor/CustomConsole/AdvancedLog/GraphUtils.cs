using System.Collections.Generic;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class GraphUtils
	{
		public static void DrawGraph(Rect rect, List<float> floats, float min, float max, Color color)
		{
			if (floats == null || floats.Count <= 0) return;
			GUIUtils.SimpleColored.SetPass(0);
			GL.PushMatrix();
			GL.Begin(GL.LINE_STRIP);
			GL.Color(color);
			for (var i = 0; i < floats.Count; i++)
			{
				var val = floats[i];
				var t = (i / (float)floats.Count);
				var x = rect.x + t * rect.width;
				// GL.Vertex3(x, rect.y + rect.height, 0);
				var y = rect.y + rect.height;
				y -= Mathf.InverseLerp(min,max, val) * rect.height;
				GL.Vertex3(x, y, 0);
			}

			GL.End();
			GL.PopMatrix();
		}


		public static void DrawOutline(Rect rect, Color color)
		{
			DrawOutline(rect, color, GL.LINE_STRIP);
		}
		
		public static void DrawRect(Rect rect, Color color)
		{
			DrawOutline(rect, color, GL.QUADS);
		}

		
		private static void DrawOutline(Rect rect, Color color, int mode)
		{
			GUIUtils.SimpleColored.SetPass(0);
			GL.PushMatrix();
			GL.Begin(mode);
			GL.Color(color);
			GL.Vertex3(rect.x, rect.y, 0);
			GL.Vertex3(rect.x + rect.width, rect.y, 0);
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);
			GL.Vertex3(rect.x, rect.y + rect.height, 0);
			GL.Vertex3(rect.x, rect.y, 0);
			GL.End();
			GL.PopMatrix();
		}
	}
}