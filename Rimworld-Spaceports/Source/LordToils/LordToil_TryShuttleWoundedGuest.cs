using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceports.LordToils
{
    class LordToil_TryShuttleWoundedGuest : LordToil_EnterShuttleOrLeave
    {
        public LordToil_TryShuttleWoundedGuest(Thing shuttle, LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false, bool interruptCurrentJob = false) : base(shuttle)
        {
            this.shuttle = shuttle;
            this.locomotion = locomotion;
            this.canDig = canDig;
            this.interruptCurrentJob = interruptCurrentJob;
        }

        private DutyDef GetExpectedDutyDef(Pawn pawn)
        {
            if (!shuttle.Spawned || !pawn.CanReach(shuttle, PathEndMode.Touch, Danger.Deadly))
            {
                return DutyDefOf.TakeWoundedGuest;
            }
            if (KidnapAIUtility.ReachableWoundedGuest(pawn) == null)
            {
                return DutyDefOf.EnterTransporterAndDefendSelf;
            }
            return SpaceportsDefOf.Spaceports_TryShuttleWoundedGuest;
        }

        private void EnsureCorrectDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                DutyDef expectedDutyDef = GetExpectedDutyDef(pawn);
                if (pawn.mindState != null && (pawn.mindState.duty == null || pawn.mindState.duty.def != expectedDutyDef))
                {
                    pawn.mindState.duty = new PawnDuty(expectedDutyDef, shuttle);
                    pawn.mindState.duty.locomotion = locomotion;
                    pawn.mindState.duty.canDig = canDig;
                    if (interruptCurrentJob && pawn.jobs != null && pawn.jobs.curJob != null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            }
        }

        public override void UpdateAllDuties()
        {
            EnsureCorrectDuties();
        }


        public override void LordToilTick()
        {
            EnsureCorrectDuties();
        }

    }
}
