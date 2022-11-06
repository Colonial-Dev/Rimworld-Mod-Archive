using RimWorld;
using Spaceports.LordToils;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports.LordJobs
{
    public class LordJob_DepartSpaceport : LordJob_ExitOnShuttle
    {
        private bool addFleeToil = true;
        public override bool AddFleeToil => addFleeToil;
        public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
        {
            return true;
        }

        public LordJob_DepartSpaceport()
        {
            
        }

        public LordJob_DepartSpaceport(Thing shuttle, bool addFleeToil = true)
        {
            this.shuttle = shuttle;
            this.addFleeToil = addFleeToil;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Wait lordToil_Wait = new LordToil_Wait();
            stateGraph.AddToil(lordToil_Wait);
            stateGraph.StartingToil = lordToil_Wait;
            LordToil_EnterShuttleOrLeaveNullChecked lordToil_EnterShuttleOrLeave = new LordToil_EnterShuttleOrLeaveNullChecked(shuttle, LocomotionUrgency.Sprint, interruptCurrentJob: true);
            stateGraph.AddToil(lordToil_EnterShuttleOrLeave);
            Transition transition = new Transition(lordToil_Wait, lordToil_EnterShuttleOrLeave);
            transition.AddPreAction(new TransitionAction_Custom(InitializeLoading));
            transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && shuttle.Spawned));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }

        private void InitializeLoading()
        {
            if (!shuttle.TryGetComp<CompTransporter>().LoadingInProgressOrReadyToLaunch)
            {
                TransporterUtility.InitiateLoading(Gen.YieldSingle(shuttle.TryGetComp<CompTransporter>()));
            }
        }

    }

}