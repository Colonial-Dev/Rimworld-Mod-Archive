using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Clonebay
{
    public class Alert_GrowerMaintence : Alert_Critical
    {
        public Alert_GrowerMaintence()
        {
            defaultLabel = "QE_AlertMaintenceRequiredLabel".Translate();
            defaultExplanation = "QE_AlertMaintenceRequiredExplanation".Translate();
        }

        public IEnumerable<Building> GrowersNeedingMaintence()
        {
            //return Find.CurrentMap.listerBuildings.allBuildingsColonist.Where(building => building is Building_GrowerBase grower && grower.status == CrafterStatus.Crafting && building is IMaintainableGrower maintainable && (maintainable.DoctorMaintence < 0.1f || maintainable.ScientistMaintence < 0.1f));
            return null;
        }

        public override AlertReport GetReport()
        {
            IEnumerable<Building> growersNeedingMaintence = GrowersNeedingMaintence();
            if(growersNeedingMaintence != null)
            {
                List<GlobalTargetInfo> culprits = new List<GlobalTargetInfo>();
                foreach(Building grower in growersNeedingMaintence)
                {
                    culprits.Add(grower);
                    AlertReport report = new AlertReport();
                    report.culpritsTargets = culprits;
                    report.active = true;
                    return report;
                }
            }

            return false;
        }
    }
}
