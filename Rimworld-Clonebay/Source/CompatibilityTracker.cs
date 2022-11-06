using System.Linq;
using Verse;

namespace Clonebay
{
    [StaticConstructorOnStartup]
    public static class CompatibilityTracker
    {
        private static bool alienRacesActiveInt = false;

        public static bool AlienRacesActive
        {
            get
            {
                return alienRacesActiveInt;
            }
        }

        private static bool deathRattleActiveInt = false;

        public static bool DeathRattleActive
        {
            get
            {
                return deathRattleActiveInt;
            }
        }

        static CompatibilityTracker()
        {
            //Check for Alien Races Compatiblity.
            Log.Message("Clonebay is checking compatibility for: Alien Race Framework...");
            if(GenTypes.AllTypes.Any(type => type.FullName == "AlienRace.ThingDef_AlienRace"))
            {
                alienRacesActiveInt = true;
                Log.Message("Alien Race Framework OK.");
            }
            else
            {
                Log.Message("No compatibility.");
            }
        }
    }
}
