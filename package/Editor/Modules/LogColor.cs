
using System;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class LogColor
	{
		public static void AddColor(string key, ref string str)
		{
			var col = GetColor(key, out var t);
			// str += " " + t;
			var hex = ColorUtility.ToHtmlStringRGB(col);
			str = "<color=#" + hex + ">" + str + "</color>";
		}
		
		public static Color GetColor(string str, out float t)
		{
			t = CalculateHash(str) % .7f + .05f;
			var distToGreen = t - .4f;
			t += distToGreen;// * 2f;// .3f;
			// t %= 1.1f;
			var col = Color.HSVToRGB(t, .8f, 2f);
			return col;
		}

		private static float CalculateHash(string read)
		{
			var hashedValue = 0f;
			foreach (var t in read)
			{
				hashedValue += 333 * t * t / (float)char.MaxValue;
			}
			return (hashedValue * read.Length);
		}
	}
}