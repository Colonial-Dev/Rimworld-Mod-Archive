using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Spaceports.Incidents
{
    public class IncidentWorker_VisitorShuttleArrival : IncidentWorker_NeutralGroup
    {
        private const float TraderChance = 0.75f;

        private static readonly SimpleCurve PointsCurve = new SimpleCurve
    {
        new CurvePoint(45f, 0f),
        new CurvePoint(50f, 1f),
        new CurvePoint(100f, 1f),
        new CurvePoint(200f, 0.25f),
        new CurvePoint(300f, 0.1f),
        new CurvePoint(500f, 0f)
    };

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                if (!Utils.IsMapInSpace((Map)parms.target))
                {
                    return false;
                }
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularVisitors)
            {
                return false;
            }
            if (!Utils.CheckIfClearForLanding((Map)parms.target, 1))
            {
                return false;
            }
            return true;
        }

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            if (base.FactionCanBeGroupSource(f, map, desperate) && !f.Hidden && !f.HostileTo(Faction.OfPlayer) && f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDef) && f.def.techLevel.ToString() != "Neolithic")
            {
                return !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f);
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryResolveParms(parms))
            {
                return false;
            }
            List<Pawn> list = SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }
            bool traderExists = false;
            if (Rand.Value < 0.75f)
            {
                traderExists = TryConvertOnePawnToSmallTrader(list, parms.faction, map);
            }
            Pawn leader = list.Find((Pawn x) => parms.faction.leader == x);
            IntVec3 pad = Utils.FindValidSpaceportPad(map, parms.faction, 1);
            TransportShip shuttle = Utils.GenerateInboundShuttle(list, pad, map);
            LordJob lordJob = new LordJobs.LordJob_ShuttleVisitColony(parms.faction, Utils.GetBestChillspot(map, pad, 1), shuttle: shuttle.shipThing);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
            SendLetter(parms, list, leader, traderExists);
            return true;
        }

        protected virtual void SendLetter(IncidentParms parms, List<Pawn> pawns, Pawn leader, bool traderExists)
        {
            if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorNotifications)
            {
                TaggedString notificationText;
                if (pawns.Count == 1)
                {
                    TaggedString taggedString2 = ((leader != null) ? ("\n\n" + "Spaceports_SingleVisitorLeader".Translate(pawns[0].Named("PAWN")).AdjustedFor(pawns[0])) : ((TaggedString)""));
                    notificationText = "Spaceports_SingleVisitorLanding".Translate(pawns[0].story.Title, parms.faction.NameColored, pawns[0].Name.ToStringFull, taggedString2, pawns[0].Named("PAWN")).AdjustedFor(pawns[0]);
                }
                else
                {
                    TaggedString taggedString4 = ((leader != null) ? ("\n\n" + "Spaceports_GroupVisitorsLeader".Translate(leader.LabelShort, leader)) : TaggedString.Empty);
                    notificationText = "Spaceports_GroupVisitorsLanding".Translate(parms.faction.NameColored, taggedString4);
                }
                Messages.Message(notificationText, MessageTypeDefOf.NeutralEvent, false);
            }
        }

        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            if (!(parms.points >= 0f))
            {
                parms.points = Rand.ByCurve(PointsCurve);
            }
        }

        private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction, Map map)
        {
            if (faction.def.visitorTraderKinds.NullOrEmpty())
            {
                return false;
            }
            Pawn pawn = pawns.RandomElement();
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
            TraderKindDef traderKindDef = faction.def.visitorTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality);
            pawn.trader.traderKind = traderKindDef;
            pawn.inventory.DestroyAll();
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.traderDef = traderKindDef;
            parms.tile = map.Tile;
            parms.makingFaction = faction;
            foreach (Thing item in ThingSetMakerDefOf.TraderStock.root.Generate(parms))
            {
                Pawn pawn2 = item as Pawn;
                if (pawn2 != null)
                {
                    if (pawn2.Faction != pawn.Faction)
                    {
                        pawn2.SetFaction(pawn.Faction);
                    }
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5);
                    GenSpawn.Spawn(pawn2, loc, map);
                    lord.AddPawn(pawn2);
                }
                else if (!pawn.inventory.innerContainer.TryAdd(item))
                {
                    item.Destroy();
                }
            }
            PawnInventoryGenerator.GiveRandomFood(pawn);
            return true;
        }
    }
}