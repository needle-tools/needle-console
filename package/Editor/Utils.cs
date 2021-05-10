using System.Collections.Generic;

namespace Needle.Demystify
{
	internal static class Utils
	{
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