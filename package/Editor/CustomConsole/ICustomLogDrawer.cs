using UnityEngine;

namespace Needle.Demystify
{
	public interface ICustomLogDrawer
	{
		bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height);
	}
}