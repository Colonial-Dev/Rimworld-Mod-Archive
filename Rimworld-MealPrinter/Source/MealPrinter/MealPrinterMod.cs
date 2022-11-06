using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace MealPrinter
{
    public class MealPrinterSettings : ModSettings 
    {
        public bool printSoundEnabled = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref printSoundEnabled, "printSoundEnabled", true);
            base.ExposeData();
        }
    }
    
    public class MealPrinterMod : Mod
    {

        MealPrinterSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("PrintSFXLabel".Translate(), ref settings.printSoundEnabled);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "MealPrinterModName".Translate();
        }

        public static bool allowForbidden;
        public static bool allowDispenserFull;
        public static Pawn getter;
        public static Pawn eater;
        public static bool allowSociallyImproper;
        public static bool BestFoodSourceOnMap;

        public MealPrinterMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MealPrinterSettings>();
            Log.Message("[MealPrinter] Okay, showtime!");          
            Harmony har = new Harmony("MealPrinter");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
