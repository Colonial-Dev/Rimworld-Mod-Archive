using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Spaceports.Incidents
{
    public class IncidentWorker_PrisonerTransfer : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().eventsEnabled || !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().PrisonerTransfer)
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
            Pawn randomPawn = PawnGenerator.GeneratePawn(faction.RandomPawnKind(), faction);
            List<Thing> LowReward = GenerateLowReward();
            List<Thing> HighReward = GenerateHighReward();
            Letters.PrisonerTransferLetter letter = (Letters.PrisonerTransferLetter)LetterMaker.MakeLetter(def.letterLabel, "Spaceports_PrisonerTransferLetter".Translate(randomPawn.NameShortColored + "", faction.NameColored, GenerateRewardInfo(LowReward), GenerateRewardInfo(HighReward)), def.letterDef);
            letter.title = "Spaceports_PrisonerTransfer".Translate();
            letter.radioMode = true;
            letter.map = map;
            letter.faction = faction;
            letter.LowSecReward = LowReward;
            letter.HighSecReward = HighReward;
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }

        private List<Thing> GenerateLowReward()
        {
            List<Thing> LowSecReward = new List<Thing>();
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = Rand.RangeInclusive(500, 1000);
            ThingSetMakerParams parameters = default(ThingSetMakerParams);
            parameters.qualityGenerator = QualityGenerator.Reward;
            parameters.totalMarketValueRange = new FloatRange(1000f, 1500f);
            List<Thing> items = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parameters);
            LowSecReward.AddRange(items);
            LowSecReward.Add(silver);
            return LowSecReward;
        }

        private List<Thing> GenerateHighReward()
        {
            List<Thing> HighSecReward = new List<Thing>();
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = Rand.RangeInclusive(1000, 1500);
            ThingSetMakerParams parameters = default(ThingSetMakerParams);
            parameters.qualityGenerator = QualityGenerator.Reward;
            parameters.totalMarketValueRange = new FloatRange(2000f, 3000f);
            List<Thing> items = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parameters);
            HighSecReward.AddRange(items);
            HighSecReward.Add(silver);
            return HighSecReward;
        }

        private string GenerateRewardInfo(List<Thing> items)
        {
            string rewardInfo = "";
            foreach (Thing t in items)
            {
                rewardInfo += t.Label + ", ";
            }
            rewardInfo = rewardInfo.TrimEnd(' ');
            rewardInfo = rewardInfo.TrimEnd(',');
            return rewardInfo;
        }

    }
}
