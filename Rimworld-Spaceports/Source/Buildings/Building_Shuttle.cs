using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Spaceports.Buildings
{
    public class Building_Shuttle : Building
    {
        public bool disabled = false;
        public bool PartyRecalled = false;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref disabled, "disabled", false);
            Scribe_Values.Look(ref PartyRecalled, "PartyRecalled", false);
            base.ExposeData();
        }

        public override void Tick()
        {
            if(Find.TickManager.TicksGame % 500 == 0) { RareTick(); }
            base.Tick();
        }

        public void RareTick()
        {
            if (this.Map != null)
            {
                if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().autoEvacuate && GenHostility.AnyHostileActiveThreatToPlayer(this.Map, true) && !PartyRecalled)
                {
                    RecallParty();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Action()
            {
                defaultLabel = "Spaceports_ImmediateDeparture".Translate(),
                defaultDesc = "Spaceports_ImmediateDepartureTooltip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/FuckOff", true),
                disabled = disabled,
                disabledReason = "Spaceports_GizmoDisabled".Translate(),
                Order = -100,
                action = delegate ()
                {
                    ForceImmediateDeparture();
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "Spaceports_RecallParty".Translate(),
                defaultDesc = "Spaceports_RecallPartyTooltip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/ComeBack", true),
                disabled = disabled,
                disabledReason = "Spaceports_GizmoDisabled".Translate(),
                Order = -100,
                action = delegate ()
                {
                    RecallParty();
                }
            };
        }

        public void ForceImmediateDeparture()
        {
            CompShuttle shuttleComp = this.GetComp<CompShuttle>();

            List<Pawn> pawnsToAbandon = new List<Pawn>();
            foreach(Pawn p in shuttleComp.requiredPawns)
            {
                if (!shuttleComp.Transporter.innerContainer.Contains(p)) { pawnsToAbandon.Add(p); }
            }
            foreach(Pawn p in pawnsToAbandon)
            {
                shuttleComp.requiredPawns.Remove(p);
            }

            ShipJob_FlyAway leave = new ShipJob_FlyAway();
            leave.loadID = Find.UniqueIDsManager.GetNextShipJobID();
            shuttleComp.shipParent.ForceJob(leave);
        }

        public void RecallParty()
        {
            CompShuttle shuttleComp = this.GetComp<CompShuttle>();
            List<Pawn> partyPawns = shuttleComp.requiredPawns;
            if (partyPawns != null && partyPawns[0] != null)
            {
                Lord lord = partyPawns[0].GetLord();
                if (lord != null)
                {
                    List<Transition> transitions = lord.Graph.transitions.ToList();
                    foreach (Transition transition in transitions)
                    {
                        foreach (Trigger trigger in transition.triggers)
                        {
                            if (trigger is Trigger_TicksPassed)
                            {
                                transition.triggers.Add(new Trigger_TicksPassed(20));
                                TaggedString notifText = "Spaceports_VisitorsLeaving".Translate(partyPawns[0].Faction.Name);
                                Messages.Message(notifText, MessageTypeDefOf.NeutralEvent, false);
                                PartyRecalled = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
