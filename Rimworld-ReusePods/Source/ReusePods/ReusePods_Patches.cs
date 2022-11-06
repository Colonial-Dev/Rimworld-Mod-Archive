using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ReusePods
{
    [StaticConstructorOnStartup]
    public static class ReusePods_Patches
    {
        
        [HarmonyPatch(typeof(TransportPodsArrivalActionUtility), nameof(TransportPodsArrivalActionUtility.DropTravelingTransportPods), new Type[] { typeof(List<ActiveDropPodInfo>), typeof(IntVec3), typeof(Map)})]
        public static class Patch_TransportPodsArrivalActionUtility_DropTravelingTransportPods {
            public static bool Prefix(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map) {
                //Log.Message("prefix is runnign"); //my last two brain cells typed this

                if (Current.Game.CurrentMap != null)
                {

                    foreach (Building_PodFunnel funnel in map.listerBuildings.AllBuildingsColonistOfClass<Building_PodFunnel>()) {
                        //Log.Message("Searching funnels...");
                        if (funnel.Position.Roofed(funnel.Map)) {
                            //Log.Message("Funnel roofed, continuing...");
                            continue;
                        }

                        if (ReusePods_Utils.FindLandingPads(map, funnel.Position).Count < dropPods.Count) {
                            //Log.Message("Splitting pods");
                            List<ActiveDropPodInfo> splitPods = new List<ActiveDropPodInfo>();
                            for (int i = 0; i < ReusePods_Utils.FindLandingPads(map, funnel.Position).Count; i++) {
                                splitPods.Add(dropPods[i]);
                                dropPods.RemoveAt(i);
                            }
                            ReusePods_Utils.LandDropPods(splitPods, map, funnel.Position, near);
                            continue;
                        }

                        ReusePods_Utils.LandDropPods(dropPods, map, funnel.Position, near);

                        return false;
                    }

                    return true;
                }

                return true;
            }

        }

        [HarmonyPatch(typeof(ActiveDropPod), "PodOpen", null)]
        internal static class PodOpen
        {
            private class state
            {
                internal Map map;
                internal IntVec3 position;
            }

            private static void Prefix(ref state __state, ActiveDropPod __instance)
            {
                __state = new state();
                __state.map = __instance.Map;
                __state.position = __instance.Position;
            }

            private static void Postfix(ref state __state, ActiveDropPod __instance)
            {
                if (ReusePods_Utils.CanLandAt(__state.map, __state.position))
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("TransportPod"));
                    thing.SetFaction(Faction.OfPlayer);
                    GenSpawn.Spawn(thing, __state.position, __state.map);
                }
            }
        }


        static ReusePods_Patches() {
            Log.Message("[ReusePods] Okay, showtime!");          
            Harmony har = new Harmony("ReusePods");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
