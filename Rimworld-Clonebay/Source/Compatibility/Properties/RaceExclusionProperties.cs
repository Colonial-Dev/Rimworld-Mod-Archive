using System.Collections.Generic;
using Verse;

namespace Clonebay
{
    /// <summary>
    /// If found on a ThingDef which is a pawn it is added to the exclusion list.
    /// </summary>
    public class RaceExclusionProperties : DefModExtension
    {
        public bool excludeThisRace = true;
        public List<HediffDef> excludeTheseHediffs = new List<HediffDef>();
    }
}
