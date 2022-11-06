using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;


namespace Spaceports.Incidents
{
    public class IncidentWorker_TraderShuttleArrival : IncidentWorker_TraderCaravanArrival
    {
        protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Trader;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                if (!Utils.IsMapInSpace((Map)parms.target))
                {
                    return false;
                }
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularTraders)
            {
                return false;
            }
            if (!Utils.CheckIfClearForLanding((Map)parms.target, 2))
            {
                return false;
            }
            return true;
        }

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            if (!base.FactionCanBeGroupSource(f, map, desperate) || f.def.caravanTraderKinds.Count == 0 || f.def.techLevel.ToString() == "Neolithic")
            {
                return false;
            }
            return f.def.caravanTraderKinds.Any((TraderKindDef t) => TraderKindCommonality(t, map, f) > 0f);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryResolveParms(parms))
            {
                return false;
            }
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            List<Pawn> pawns = SpawnPawns(parms);
            if (pawns.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].needs != null && pawns[i].needs.food != null)
                {
                    pawns[i].needs.food.CurLevel = pawns[i].needs.food.MaxLevel;
                }
            }
            TraderKindDef traderKind = null;
            for (int j = 0; j < pawns.Count; j++)
            {
                Pawn pawn = pawns[j];
                if (pawn.TraderKind != null)
                {
                    traderKind = pawn.TraderKind;
                    break;
                }
            }
            SendLetter(parms, pawns, traderKind);
            IntVec3 pad = Utils.FindValidSpaceportPad(map, parms.faction, 2);
            TransportShip shuttle = Utils.GenerateInboundShuttle(pawns, pad, map);
            LordJobs.LordJob_ShuttleTradeWithColony lordJob = new LordJobs.LordJob_ShuttleTradeWithColony(parms.faction, Utils.GetBestChillspot(map, pad, 2), shuttle.shipThing);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, pawns);
            return true;
        }

        protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
        {
            TaggedString letterText = "LetterTraderShuttleArrival".Translate(parms.faction.NameColored, traderKind.label).CapitalizeFirst();
            if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderNotifications)
            {
                Messages.Message(letterText, MessageTypeDefOf.NeutralEvent, false);
            }
        }

    }
}
