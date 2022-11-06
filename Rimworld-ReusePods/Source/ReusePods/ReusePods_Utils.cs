using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ReusePods
{
    class ReusePods_Utils
    {

		internal static void LandDropPods(List<ActiveDropPodInfo> dropPods, Map map, IntVec3 funnelCell, IntVec3 selectedCell) {
			List<IntVec3> validPads = FindLandingPads(map, funnelCell);
			RimWorld.Planet.TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(dropPods);

			for (int i = 0; i < dropPods.Count; i++)
			{

				try {
					if (i <= validPads.Count && validPads.Count > 0)
					{
						DropPodUtility.MakeDropPodAt(validPads[i], map, dropPods[i]);
					}

					else if (i > validPads.Count || validPads.Count <= 0)
					{
						DropCellFinder.TryFindDropSpotNear(selectedCell, map, out var result, allowFogged: false, canRoofPunch: true);
						DropPodUtility.MakeDropPodAt(result, map, dropPods[i]);
					}

					else
					{
						DropCellFinder.TryFindDropSpotNear(selectedCell, map, out var result, allowFogged: false, canRoofPunch: true);
						DropPodUtility.MakeDropPodAt(result, map, dropPods[i]);
					}
				}
				catch (Exception e) {
					DropCellFinder.TryFindDropSpotNear(selectedCell, map, out var result, allowFogged: false, canRoofPunch: true);
					DropPodUtility.MakeDropPodAt(result, map, dropPods[i]);
				}

			}
		}
		
		internal static List<Building> FindLaunchersWithinRadius(Map map, IntVec3 center, float radius)
		{
			List<Building> outputList = new List<Building>();

			foreach (Building item in map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("PodLauncher")))
			{
				if (item.Position.DistanceTo(center) <= radius)
				{
					outputList.Add(item);
				}
			}

			if(Verse.ModLister.HasActiveModWithName("Vanilla Furniture Expanded - Power"))
			foreach (Building item in map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("VPE_GasPodLauncher")))
			{
				if (item.Position.DistanceTo(center) <= radius)
				{
					outputList.Add(item);
				}
			}

			return outputList;
		}

		internal static List<IntVec3> FindLandingPads(Map map, IntVec3 cell)
		{
			List<IntVec3> outputList = new List<IntVec3>();
			List<Building> bldgList = FindLaunchersWithinRadius(map, cell, 20f);
			foreach (Building item in bldgList)
			{
				IntVec3 fuelingPortCell = FuelingPortUtility.GetFuelingPortCell(item);
				if (CanLandAt(map, fuelingPortCell) && fuelingPortCell.Standable(map))
				{
					outputList.Add(fuelingPortCell);
				}
			}
			return outputList;
		}

		internal static bool CanLandAt(Map map, IntVec3 cell)
		{
			if (!cell.Roofed(map))
			{
				return FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(cell, map) != null;
			}

			return false;
		}

	}
}
