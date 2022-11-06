using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PermadeathEnforcementSuite
{
    public static class StorytellerLock
    {
        [HarmonyPatch(typeof(Listing_Standard), nameof(Listing_Standard.ButtonText), new Type[] { typeof(string), typeof(string), typeof(float)})]
        public static class Patch_Listing_Standard_ButtonText {
            public static bool Prefix(ref string label) {
                if (label == Translator.Translate("ChangeStoryteller") && !LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().allowStorytellerAccess)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
