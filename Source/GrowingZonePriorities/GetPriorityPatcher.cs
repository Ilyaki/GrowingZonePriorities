using RimWorld;
using System;
using Verse;

namespace GrowingZonePriorities
{
	class GetPriorityPatcher : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(WorkGiver_Grower), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) });

		public static void Postfix(Pawn pawn, TargetInfo t, ref float __result, WorkGiver_Grower __instance)
		{
			IntVec3 cell = t.Cell;
			var zone = (Zone_Growing)pawn.Map.zoneManager.AllZones.FirstOrFallback(x => x is Zone_Growing growZone && growZone.cells.Contains(cell));

			if (zone != null)
			{
				__result = zone != null && PriorityTracker.growingZonePriorities.TryGetValue(zone, out PriorityIntHolder intp) ? intp.Int : (int)Priority.Normal;
			}
			else
			{
				foreach (Building b in pawn.Map.listerBuildings.allBuildingsColonist)
				{
					if (b is Building_PlantGrower building)
					{
						CellRect.CellRectIterator cri = building.OccupiedRect().GetIterator();
						while (!cri.Done())
						{
							Priority priority = PriorityTracker.plantBuildingPriorities.TryGetValue(building, out PriorityIntHolder intp) ? (Priority)intp.Int : Priority.Normal;

							__result = (float)priority;

							cri.MoveNext();
						}
					}
				}
			}
		}
	}

	//If Prioritized == true, JobGiver_Work.TryIssueJobPackage will sort by priorities. Patch GetPriority to set the priority.
	class GetIsPrioritizedPatcher : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(WorkGiver_Scanner), "get_Prioritized");

		public static bool Prefix(ref bool __result, WorkGiver_Scanner __instance)
		{
			if (__instance is WorkGiver_Grower)
			{
				__result = true;
				return false;
			}

			else return true;
		}
	}
	
}
