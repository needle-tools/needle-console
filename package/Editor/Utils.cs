using System.Collections.Generic;

namespace Needle.Console
{
	internal static class Utils
	{
		public static string SanitizeMenuItemText(this string txt)
		{
			return txt.Replace("/", "_");//.Replace("\n", "").Replace("\t", "");
		}
		
		public static bool TryFindIndex<T>(this List<T> list, T el, out int index)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i].Equals(el))
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}
	}
}