using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PermadeathEnforcementSuite
{
    [StaticConstructorOnStartup]
    class DevModeLock
    {
        static DevModeLock() {

            try
            {
                DevModePermanentlyDisabledUtility.Disable();
            }
            catch (Exception e) {
                Log.Warning("[PES] Exception while disabling devmode!" + e);
            }
        }
    }
}
