using RimWorld;

namespace GrowingZonePriorities.PlantBuilding
{
	class PlantBuildingDeSpawnListener : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(Building_PlantGrower), "DeSpawn");

		public static void Postfix(Building_PlantGrower __instance)
		{
			PriorityTracker.plantBuildingPriorities.Remove(__instance);
		}
	}
}
