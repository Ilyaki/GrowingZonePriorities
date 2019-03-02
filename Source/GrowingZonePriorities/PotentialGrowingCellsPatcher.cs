using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;
using Vec3BoolTuple = System.Collections.Generic.KeyValuePair<Verse.IntVec3, bool>;

namespace GrowingZonePriorities
{
	class PotentialGrowingCellsPatcher : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(WorkGiver_Grower), "PotentialWorkCellsGlobal");

		private static bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn0, WorkGiver_Grower __instance)
		{
			var method = typeof(WorkGiver_Grower).GetMethod("ExtraRequirements", BindingFlags.NonPublic | BindingFlags.Instance);
			return (bool)method.Invoke(__instance, new object[] { settable, pawn0 });
		}

		private static void SetWantedPlantDef(ThingDef value)
		{
			var methodExtraRequirements = typeof(WorkGiver_Grower).GetField("wantedPlantDef", BindingFlags.NonPublic | BindingFlags.Static);
			methodExtraRequirements.SetValue(null, value);
		}

		public static bool Prefix(Pawn pawn, WorkGiver_Grower __instance, ref IEnumerable<IntVec3> __result)
		{
			//FloatMenuMakerMap and JobGiver_Work both call this method. FloatMenuMakerMap is for when the player right clicks on a field. This shouldn't be patched or sowing/harvesting can't be prioritised for a zone that doesn't have the highest priority.
			StackFrame frame = new StackFrame(2);
			var callingMethod = frame.GetMethod();
			var callingType = callingMethod.DeclaringType;
			if (frame.GetMethod().DeclaringType != typeof(JobGiver_Work))
			{
				return true;
			}
			
			__result = GetCells(pawn, __instance, out bool runOriginal);

			return runOriginal;
		}

		private static IEnumerable<IntVec3> GetCells(Pawn pawn, WorkGiver_Grower workGiver, out bool runOriginal)
		{
			runOriginal = false;

			var priorities = new Dictionary<Priority, List<Vec3BoolTuple>>();//bool: true if is a growing zone
			for (int i = (int)Priority.Critical; i >= (int)Priority.Low; i--)
			{
				priorities[(Priority)i] = new List<Vec3BoolTuple>();
			}

			/////

			Danger maxPawnDanger = pawn.NormalMaxDanger();
			List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < bList.Count; i++)
			{
				if (bList[i] is Building_PlantGrower building)
				{
					if (ExtraRequirements(building, pawn, workGiver))
					{
						if (!building.IsForbidden(pawn))
						{
							if (pawn.CanReach(building, PathEndMode.OnCell, maxPawnDanger, false, TraverseMode.ByPawn))
							{
								if (!building.IsBurning())
								{
									CellRect.CellRectIterator cri = building.OccupiedRect().GetIterator();
									while (!cri.Done())
									{
										Priority priority = PriorityTracker.plantBuildingPriorities.TryGetValue(building, out PriorityIntHolder intp) ? (Priority)intp.Int : Priority.Normal;

										priorities[priority].Add(new Vec3BoolTuple(cri.Current, false));
										cri.MoveNext();
									}
									SetWantedPlantDef(null);
								}
							}
						}
					}
				}
			}
			SetWantedPlantDef(null);



			List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
			for (int j = 0; j < zonesList.Count; j++)
			{
				if (zonesList[j] is Zone_Growing growZone)
				{
					if (growZone.cells.Count == 0)
					{
						Log.ErrorOnce("Grow zone has 0 cells: " + growZone, -563487, false);
					}
					else if (ExtraRequirements(growZone, pawn, workGiver))
					{
						if (!growZone.ContainsStaticFire)
						{
							if (pawn.CanReach(growZone.Cells[0], PathEndMode.OnCell, maxPawnDanger, false, TraverseMode.ByPawn))
							{
								Priority priority = PriorityTracker.growingZonePriorities.TryGetValue(growZone, out PriorityIntHolder intp) ? (Priority)intp.Int : Priority.Normal;

								for (int k = 0; k < growZone.cells.Count; k++)
								{
									priorities[priority].Add(new Vec3BoolTuple(growZone.cells[k], true));
								}

								SetWantedPlantDef(null);
							}
						}
					}
				}
			}

			SetWantedPlantDef(null);

			/////
			
			IntVec3 pawnPosition = pawn.Position;
			bool prioritized = workGiver.Prioritized;
			bool allowUnreachable = workGiver.AllowUnreachable;
			Danger maxDanger = workGiver.MaxPathDanger(pawn);

			foreach (Priority p in priorities.Keys)
			{
				priorities[p].SortBy(x => (float)(x.Key - pawnPosition).LengthHorizontalSquared);
			}


			IntVec3 first = IntVec3.Zero;
			try
			{
				first = priorities.SelectMany(x => x.Value).FirstOrFallback(tuple =>
				{
					IntVec3 intVec = tuple.Key;
					bool flag = false;
					float distToVec = (float)(intVec - pawnPosition).LengthHorizontalSquared;

					if (prioritized)
					{
						if (!intVec.IsForbidden(pawn) && workGiver.HasJobOnCell(pawn, intVec, false))
						{
							if (!allowUnreachable && !pawn.CanReach(intVec, workGiver.PathEndMode, maxDanger, false, TraverseMode.ByPawn))
							{
								return false;
							}
							//num5 = workGiver.GetPriority(pawn, intVec);
							//if (num5 > num3 || (num5 == num3 && distToVec < num2))
							//{
							//	flag = true;
							//}
						}
					}
					else if (!intVec.IsForbidden(pawn) && workGiver.HasJobOnCell(pawn, intVec, false))//distToVec < num2 &&
					{
						if (!allowUnreachable && !pawn.CanReach(intVec, workGiver.PathEndMode, maxDanger, false, TraverseMode.ByPawn))
						{
							return false;
						}
						flag = true;
					}

					return flag;
				}).Key;
			}catch(Exception e)
			{
				Log.Error(e.ToString());
				runOriginal = true;
				return new List<IntVec3>();
			}

			runOriginal = false;
			return new List<IntVec3>() { first };
		}
	}

	/*class PotentialGrowingCellsPatcher2 : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) });

		public static bool Prefix(Pawn pawn, TargetInfo t, WorkGiver_Scanner __instance, ref int __result)
		{
			if (__instance is WorkGiver_Grower)
			{
				IntVec3 cell = t.Cell;
				var zone = (Zone_Growing)pawn.Map.zoneManager.AllZones.FirstOrFallback(x => x is Zone_Growing growZone && growZone.cells.Contains(cell));

				__result = zone != null && GrowingZoneTracker.growingZonePriorities.TryGetValue(zone, out int intp) ? intp : (int)Priority.Normal;

				return false;
			}

			return true;
		}
	}*/
}
