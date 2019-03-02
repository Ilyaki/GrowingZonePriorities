using Verse;

namespace GrowingZonePriorities
{
	[StaticConstructorOnStartup]
	class ModEntry
	{
		static ModEntry()
		{
			Patch.PatchAll("Ilyaki.GrowingZonePriorities");
			Log.Message("Growing Zone Priorities loaded");
		}
	}
}
