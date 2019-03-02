using RimWorld;
using Verse;

namespace GrowingZonePriorities
{
	class PlantBuildingExposeDataPatcher : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(Building_PlantGrower), "ExposeData");

		public static void Postfix(Building_PlantGrower __instance)
		{
			if (!PriorityTracker.plantBuildingPriorities.ContainsKey(__instance))
				PriorityTracker.plantBuildingPriorities[__instance] = new PriorityIntHolder((int)Priority.Normal);


			Scribe_Values.Look<int>(ref PriorityTracker.plantBuildingPriorities[__instance].Int, "growingPriority", (int)Priority.Normal, false);
		}
	}
}
