using Verse;

namespace Clonebay
{
    /// <summary>
    /// Points to the ThingDef for the GenomeSequencer.
    /// </summary>
    public class RecipeOutcomeProperties : DefModExtension
    {
        public ThingDef outputThingDef;
        public HediffDef outcomeHediff;
        public float outcomeHediffPotency = 1f;
    }
}
