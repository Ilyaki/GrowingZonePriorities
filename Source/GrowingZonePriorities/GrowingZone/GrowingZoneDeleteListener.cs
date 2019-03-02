using RimWorld;
using Verse;

namespace GrowingZonePriorities
{
	class GrowingZoneDeleteListener : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(Zone), "Deregister");

		public static void Postfix(Zone __instance)
		{
			if (__instance is Zone_Growing growingZone)
			{
				PriorityTracker.growingZonePriorities.Remove(growingZone);
			}
		}
	}
}
