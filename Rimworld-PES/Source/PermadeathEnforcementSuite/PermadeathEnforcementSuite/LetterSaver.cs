using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PermadeathEnforcementSuite
{
    public static class LetterSaver
    {

        [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), new Type[] { typeof(Letter), typeof(string)})]
        public static class Patch_LetterStack_ReceiveLetter {
            public static void Postfix(Letter let, string debugInfo = null) {
                if (Current.Game.Info.permadeathMode) {
                    if (!LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().saveOnPositiveEvents)
                    {
                        if (let.def == LetterDefOf.NeutralEvent || let.def == LetterDefOf.NewQuest || let.def == LetterDefOf.PositiveEvent || let.def == LetterDefOf.RelicHuntInstallationFound || let.def == LetterDefOf.RitualOutcomePositive)
                        {
                            return;
                        }
                        else
                        {
                            Autosave();
                            return;
                        }
                    }
                    else {
                        Autosave();
                        return;
                    }
                }
            }

            public static void Autosave() {
                if (LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().disableDoubleAutosave)
                {
                    Current.Game.autosaver.DoAutosave();
                }
                else {
                    Current.Game.autosaver.DoAutosave();
                    Current.Game.autosaver.DoAutosave();
                }
            }
        }

    }
}
