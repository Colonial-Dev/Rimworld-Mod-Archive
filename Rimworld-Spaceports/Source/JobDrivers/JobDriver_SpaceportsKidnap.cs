using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Spaceports.JobDrivers
{

    public class JobDriver_SpaceportsKidnap : JobDriver
    {
        private const TargetIndex ItemInd = TargetIndex.A;
        private TargetIndex TransporterInd = TargetIndex.B;
        protected Thing Item => job.GetTarget(TargetIndex.A).Thing;
        protected Pawn Takee => (Pawn)Item;
        public CompTransporter Transporter => job.GetTarget(TransporterInd).Thing?.TryGetComp<CompTransporter>();
        public CompShuttle Shuttle => job.GetTarget(TransporterInd).Thing?.TryGetComp<CompShuttle>();


        public override string GetReport()
        {
            if (Takee == null || pawn.HostileTo(Takee))
            {
                return base.GetReport();
            }
            return JobUtility.GetResolvedJobReport(JobDefOf.Rescue.reportString, Takee);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => Takee == null || (!Takee.Downed && Takee.Awake()));
            this.FailOnDespawnedOrNull(TransporterInd);
            this.FailOn(() => Shuttle != null && !Shuttle.IsAllowed(pawn));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TransporterInd, PathEndMode.Touch);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                CompTransporter transporter = Transporter;
                transporter.innerContainer.TryAdd(Takee.SplitOff(1));
                transporter.innerContainer.TryAdd(pawn.SplitOff(1));
            };
            yield return toil;
        }
    }
}
