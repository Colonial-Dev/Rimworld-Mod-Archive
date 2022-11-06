using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Spaceports.Incidents
{
    public class IncidentWorker_ShuttleMedevac : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().eventsEnabled || !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().ShuttleMedevac)
            {
                return false;
            }
            if (!Utils.CheckIfSpaceport((Map)parms.target))
            {
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.faction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false, minTechLevel: TechLevel.Industrial);
            Faction faction = parms.faction;
            Letters.MedevacLetter letter = (Letters.MedevacLetter)LetterMaker.MakeLetter(def.letterLabel, "Spaceports_ShuttleMedevacLetter".Translate(faction.NameColored, faction.LeaderTitle + " " + faction.leader.NameShortColored), def.letterDef);
            letter.title = "Spaceports_ShuttleMedevacLetterLabel".Translate();
            letter.radioMode = true;
            letter.map = map;
            letter.faction = faction;
            letter.StartTimeout(5000);
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }
    }

    public class IncidentWorker_ShuttleMedevacReward : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            ThingSetMakerParams parameters = default(ThingSetMakerParams);
            parameters.qualityGenerator = QualityGenerator.Reward;
            List<Thing> list = ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate(parameters); //100% balanced rewards generation, dont't fucking question it
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, list, 110, canInstaDropDuringInit: false, leaveSlag: true);
            SendStandardLetter("Spaceports_ShuttleMedevacRewardLabel".Translate(), "Spaceports_ShuttleMedevacReward".Translate(), LetterDefOf.PositiveEvent, parms, new TargetInfo(intVec, map));
            return true;
        }
    }
}
