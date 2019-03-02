using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GrowingZonePriorities
{
	class GrowingZoneGizmoPatcher : Patch
	{
		protected override Desc GetDesc() => new Desc(typeof(Zone_Growing), "GetGizmos");

		public static void Postfix(ref IEnumerable<Gizmo> __result, Zone_Growing __instance)
		{
			var list = __result.ToList();

			Priority priority = PriorityTracker.growingZonePriorities.TryGetValue(__instance, out PriorityIntHolder intp) ? (Priority)intp.Int : Priority.Normal;

			list.Add(new Command_GrowingPriority((int)priority)
			{
				defaultLabel = $"Priority {priority}",
				defaultDesc = $"Set this growing zone's priority. Current priority = {priority}",
				icon = TexCommand.ForbidOff,
				onChanged = (x) => {
					var p = PriorityTracker.growingZonePriorities;
					if (p.ContainsKey(__instance))
						p[__instance].Int = x;
					else
						p[__instance] = new PriorityIntHolder(x);
				}
			});

			__result = list;
		}
	}
}
