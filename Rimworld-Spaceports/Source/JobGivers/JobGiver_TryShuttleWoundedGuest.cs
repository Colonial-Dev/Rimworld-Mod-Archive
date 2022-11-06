using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Spaceports.JobGivers
{
    public class JobGiver_TryShuttleWoundedGuest : JobGiver_EnterTransporter
    {
        private static List<CompTransporter> tmpTransporters = new List<CompTransporter>();
        protected override Job TryGiveJob(Pawn pawn)
        {
            /*if (!RCellFinder.TryFindBestExitSpot(pawn, out var spot))
			{
				return null;
			}*/

            Pawn pawn2 = KidnapAIUtility.ReachableWoundedGuest(pawn);
            if (pawn2 == null)
            {
                return null;
            }

            int transportersGroup = pawn.mindState.duty.transportersGroup;
            if (transportersGroup != -1)
            {
                List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    if (allPawnsSpawned[i] != pawn && allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
                    {
                        CompTransporter transporter = ((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
                        if (transporter != null && transporter.groupID == transportersGroup)
                        {
                            return null;
                        }
                    }
                }
                TransporterUtility.GetTransportersInGroup(transportersGroup, pawn.Map, tmpTransporters);
                CompTransporter compTransporter = FindMyTransporter(tmpTransporters, pawn);
                tmpTransporters.Clear();
                if (compTransporter == null || !pawn.CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly))
                {
                    return null;
                }
                return JobMaker.MakeJob(JobDefOf.EnterTransporter, compTransporter.parent);
            }
            Thing thing = pawn.mindState.duty.focus.Thing;
            if (thing == null || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(SpaceportsDefOf.Spaceports_Kidnap, pawn2, pawn.mindState.duty.focus.Thing);
            job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Sprint);
            job.count = 1;
            return job;
        }
    }
}
