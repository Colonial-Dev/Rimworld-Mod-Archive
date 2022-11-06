﻿using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports.LordJobs
{
    internal class LordToil_DefendTraderCaravan : LordToil_DefendPoint //PURE copy paste from base Rimworld, because the original is internal for some arcane reason
    {
        public override bool AllowSatisfyLongNeeds => false;

        public override float? CustomWakeThreshold => 0.5f;

        public LordToil_DefendTraderCaravan()
        {
        }

        public LordToil_DefendTraderCaravan(IntVec3 defendPoint)
            : base(defendPoint)
        {
        }

        public override void UpdateAllDuties()
        {
            LordToilData_DefendPoint lordToilData_DefendPoint = base.Data;
            Pawn pawn = TraderCaravanUtility.FindTrader(lord);
            if (pawn == null)
            {
                return;
            }
            pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, lordToilData_DefendPoint.defendPoint, lordToilData_DefendPoint.defendRadius);
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn2 = lord.ownedPawns[i];
                switch (pawn2.GetTraderCaravanRole())
                {
                    case TraderCaravanRole.Carrier:
                        pawn2.mindState.duty = new PawnDuty(DutyDefOf.Follow, pawn, 5f);
                        pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
                        break;
                    case TraderCaravanRole.Chattel:
                        pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, pawn, 5f);
                        pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
                        break;
                    case TraderCaravanRole.Guard:
                        pawn2.mindState.duty = new PawnDuty(DutyDefOf.Defend, lordToilData_DefendPoint.defendPoint, lordToilData_DefendPoint.defendRadius);
                        break;
                }
            }
        }
    }
}

