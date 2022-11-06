using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Spaceports.Incidents
{
    public class IncidentWorker_SpicyPawnLending : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().eventsEnabled || !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().SpicyPawnLending)
            {
                return false;
            }
            if (!Utils.CheckIfSpaceport((Map)parms.target) || !AnySufficientlySkilledPawn((Map)parms.target))
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
            string skill = PotentialSkills(map).RandomElement();
            Pawn pawn = TrySelectSkilledPawn(map, skill);
            List<Thing> rewards = GenerateHighReward();

            Letters.SpicyPawnLendingLetter letter = (Letters.SpicyPawnLendingLetter)LetterMaker.MakeLetter(def.letterLabel, "Spaceports_SpicyPawnLendingLetter".Translate(faction.Name, skill.Colorize(Color.green), pawn.NameShortColored, pawn.gender.GetPronoun(), GetFluffText(skill), GenerateRewardInfo(rewards)), def.letterDef);
            letter.title = "Spaceports_SpicyPawnLending".Translate(faction.Name);
            letter.radioMode = true;
            letter.map = map;
            letter.faction = faction;
            letter.RequestedPawn = pawn;
            letter.rewards = rewards;
            letter.WasInjured = WillBeInjured(pawn, skill);

            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }

        private bool WillBeInjured(Pawn pawn, string skill)
        {
            Pawn_SkillTracker PawnSkills = pawn.skills;
            if (PawnSkills != null)
            {
                for (int i = 0; i < PawnSkills.skills.Count; i++)
                {
                    if (PawnSkills.skills[i].def.defName == skill)
                    {
                        float chance = (20 - PawnSkills.skills[i].Level - 10) * 0.025f;
                        chance = 0.50f - chance;
                        return Rand.Chance(chance);
                    }
                }
            }
            return false;
        }

        private Pawn TrySelectSkilledPawn(Map map, string skill)
        {
            List<Pawn> PlayerPawns = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            foreach (Pawn p in PlayerPawns)
            {
                Pawn_SkillTracker PawnSkills = p.skills;
                if (PawnSkills != null)
                {
                    for (int i = 0; i < PawnSkills.skills.Count; i++)
                    {
                        if (PawnSkills.skills[i].Level >= 10 && PawnSkills.skills[i].def.defName == skill)
                        {
                            return p;
                        }
                    }
                }

            }
            Log.Error("[Spaceports] Could not find a sufficiently skilled pawn!");
            return null;
        }

        private List<string> PotentialSkills(Map map)
        {
            List<string> skills = new List<string>();
            List<Pawn> PlayerPawns = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            foreach (Pawn p in PlayerPawns)
            {
                Pawn_SkillTracker PawnSkills = p.skills;
                if (PawnSkills != null)
                {
                    for (int i = 0; i < PawnSkills.skills.Count; i++)
                    {
                        if (PawnSkills.skills[i].Level >= 10)
                        {
                            skills.Add(PawnSkills.skills[i].def.defName);
                        }
                    }
                }
            }
            return skills;
        }

        private bool AnySufficientlySkilledPawn(Map map)
        {
            List<Pawn> PlayerPawns = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            foreach (Pawn p in PlayerPawns)
            {
                Pawn_SkillTracker PawnSkills = p.skills;
                if (PawnSkills != null)
                {
                    for (int i = 0; i < PawnSkills.skills.Count; i++)
                    {
                        if (PawnSkills.skills[i].Level >= 10)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string GetFluffText(string skill)
        {
            string body = "";

            switch (skill)
            {
                case "Shooting":
                    body += "Spaceports_ShootingFluff".Translate();
                    break;

                case "Melee":
                    body += "Spaceports_MeleeFluff".Translate();
                    break;

                case "Construction":
                    body += "Spaceports_ConstructionFluff".Translate();
                    break;

                case "Mining":
                    body += "Spaceports_MiningFluff".Translate();
                    break;

                case "Cooking":
                    body += "Spaceports_CookingFluff".Translate();
                    break;

                case "Plants":
                    body += "Spaceports_PlantsFluff".Translate();
                    break;

                case "Animals":
                    body += "Spaceports_AnimalsFluff".Translate();
                    break;

                case "Crafting":
                    body += "Spaceports_CraftingFluff".Translate();
                    break;

                case "Artistic":
                    body += "Spaceports_ArtisticFluff".Translate();
                    break;

                case "Medicine":
                    body += "Spaceports_MedicalFluff".Translate();
                    break;

                case "Social":
                    body += "Spaceports_SocialFluff".Translate();
                    break;

                case "Intellectual":
                    body += "Spaceports_IntellectualFluff".Translate();
                    break;

                default:
                    Log.Error("[Spaceports] Invalid skill " + skill + " provided when trying to get fluff text!");
                    break;
            }

            return body;
        }

        private List<Thing> GenerateHighReward()
        {
            List<Thing> reward = new List<Thing>();
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = Rand.RangeInclusive(1000, 1500);
            ThingSetMakerParams parameters = default(ThingSetMakerParams);
            parameters.qualityGenerator = QualityGenerator.Reward;
            parameters.totalMarketValueRange = new FloatRange(1500f, 2000f);
            List<Thing> items = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parameters);
            List<Thing> items2 = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parameters);
            reward.AddRange(items);
            reward.AddRange(items2);
            reward.Add(silver);
            return reward;
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
