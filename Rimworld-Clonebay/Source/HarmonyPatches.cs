using System;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Clonebay
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);


        static HarmonyPatches()
        {
            //Harmony
            var harmony = new Harmony("rimworld.captainhaywood.clonebay");

            harmony.Patch(original: AccessTools.Method(type: typeof(TaleUtility), name: nameof(TaleUtility.Notify_PawnDied)), prefix: null,
            postfix: new HarmonyMethod(methodType: patchType, methodName: nameof(Notify_PawnDiedPostfix)));

            {
                //need a prefix on the pawn death method that checks for neural scanner implant;
                //if checks out as yes, then yeet em back to life in a clonebay
                //if checks out as no, proceed as normal (i.e. they ded)


            }

        }

        public static void Notify_PawnDiedPostfix(Pawn victim)
        {
            RaceProperties raceProps = victim.RaceProps;
            var pawnIsImplanted = victim.health?.hediffSet?.GetFirstHediffOfDef(QEHediffDefOf.Clonebay_Implant);
            if (raceProps.Humanlike && victim.IsColonist && pawnIsImplanted != null)
            {
                Thing genomeSequence = GenomeUtility.MakeGenomeSequence(victim, QEThingDefOf.Clonebay_GenomeSequencerFilled);
                if(genomeSequence == null)
                {
                    Log.Warning("CloneBay: Failed to create a new genomesequence");
                    return;
                }
                GenomeSequence genome = genomeSequence as GenomeSequence;

                IntVec3 cell = victim.Position;

                victim.equipment.DropAllEquipment(cell);
                victim.apparel.DropAll(cell);
                victim.inventory.DropAllNearPawn(cell);

                Pawn prevPawn = victim as Pawn;

                foreach (Building_PawnVatGrower vatToUse in victim.Map.listerBuildings.AllBuildingsColonistOfClass<Building_PawnVatGrower>())
                {
                    if (vatToUse.status != CrafterStatus.Crafting)
                    {
                        float fuel = vatToUse.TryGetComp<CompRefuelable>().Fuel;
                        if (fuel > 99)
                        {
                            vatToUse.Notify_CraftingStarted(genome, prevPawn, cell);
                            vatToUse.TryGetComp<CompRefuelable>().ConsumeFuel(100);
                        }
                        break;
                    }
                }
            }
        }
    }
}
