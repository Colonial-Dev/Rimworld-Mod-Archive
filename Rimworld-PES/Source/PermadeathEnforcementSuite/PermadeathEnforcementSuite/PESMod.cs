using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace PermadeathEnforcementSuite
{
    public class PESSettings : ModSettings
    {
        public bool saveOnPositiveEvents = false;
        public bool allowStorytellerAccess = false;
        public bool disableDoubleAutosave = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref saveOnPositiveEvents, "saveOnPositiveEvents", false);
            Scribe_Values.Look(ref allowStorytellerAccess, "allowStorytellerAccess", false);
            Scribe_Values.Look(ref disableDoubleAutosave, "disableDoubleAutosave", false);
            base.ExposeData();
        }
    }

    public class PESMod : Mod
    {

        PESSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            if (Current.Game == null)
            {
                listingStandard.CheckboxLabeled("PES_SaveOnPositive".Translate(), ref settings.saveOnPositiveEvents);
                listingStandard.CheckboxLabeled("PES_AllowStorytellerAccess".Translate(), ref settings.allowStorytellerAccess);
                listingStandard.CheckboxLabeled("PES_DisableDoubleAutosave".Translate(), ref settings.disableDoubleAutosave);
                listingStandard.Label("PES_Explanation".Translate());
            }
            else {
                listingStandard.Label("PES_NullGameOnly".Translate());
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "PES_ModName".Translate();
        }

        public PESMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PESSettings>();
            Log.Message("[PES] Okay, showtime!");
            Harmony har = new Harmony("PermadeathEnforcementSuite");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}