using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter
{
    public static class MealPrinter_Patches
    {

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
        public static class Harmony_FoodUtility_BestFoodSourceOnMap
        {
            static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull,
                    ref bool allowForbidden, ref bool allowSociallyImproper)
            {
                MealPrinterMod.BestFoodSourceOnMap = true;
                MealPrinterMod.getter = getter;
                MealPrinterMod.eater = eater;
                MealPrinterMod.allowDispenserFull = allowDispenserFull;
                MealPrinterMod.allowForbidden = allowForbidden;
                MealPrinterMod.allowSociallyImproper = allowSociallyImproper;
            }

            static void Postfix(ref Thing __result)
            {
                MealPrinterMod.BestFoodSourceOnMap = false;
            }
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFinalIngestibleDef))]
        public static class Harmony_FoodUtility_GetFinalIngestibleDef
        {
            static bool Prefix(ref Thing foodSource, ref ThingDef __result)
            {
                if (foodSource is Building_MealPrinter && MealPrinterMod.BestFoodSourceOnMap)
                {
                    Building_MealPrinter printer = (Building_MealPrinter)foodSource;
                    __result = printer.GetMealThing();
                    return false;
                }

                return true;
            }
        }

        //the method was private so i just copy pasted that bitch
        private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
        {
            if (!allowSociallyImproper)
            {
                bool animalsCare = !getter.RaceProps.Animal;
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(FoodUtility), "SpawnedFoodSearchInnerScan", null)]
        public static class Harmony_FoodUtility_SpawnedFoodSearchInnerScan
        {
            static bool Prefix(ref Predicate<Thing> validator)
            {
                var malidator = validator;
                bool salivator(Thing x) => x is Building_MealPrinter rep ? PrintDel(rep) : malidator(x);
                validator = salivator;
                return true;
            }

            private static bool PrintDel(Building_MealPrinter t)
            {
                if (
                    !MealPrinterMod.allowDispenserFull
                    || !(MealPrinterMod.getter.RaceProps.ToolUser && MealPrinterMod.getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    || t.Faction != MealPrinterMod.getter.Faction && t.Faction != MealPrinterMod.getter.HostFaction
                    || !MealPrinterMod.allowForbidden && t.IsForbidden(MealPrinterMod.getter)
                    || !t.powerComp.PowerOn
                    || !t.InteractionCell.Standable(t.Map)
                    || !IsFoodSourceOnMapSociallyProper(t, MealPrinterMod.getter, MealPrinterMod.eater, MealPrinterMod.allowSociallyImproper)
                    || MealPrinterMod.getter.IsWildMan()
                    || !t.CanPawnPrint(MealPrinterMod.eater)
                    || !t.HasEnoughFeedstockInHoppers()
                    || !MealPrinterMod.getter.Map.reachability.CanReachNonLocal(MealPrinterMod.getter.Position, new TargetInfo(t.InteractionCell, t.Map),
                        PathEndMode.OnCell, TraverseParms.For(MealPrinterMod.getter, Danger.Some)))
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(JobDriver_FoodDeliver), nameof(JobDriver_FoodDeliver.GetReport))]
        public static class Harmony_JobDriver_FoodDeliver_GetReport
        {
            static void Postfix(JobDriver_FoodDeliver __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter &&
                    (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "printed meal")
                        .Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), nameof(JobDriver_FoodFeedPatient.GetReport))]
        public static class Harmony_JobDriver_FoodFeedPatient_GetReport
        {
            static void Postfix(JobDriver_FoodFeedPatient __instance, ref string __result)
            {
                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter && (Pawn)__instance.job.targetB.Thing != null)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "printed meal").Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
                }
            }
        }

        [HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.GetReport))]
        public static class Harmony_JobDriver_Ingest_GetReport
        {
            static void Postfix(JobDriver_Ingest __instance, ref string __result)
            {

                if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter)
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", "printed meal");
                }
                else
                {
                    __result = __instance.job.def.reportString.Replace("TargetA", __instance.job.GetTarget(TargetIndex.A).Thing.Label);
                }

            }
        }

        [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
        public static class Harmony_ThingListGroupHelper_Includes
        {
            static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
            {
                if (group == ThingRequestGroup.FoodSource || group == ThingRequestGroup.FoodSourceNotPlantOrTree)
                {
                    if (def.thingClass == typeof(Building_MealPrinter))
                    {
                        __result = true;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser))]
        public static class Harmony_Toils_Ingest_TakeMealFromDispenser
        {
            static bool Prefix(ref TargetIndex ind, ref Pawn eater, ref Toil __result)
            {
                if (eater.jobs.curJob.GetTarget(ind).Thing is Building_MealPrinter)
                {
                    var windex = ind;
                    var toil = new Toil();
                    toil.initAction = delegate
                    {
                        var actor = toil.actor;
                        var curJob = actor.jobs.curJob;
                        var printer = (Building_MealPrinter)curJob.GetTarget(windex).Thing;

                        var PawnForMealScan = actor;
                        if (curJob.GetTarget(TargetIndex.B).Thing is Pawn p)
                        {
                            PawnForMealScan = p;
                        }

                        var thing = printer.TryDispenseFood();
                        if (thing == null)
                        {
                            actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                            return;
                        }

                        actor.carryTracker.TryStartCarry(thing);
                        actor.CurJob.SetTarget(windex, actor.carryTracker.CarriedThing);
                    };
                    toil.FailOnCannotTouch(ind, PathEndMode.Touch);
                    toil.defaultCompleteMode = ToilCompleteMode.Delay;
                    toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;
                    __result = toil;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsFoodDispenser), MethodType.Getter)]
        public static class Harmony_ThingDef_IsFoodDispenser
        {
            // stop need hoppers alert

            static bool Prefix(ThingDef __instance, ref bool __result)
            {
                if (__instance.thingClass == typeof(Building_MealPrinter))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}
