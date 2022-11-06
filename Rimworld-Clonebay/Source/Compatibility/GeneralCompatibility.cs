using System.Collections.Generic;
using Verse;

namespace Clonebay
{
    /// <summary>
    /// Stores general compatibility information for the Genome sequencer and Brain scanner.
    /// </summary>
    public static class GeneralCompatibility
    {
        public static List<ThingDef> excludedRaces = new List<ThingDef>();
        public static List<HediffDef> excludedHediffs = new List<HediffDef>();
    }
}
