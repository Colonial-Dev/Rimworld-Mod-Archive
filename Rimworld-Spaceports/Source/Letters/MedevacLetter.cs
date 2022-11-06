using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace Spaceports.Letters
{
    public class MedevacTracker : Utils.Tracker
    {
        private Pawn hurtPawn;
        private Thing shuttle;

        public MedevacTracker()
        {

        }

        public MedevacTracker(Pawn pawn, Thing shuttle)
        {
            hurtPawn = pawn;
            this.shuttle = shuttle;
        }

        public override bool Check()
        {
            if (!hurtPawn.Downed && hurtPawn.Spawned)
            {
                List<Pawn> list = new List<Pawn>();
                list.Add(hurtPawn);
                LordJob lordJob = new LordJobs.LordJob_DepartSpaceport(shuttle);
                LordMaker.MakeNewLord(hurtPawn.Faction, lordJob, hurtPawn.Map, list);

                if (Rand.RangeInclusive(1, 100) <= 50)
                {
                    IncidentQueue stQueue = Find.Storyteller.incidentQueue;
                    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(SpaceportsDefOf.Spaceports_MedevacReward.category, hurtPawn.Map);
                    var qi = new QueuedIncident(new FiringIncident(SpaceportsDefOf.Spaceports_MedevacReward, null, incidentParms), (int)(Find.TickManager.TicksGame + GenDate.TicksPerDay * 15));
                    stQueue.Add(qi);
                }

                return true;
            }
            else if (hurtPawn.Dead)
            {
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref hurtPawn, "hurtPawn");
            Scribe_References.Look(ref shuttle, "shuttle");
        }
    }

    internal class MedevacLetter : ChoiceLetter
    {
        public Faction faction;
        public Map map;
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }
                DiaOption diaAccept = new DiaOption("Spaceports_ShuttleMedevacAccept".Translate());
                DiaOption diaDeny = new DiaOption("Spacepots_ShuttleMedevacDeny".Translate());
                diaAccept.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 0);
                    Pawn pawn = PawnGenerator.GeneratePawn(faction.RandomPawnKind(), faction);
                    Utils.StripPawn(pawn);
                    HealthUtility.DamageUntilDowned(pawn);
                    List<Pawn> list = new List<Pawn>();
                    list.Add(pawn);
                    TransportShip shuttle = Utils.GenerateInboundShuttle(list, pad, map);
                    map.GetComponent<SpaceportsMapComp>().LoadTracker(new MedevacTracker(pawn, shuttle.shipThing));
                    Buildings.Building_Shuttle b = shuttle.shipThing as Buildings.Building_Shuttle;
                    if (b != null)
                    {
                        b.disabled = true;
                    }
                    Find.LetterStack.RemoveLetter(this);
                };
                diaAccept.resolveTree = true;
                diaAccept.disabledReason = "Spaceports_ShuttleDisabled".Translate();
                if (!Utils.AnyValidSpaceportPad(map, 0))
                {
                    diaAccept.disabled = true;
                }
                diaDeny.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                };
                diaDeny.resolveTree = true;
                yield return diaAccept;
                yield return diaDeny;
                yield return base.Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_References.Look(ref map, "map");
            base.ExposeData();
        }
    }
}
