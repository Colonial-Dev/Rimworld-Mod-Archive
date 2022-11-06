using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ReusePods {
    public class Building_PodFunnel : Building
    {

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (this.Position.Roofed(this.Map))
            {
                text = "Pod funnel offline - blocked by roof.";
            }
            else if (!this.Position.Roofed(this.Map)) {
                text = "Pod funnel operational.";
                text = text + "\nCurrently within funneling range of " + getServiceCount() + " pod launchers.";
            }
            return text;
        }

        private int getServiceCount() {
            List<Building> outputList = new List<Building>();

            foreach (Building item in this.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("PodLauncher")))
            {
                if (item.Position.DistanceTo(this.Position) <= 20f)
                {
                    outputList.Add(item);
                }
            }

            if (Verse.ModLister.HasActiveModWithName("Vanilla Furniture Expanded - Power"))
            {
                foreach (Building item in this.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("VPE_GasPodLauncher")))
                {
                    if (item.Position.DistanceTo(this.Position) <= 20f)
                    {
                        outputList.Add(item);
                    }
                }
            }

            return outputList.Count;
        }

    }

}
