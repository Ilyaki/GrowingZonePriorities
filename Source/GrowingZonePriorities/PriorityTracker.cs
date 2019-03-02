using RimWorld;
using System.Collections.Generic;

namespace GrowingZonePriorities
{
	class PriorityTracker
	{
		public static readonly Dictionary<Zone_Growing, PriorityIntHolder> growingZonePriorities = new Dictionary<Zone_Growing, PriorityIntHolder>();
		public static readonly Dictionary<Building_PlantGrower, PriorityIntHolder> plantBuildingPriorities = new Dictionary<Building_PlantGrower, PriorityIntHolder>();
	}
}
