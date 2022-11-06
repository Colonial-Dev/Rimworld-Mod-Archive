using HarmonyLib;
using RimWorld;
using Spaceports.LordToils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports
{
    [StaticConstructorOnStartup]
    public static class SpaceportsHarmony
    {

        static SpaceportsHarmony()
        {
            Log.Message("[Spaceports] Okay, showtime!");
            Harmony har = new Harmony("Spaceports_Base");
            har.PatchAll(Assembly.GetExecutingAssembly());
            TryModPatches();
        }

        public static void TryModPatches()
        {

            if (Verse.ModLister.HasActiveModWithName("Hospitality")) //conditional patch to Hospitality
            {
                Harmony harmony = new Harmony("Spaceports_Plus_Hospitality");
                Log.Message("[Spaceports] Hospitality FOUND, attempting to patch...");
                var mOriginalA = AccessTools.Method("Verse.AI.Group.LordMaker:MakeNewLord");
                var mPostfixA = typeof(SpaceportsHarmony).GetMethod("LordMaker_MakeNewLordPostfix");
                var mOriginalB = AccessTools.Method("Verse.AI.Group.LordManager:ExposeData");
                var mPostfixB = typeof(SpaceportsHarmony).GetMethod("LordManager_ExposeDataPostfix");
                var mOriginalC = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:TryExecuteWorker");
                var mPrefixC = typeof(SpaceportsHarmony).GetMethod("IncidentWorker_VisitorGroup_TryExecuteWorkerPrefix");

                if (mOriginalA != null)
                {
                    var patch = new HarmonyMethod(mPostfixA);
                    Log.Message("[Spaceports] Attempting to postfix Verse.AI.Group.LordMaker.MakeNewLord...");
                    harmony.Patch(mOriginalA, postfix: patch);
                }

                if (mOriginalB != null)
                {
                    var patch = new HarmonyMethod(mPostfixB);
                    Log.Message("[Spaceports] Attempting to postfix Verse.AI.Group.LordManager.ExposeData...");
                    harmony.Patch(mOriginalB, postfix: patch);
                }

                if (mOriginalC != null)
                {
                    var patch = new HarmonyMethod(mPrefixC);
                    Log.Message("[Spaceports] Attempting to prefix Hospitality.IncidentWorker_VisitorGroup.TryExecuteWorker...");
                    harmony.Patch(mOriginalC, prefix: patch);
                }

            }
            else
            {
                Log.Message("[Spaceports] Hospitality not found, patches bypassed.");
            }

            if (Verse.ModLister.HasActiveModWithName("Save Our Ship 2") && Verse.ModLister.HasActiveModWithName("Hospitality")) //Conditional integration patches with SOS2/Hosp
            {
                Harmony harmony = new Harmony("Spaceports_Hospitality_SOS2_Bridge");
                Log.Message("[Spaceports] Hospitality and SOS2 FOUND, attempting integration patch...");
                var mOriginalA = AccessTools.Method("Hospitality.ItemUtility:PocketHeadgear");
                var mPrefixA = typeof(SpaceportsHarmony).GetMethod("PocketHeadgearPrefix");
                var mOriginalB = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:FactionCanBeGroupSource");
                var mPostfixB = typeof(SpaceportsHarmony).GetMethod("FactionCanBeGroupSourcePostfix");
                var mOriginalC = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:CheckCanCome");
                var mPostfixC = typeof(SpaceportsHarmony).GetMethod("CheckCanComePostfix");

                if (mOriginalA != null)
                {
                    var patch = new HarmonyMethod(mPrefixA);
                    Log.Message("[Spaceports] Attempting to prefix Hospitality.ItemUtility.PocketHeadgear...");
                    harmony.Patch(mOriginalA, prefix: patch);
                }
                if (mOriginalB != null)
                {
                    var patch = new HarmonyMethod(mPostfixB);
                    Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.FactionCanBeGroupSource...");
                    harmony.Patch(mOriginalB, postfix: patch);
                }
                if (mOriginalC != null)
                {
                    var patch = new HarmonyMethod(mPostfixC);
                    Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.CheckCanCome...");
                    harmony.Patch(mOriginalC, postfix: patch);
                }
            }
            else
            {
                Log.Message("[Spaceports] Hospitality + SOS2 not found, integration patches bypassed.");
            }

            //conditional patch to Trader Ships/Themis Traders (they use the same assembly under the hood LMFAOOO)
            if (Verse.ModLister.HasActiveModWithName("Trader ships") || Verse.ModLister.HasActiveModWithName("Rim-Effect: Themis Traders")) 
            {
                Harmony harmony = new Harmony("Spaceports_Plus_TraderShips");
                Log.Message("[Spaceports] Trader Ships/Themis Traders FOUND, attempting to patch...");
                var mOriginal = AccessTools.Method("TraderShips.IncidentWorkerTraderShip:FindCloseLandingSpot");
                var mPostfix = typeof(SpaceportsHarmony).GetMethod("FindCloseLandingSpotPostfix");

                if (mOriginal != null)
                {
                    var patch = new HarmonyMethod(mPostfix);
                    Log.Message("[Spaceports] Attempting to postfix TraderShips.IncidentWorkerTraderShip.FindCloseLandingSpot...");
                    harmony.Patch(mOriginal, postfix: patch);
                }
            }
            else
            {
                Log.Message("[Spaceports] Trader Ships not found, patches bypassed.");
            }
        }

        [HarmonyPatch(typeof(DropCellFinder), "GetBestShuttleLandingSpot", new Type[] { typeof(Map), typeof(Faction) })] //Royalty shuttle patch
        public static class Harmony_DropCellFinder_GetBestShuttleLandingSpot
        {
            static void Postfix(Map map, Faction factionForFindingSpot, ref IntVec3 __result)
            {
                if (!Utils.CheckIfClearForLanding(map, 0))
                {
                    return;
                }
                else
                {
                    __result = Utils.FindValidSpaceportPad(map, factionForFindingSpot, 0);
                }
                return;
            }
        }

        //Postfix to CompShuttle's Tick() that checks to see if any required pawns are despawned (e.g. left the map through alternate means) and removes them
        //from the shuttle's required list accordingly
        //This actually fixes a bug in the base game as well - ty Tynan very cool
        [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.CompTick))]
        private static class Harmony_CompShuttle_CompTick
        {
            static void Postfix(List<Pawn> ___requiredPawns, TransportShip ___shipParent)
            {
                if(Find.TickManager.TicksGame % 500 == 0)
                {
                    List<Pawn> targetPawns = new List<Pawn>();
                    foreach (Pawn pawn in ___requiredPawns)
                    {
                        if (!pawn.Spawned && !___shipParent.TransporterComp.innerContainer.Contains(pawn) && pawn.CarriedBy == null)
                        {
                            targetPawns.Add(pawn);
                        }
                    }
                    foreach (Pawn pawn in targetPawns)
                    {
                        ___requiredPawns.Remove(pawn);
                    }
                }
            }
        }

        //aka "where the fuck is our shuttle?"
        public static Thing TryFindGroupShuttle(Map map, Pawn pawn)
        {
            foreach (TransportShip ts in Find.TransportShipManager.AllTransportShips)
            {
                if (ts.ShuttleComp.requiredPawns.Contains(pawn))
                {
                    return ts.shipThing;
                }
            }
            return null;
        }

        //The Beast(TM)
        public static StateGraph PatchHospitalityGraph(LordJob lj, Thing shuttle, StateGraph sg, bool ExistingGroup = true, Map map = null, Faction faction = null, List<Pawn> pawns = null)
        {
            if (!ExistingGroup)
            {
                IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 3); //Find valid landing pad
                shuttle = Utils.GenerateInboundShuttle(pawns, pad, map).shipThing; //Initialize shuttle
            }

            StateGraph graphExit = new LordJobs.LordJob_DepartSpaceport(shuttle).CreateGraph(); //Intialize patched subgraph

            Lord lord = lj.lord; //Get Lord
            List<LordToil> lordToils = sg.lordToils.ToList(); //Get and copy Lord lordToils to list

            LordToil_TryShuttleWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TryShuttleWoundedGuest(shuttle, LocomotionUrgency.Sprint, canDig: false, interruptCurrentJob: true); //Initialize patched guest rescue
            lordToil_TakeWoundedGuest.lord = lord;
            lordToils.Add(lordToil_TakeWoundedGuest);

            LordToil patchedToilExit = sg.AttachSubgraph(graphExit).StartingToil; //Attach patched subgraph
            LordToil patchedToilExit2 = graphExit.lordToils[1]; //Get second toil from subgraph
            patchedToilExit.lord = lord; //Ensure lord is assigned correctly - stops "curLordToil lord is null (forgot to add toil to graph?)" bullshit
            patchedToilExit2.lord = lord; //ditto
            lordToils.Add(patchedToilExit); //Attach first patched toil
            lordToils.Add(patchedToilExit2); //Attach second patched toil
            sg.lordToils = lordToils; //Ensure lord graph is updated (might be redundant)


            List<Transition> transitions = sg.transitions.ToList(); //Get and copy transitions to list
            foreach (Transition transition in transitions)
            {
                foreach (Trigger trigger in transition.triggers) //foreach trigger in each transition
                {
                    //determine which transition we're dealing with by referencing its triggers
                    //might replace with string matching since this is the equivalent of fucking triangulation
                    if (trigger is Trigger_PawnExperiencingDangerousTemperatures || trigger is Trigger_BecamePlayerEnemy || (transition.preActions.Any(x => x is TransitionAction_Message) && !(trigger is Trigger_WoundedGuestPresent)))
                    {
                        transition.target = patchedToilExit;
                    }
                    //Edge case to patch wounded guest trigger
                    else if (trigger is Trigger_WoundedGuestPresent)
                    {
                        transition.target = lordToil_TakeWoundedGuest;
                    }
                    //Remove all occurences of a custom Hospitality preaction that throws NRE (I think because it assumes it is interacting w/ a LordToil_Travel but gets a LordToil_Wait)
                    foreach (TransitionAction preAction in transition.preActions.ToList())
                    {
                        if (preAction.ToString().Equals("Hospitality.TransitionAction_EnsureHaveNearbyExitDestination"))
                        {
                            transition.preActions.Remove(preAction);
                        }
                    }
                }
            }
            sg.transitions = transitions; //Ensure lord graph is updated (might be redundant)

            if (!ExistingGroup)
            {
                lord.Graph.transitions = transitions;
                lord.Graph.lordToils = lordToils;
            }

            return sg;
        }

        [HarmonyPostfix]//Hospitality patch, patches the StateGraphs of *new* Lords
        public static void LordMaker_MakeNewLordPostfix(Faction faction, LordJob lordJob, Map map, IEnumerable<Pawn> startingPawns = null)
        {
            if (lordJob.ToString() == "Hospitality.LordJob_VisitColony")
            {
                if (Utils.HospitalityShuttleCheck(map, faction))
                {
                    PatchHospitalityGraph(lordJob, null, lordJob.lord.Graph, false, map, faction, startingPawns.ToList());
                    return;
                }
                return; //Check failed, do not patch graph
            }
        }

        [HarmonyPostfix]//Hospitality patch, patches the StateGraphs of Lords loaded from save

        public static void LordManager_ExposeDataPostfix(LordManager __instance)
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Lord l in __instance.lords)
                {
                    if (l.LordJob.ToString() == "Hospitality.LordJob_VisitColony")
                    {
                        Thing shuttle = TryFindGroupShuttle(l.Map, l.ownedPawns[0]);
                        if (shuttle == null) { Log.Warning("[Spaceports] Could not find group shuttle when trying to patch Hospitality stategraph post-load.\nThis message is expected for groups that arrived on foot. Sampling pawn was: " + l.ownedPawns[0].Label); continue; }
                        PatchHospitalityGraph(l.LordJob, shuttle, l.Graph, true, l.Map, l.ownedPawns[0].Faction, l.ownedPawns);
                    }
                }
            }
        }

        [HarmonyPrefix]//Hospitality patch, stops visitor events from firing if they'd walk in and suffocate
        public static bool IncidentWorker_VisitorGroup_TryExecuteWorkerPrefix(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            bool RetVal = true;
            if (Utils.IsMapInSpace(map))
            {
                if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().hospitalityEnabled) { RetVal = false; }
                if (!Utils.CheckIfClearForLanding(map, 3)) { RetVal = false; }
                if (map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome)) { RetVal = false; }
            }

            if (!RetVal)
            {
                Log.Warning("[Spaceports] Surpressed dangerous or impossible visitor event on space map. This message is normal.");
            }

            return RetVal;
        }

        [HarmonyPrefix] //Hospitality/SOS2 bridge patch, stops pawns from removing their EVA helmets
        public static bool PocketHeadgearPrefix(Pawn pawn)
        {
            foreach(Apparel ap in pawn.apparel.WornApparel)
            {
                if(ap.def.defName == "Apparel_SpaceSuitHelmet")
                {
                    return false; //keep that EVA helmet on you fucking idiot
                }
            }
            return true;
        }

        [HarmonyPostfix] //Hospitality/SOS2 bridge patch, disqualifies neolithic factions from visiting space maps
        public static void FactionCanBeGroupSourcePostfix(Faction f, Map map, ref bool __result)
        {
            if(f.def.techLevel == TechLevel.Neolithic && Utils.IsMapInSpace(map))
            {
                __result = false;
            }
        }

        [HarmonyPostfix] //Hospitality/SOS2 bridge patch, stops temp concerns when trying to visit a space map
        public static void CheckCanComePostfix(Map map, Faction faction, ref TaggedString reasons, ref bool __result)
        {
            if (__result || reasons == null)
            {
                return;
            }

            String TranslatedTemp = "- " + "Temperature".Translate();
            if (reasons.ToString().Contains(TranslatedTemp))
            {
                TaggedString newstr = reasons.ToString().Replace(TranslatedTemp, "");
                reasons = newstr;
            }

            if (!reasons.ToString().Contains("-"))
            {
                __result = true;
            }
        }

        [HarmonyPostfix] //Trader Ships/Themis Traders patch
        public static void FindCloseLandingSpotPostfix(Map map, Faction faction, ref IntVec3 spot)
        {
            if(!Utils.CheckIfClearForLanding(map, 2))
            {
                return;
            }
            else
            {
                spot = Utils.FindValidSpaceportPad(map, faction, 2);
            }
            return;
        }

    }

}
