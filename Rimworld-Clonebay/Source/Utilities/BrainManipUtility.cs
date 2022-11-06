using System;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Clonebay
{
    public static class BrainManipUtility
    {
        public static bool IsValidBrainScanningDef(this ThingDef def)
        {
            return !def.race.IsMechanoid && !GeneralCompatibility.excludedRaces.Contains(def);
        }

        public static bool IsValidBrainScanningTarget(this Pawn pawn)
        {
            ThingDef def = pawn.def;
            return IsValidBrainScanningDef(def) && !pawn.Dead && !pawn.health.hediffSet.hediffs.Any(hediff => GeneralCompatibility.excludedHediffs.Any(hediffDef => hediff.def == hediffDef));
        }

        public static bool IsValidBrainTemplatingTarget(this Pawn pawn)
        {
            return IsValidBrainScanningTarget(pawn) &&
                pawn.health.hediffSet.HasHediff(QEHediffDefOf.QE_CloneStatus, false) &&
                !pawn.health.hediffSet.HasHediff(QEHediffDefOf.QE_BrainTemplated, false);
        }

        public static Thing MakeBrainScan(Pawn pawn, ThingDef genomeDef)
        {
            Thing brainScanThing = ThingMaker.MakeThing(genomeDef);
            BrainScanTemplate brainScan = brainScanThing as BrainScanTemplate;
            if (brainScan != null)
            {
                //Standard.
                brainScan.sourceName = pawn?.Name?.ToStringFull ?? null;

                //Backgrounds
                Pawn_StoryTracker story = pawn.story;
                if (story != null)
                {
                    brainScan.backStoryChild = story.childhood;
                    brainScan.backStoryAdult = story.adulthood;
                    brainScan.hairDef = story.hairDef;
                }

                //Skills
                Pawn_SkillTracker skillTracker = pawn.skills;
                if(skillTracker != null)
                {
                    foreach (SkillRecord skill in skillTracker.skills)
                    {
                        brainScan.skills.Add(new SkillRecord()
                        {
                            def = skill.def,
                            Level = skill.Level,
                            passion = skill.passion
                        });
                    }
                }

                //Relations
                //I have no clue on this one, but it NEEDS to happen or this mod won't do what it promises
                /*Pawn_RelationsTracker relationsTracker = pawn.relations;
                if (relationsTracker != null)
                {
                    foreach (DirectPawnRelation relations in relationsTracker.DirectRelations)
                    {
                        brainScan.directRelations.Add(new DirectPawnRelation()
                        {
                            def = relations.def,
                            otherPawn = relations.otherPawn,
                            startTicks = relations.startTicks,
                    });
                    }
                }*/

                //Animal
                brainScan.isAnimal = pawn.RaceProps.Animal;

                //Training
                Pawn_TrainingTracker trainingTracker = pawn.training;
                if(trainingTracker != null)
                {
                    DefMap<TrainableDef, bool> learned = (DefMap<TrainableDef, bool>)AccessTools.Field(typeof(Pawn_TrainingTracker), "learned").GetValue(trainingTracker);
                    DefMap<TrainableDef, int> steps = (DefMap<TrainableDef, int>)AccessTools.Field(typeof(Pawn_TrainingTracker), "steps").GetValue(trainingTracker);

                    //Copy
                    foreach (var item in learned)
                    {
                        brainScan.trainingLearned[item.Key] = item.Value;
                    }
                    foreach (var item in steps)
                    {
                        brainScan.trainingSteps[item.Key] = item.Value;
                    }
                }
            }

            return brainScanThing;
        }

        public static void ApplyBrainScanTemplateOnPawn(Pawn pawn, BrainScanTemplate brainScan, float efficency = 1f)
        {
            if(pawn.IsValidBrainScanningTarget())
            {
                //Backgrounds
                Pawn_StoryTracker storyTracker = pawn.story;
                if (storyTracker != null)
                {
                    //Log.Warning("Child backstory transferring...");
                    storyTracker.childhood = brainScan.backStoryChild;
                    //Log.Warning("Adult backstory transferring...");
                    storyTracker.adulthood = brainScan.backStoryAdult;
                    //Log.Warning("Hairstyle being grown...");
                    storyTracker.hairDef = brainScan.hairDef;
                }

                //Skills

                Pawn_SkillTracker skillTracker = pawn.skills;
                if (skillTracker != null)
                {
                    foreach (SkillRecord skill in brainScan.skills)
                    {
                        //Log.Warning("Iterating through skills...");
                        SkillRecord pawnSkill = skillTracker.GetSkill(skill.def);
                        pawnSkill.Level = (int)Math.Floor((float)skill.levelInt * efficency);
                        pawnSkill.passion = skill.passion;
                        pawnSkill.Notify_SkillDisablesChanged();
                    }
                }

                /*Pawn_RelationsTracker relationsTracker = pawn.relations;
                if (relationsTracker != null)
                {
                    foreach (DirectPawnRelation relations in brainScan.directRelations)
                    {
                        DirectPawnRelation relationDef = relationsTracker.GetDirectRelation(relations.def, relations.otherPawn);
                        relationDef.def = relations.def;
                        relationDef.otherPawn = relations.otherPawn;
                        relationDef.startTicks = relations.startTicks;
                    }
                }*/

                //Dirty hack ahoy! - No longer exists in 1.1
                //if (storyTracker != null)
                //{
                //    //Log.Warning("Transferring disabled worktypes... (yes, they still can't haul)");
                //    AccessTools.Field(typeof(Pawn_StoryTracker), "cachedDisabledWorkTypes").SetValue(storyTracker, null);
                //}
                
                //Training
                Pawn_TrainingTracker trainingTracker = pawn.training;
                if (trainingTracker != null)
                {
                    DefMap<TrainableDef, bool> learned = (DefMap<TrainableDef, bool>)AccessTools.Field(typeof(Pawn_TrainingTracker), "learned").GetValue(trainingTracker);
                    DefMap<TrainableDef, int> steps = (DefMap<TrainableDef, int>)AccessTools.Field(typeof(Pawn_TrainingTracker), "steps").GetValue(trainingTracker);

                    //Copy
                    foreach (var item in brainScan.trainingLearned)
                    {
                        learned[item.Key] = item.Value;
                    }
                    foreach (var item in brainScan.trainingSteps)
                    {
                        steps[item.Key] = (int)Math.Floor((float)item.Value * efficency);
                    }
                }

                //Apply Hediff
                pawn.health.AddHediff(QEHediffDefOf.QE_BrainTemplated);
            }
        }
    }
}
