using RimWorld;
using System.Collections.Generic;
using Verse;


namespace Spaceports
{
    public class Alert_AutoEvacInProgress : Alert_Critical
    {

        private static string ColoniesUnderLockdown
        {
            get
            {
                string output = "Spaceports_AutoEvacDesc".Translate();
                List<Map> maps = Find.Maps;
                if (maps != null)
                {
                    foreach (Map map in maps)
                    {
                        if (map.IsPlayerHome && GenHostility.AnyHostileActiveThreatToPlayer(map, true) && Utils.AnyShuttlesOnMap(map))
                        {
                            output = output + "- " + map.info.parent.Label + "\n";
                        }
                    }
                }
                return output;
            }
        }

        public Alert_AutoEvacInProgress()
        {
            defaultLabel = "Spaceports_AutoEvac".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return ColoniesUnderLockdown;
        }

        public override AlertReport GetReport()
        {
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().autoEvacuate)
            {
                return false;
            }
            if (Find.AnyPlayerHomeMap == null)
            {
                return false;
            }
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome && GenHostility.AnyHostileActiveThreatToPlayer(maps[i], true) && Utils.CheckIfSpaceport(maps[i]) && Utils.AnyShuttlesOnMap(maps[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
