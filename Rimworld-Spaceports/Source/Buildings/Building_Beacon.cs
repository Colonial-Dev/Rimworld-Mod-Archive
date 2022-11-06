using RimWorld;
using SharpUtils;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    class Building_Beacon : Building
    {
        private SharpAnim.SpinOver RadarDish;
        private SharpAnim.DrawOver RadarDishStill;
        private SharpAnim.AnimateOver RimLights;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            RadarDish = new SharpAnim.SpinOver(SpaceportsMats.RadarDish, this, 1.5f, 3f, 3f, PowerDependent: true);
            RadarDishStill = new SharpAnim.DrawOver(SpaceportsMats.RadarDish, this, 3f, 3f);
            RimLights = new SharpAnim.AnimateOver(SpaceportsMats.BeaconLights, this, 30, 3f, 3f);
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override string GetInspectString()
        {
            string info = base.GetInspectString();
            if (this.Map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome))
            {
                info += "Spaceports_BeaconKessler".Translate();
            }
            else if (!this.GetComp<CompPowerTrader>().PowerOn)
            {
                info += "Spaceports_BeaconUnpowered".Translate();
            }
            else if (!Utils.AnyPoweredSpaceportPads(this.Map))
            {
                info += "Spaceports_BeaconNoPads".Translate();
            }
            else if (Utils.AtShuttleCapacity(this.Map))
            {
                info += "Spaceports_BeaconAtCap".Translate();
            }
            else if (!Utils.AnyValidSpaceportPad(this.Map, 0))
            {
                info += "Spaceports_BeaconPadsFull".Translate();
            }
            else
            {
                info += "Spaceports_BeaconOK".Translate();
            }
            return info;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Toggle() //Lockdown toggle
            {
                defaultLabel = "Spaceports_ManualAirspaceLockdown".Translate(),
                defaultDesc = "Spaceports_ManualAirspaceLockdownTooltip".Translate(),
                isActive = () => this.Map.GetComponent<SpaceportsMapComp>().ForcedLockdown,
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Lockdown", true),
                disabled = !this.GetComp<CompPowerTrader>().PowerOn,
                disabledReason = "Spaceports_BeaconDisabledGeneral".Translate(),
                Order = -100,
                toggleAction = delegate ()
                {
                    this.Map.GetComponent<SpaceportsMapComp>().ForcedLockdown = !this.Map.GetComponent<SpaceportsMapComp>().ForcedLockdown;
                }
            };
            yield return new Command_Action() //Dismiss all shuttles command
            {
                defaultLabel = "Spaceports_DismissAll".Translate(),
                defaultDesc = "Spaceports_DismissAllTooltip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/FuckOff", true),
                disabled = !this.GetComp<CompPowerTrader>().PowerOn,
                disabledReason = "Spaceports_BeaconDisabledGeneral".Translate(),
                Order = -100,
                action = delegate ()
                {
                    DismissAll();
                }
            };
            yield return new Command_Action() //Recall all shuttle parties command
            {
                defaultLabel = "Spaceports_RecallAll".Translate(),
                defaultDesc = "Spaceports_RecallAllTooltip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/ComeBack", true),
                disabled = !this.GetComp<CompPowerTrader>().PowerOn,
                disabledReason = "Spaceports_BeaconDisabledGeneral".Translate(),
                Order = -100,
                action = delegate ()
                {
                    RecallAll();
                }
            };
            yield return new Command_Action() //Call taxi command
            {
                defaultLabel = "Spaceports_CallTaxi".Translate(),
                defaultDesc = "Spaceports_CallTaxiTooltip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/CallTaxi", true),
                disabled = !this.GetComp<CompPowerTrader>().PowerOn,
                disabledReason = "Spaceports_CallTaxiDisabled".Translate(),
                Order = -100,
                action = delegate ()
                {
                    CallTaxiShuttle();
                }
            };
        }

        private void CallTaxiShuttle()
        {

            Dialogs.Dialog_CallShuttle window = new Dialogs.Dialog_CallShuttle(delegate ()
            {
                ConfirmAction();
            }, TradeUtility.ColonyHasEnoughSilver(this.Map, 500), Utils.AnyValidSpaceportPad(this.Map, 0));
            Find.WindowStack.Add(window);
        }

        private void ConfirmAction()
        {
            TradeUtility.LaunchSilver(this.Map, 500);
            Thing thing = ThingMaker.MakeThing(SpaceportsDefOf.Spaceports_RoyaltyShuttle);
            IntVec3 pad = Utils.FindValidSpaceportPad(Find.CurrentMap, null, 0);
            thing.TryGetComp<CompShuttle>().permitShuttle = true;

            Building_Shuttle b = thing as Building_Shuttle;
            if(b != null)
            {
                b.disabled = true;
            }

            TransportShip transportShip = TransportShipMaker.MakeTransportShip(SpaceportsDefOf.Spaceports_RoyaltyShuttleTS, null, thing);
            transportShip.ArriveAt(pad, this.Map.Parent);
            transportShip.AddJobs(ShipJobDefOf.WaitForever, ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
        }

        private void DismissAll()
        {
            List<Building_Shuttle> shuttles = new List<Building_Shuttle>();
            foreach (Building b in this.Map.listerBuildings.allBuildingsNonColonist)
            {
                Buildings.Building_Shuttle target = b as Spaceports.Buildings.Building_Shuttle;
                if (target != null)
                {
                    if (!target.disabled)
                    {
                        shuttles.Add(target);
                    }
                }
            }
            foreach (Building_Shuttle b in shuttles)
            {
                b.ForceImmediateDeparture();
            }
        }

        private void RecallAll()
        {
            List<Building_Shuttle> shuttles = new List<Building_Shuttle>();
            foreach (Building b in this.Map.listerBuildings.allBuildingsNonColonist)
            {
                Buildings.Building_Shuttle target = b as Spaceports.Buildings.Building_Shuttle;
                if (target != null)
                {
                    if (!target.disabled)
                    {
                        shuttles.Add(target);
                    }
                }
            }
            foreach (Building_Shuttle b in shuttles)
            {
                b.RecallParty();
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().beaconAnimationsGlobal)
            {
                if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().beaconRadarAnimations)
                {
                    RadarDish.Draw();
                }
                if (this.GetComp<CompPowerTrader>().PowerOn && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().beaconRimAnimations)
                {
                    RimLights.Draw();
                }
            }
            else
            {
                RadarDishStill.Draw();
            }

        }
    }
}
