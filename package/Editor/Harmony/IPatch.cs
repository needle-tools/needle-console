using HarmonyLib;

namespace Needle.Demystify
{
	public interface IPatch
	{
		void Apply(Harmony harmony);
		void Remove(Harmony harmony);
	}
}