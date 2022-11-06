using RimWorld;
using SharpUtils;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    public class Building_ShuttlePad : Building
    {
        private bool ShuttleInbound = false;
        private int ticksSinceReserved = 0;

        private int AccessState = 0; //-1 for none, 0 for all, 1 for visitors, 2 for traders, 3 for hospitality guests

        private SharpAnim.AnimateOver landingPatternAnimation;
        private SharpAnim.AnimateOver rimLightAnimation;
        private SharpAnim.DrawOver holdingPattern;
        private SharpAnim.DrawOver blockedPattern;

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (IsUnroofed() == false)
            {
                text += "Spaceports_RoofBlocking".Translate();
            }
            return text;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            landingPatternAnimation = new SharpAnim.AnimateOver(SpaceportsMats.LandingPadTouchdownLights, this, 30, 7f, 5f);
            rimLightAnimation = new SharpAnim.AnimateOver(SpaceportsMats.LandingPadRimLights, this, 30, 7f, 5f);
            holdingPattern = new SharpAnim.DrawOver(SpaceportsMats.HoldingPatternGraphic, this, 7f, 5f);
            blockedPattern = new SharpAnim.DrawOver(SpaceportsMats.BlockedPatternGraphic, this, 7f, 5f);
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AccessState, "accessState", 0);
            Scribe_Values.Look(ref ShuttleInbound, "ShuttleInbound", false);
        }

        public override void Draw()
        {
            if (IsPowered())
            {
                if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().padAnimationsGlobal)
                {
                    if (ShuttleInbound && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().landingAnimations && IsPowered())
                    {
                        landingPatternAnimation.Draw();
                    }
                    if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().rimLightsAnimations && IsPowered())
                    {
                        rimLightAnimation.Draw();
                    }
                }
                if (IsShuttleOnPad() && IsPowered())
                {
                    holdingPattern.Draw();
                }
                if (!IsShuttleOnPad() && !IsUnroofed() && IsPowered())
                {
                    blockedPattern.Draw();
                }
                if (AccessState == -1 && IsPowered())
                {
                    blockedPattern.Draw();
                }
            }

            base.Draw();
        }

        public override void Tick()
        {
            if (IsShuttleOnPad())
            {
                if (ShuttleInbound && Rand.Chance(0.70f))
                {
                    Building_FuelDispenser dispenser = GetFuelDispenser();
                    if(dispenser != null)
                    {
                        dispenser.TrySellFuel();
                    }
                }
                ShuttleInbound = false;
            }
            if (ShuttleInbound)
            {
                ticksSinceReserved++;
                if (ticksSinceReserved > 500) //fallback if a shuttle fails to spawn, stops the pad from being locked up forever
                {
                    ShuttleInbound = false;
                }
            }
            base.Tick();
        }

        private Building_FuelDispenser GetFuelDispenser() 
        {
            List<Thing> LinkedThings = this.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading;
            foreach(Thing thing in LinkedThings)
            {
                Building_FuelDispenser d = thing as Building_FuelDispenser;
                if(d != null)
                {
                    return d;
                }
            }
            return null;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {

                defaultLabel = "AccessControlButton".Translate(),
                defaultDesc = "AccessControlDesc".Translate(),
                icon = GetAccessIcon(),
                Order = -100,
                action = delegate ()
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Utils.AccessControlState state in SpaceportsMisc.AccessStates)
                    {
                        string label = state.GetLabel();
                        FloatMenuOption option = new FloatMenuOption(label, delegate ()
                        {
                            SetAccessState(state.getValue());
                        });
                        options.Add(option);
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            };

        }

        public void NotifyIncoming()
        {
            ShuttleInbound = true;
            ticksSinceReserved = 0;
        }

        public bool IsAvailable()
        {
            if (!IsUnroofed() || IsShuttleOnPad() || ShuttleInbound || !IsPowered())
            {
                return false;
            }
            if (CheckAirspaceLockdown())
            {
                return false;
            }
            return true;
        }

        public bool IsPowered()
        {
            return this.GetComp<CompPowerTrader>().PowerOn;
        }

        public bool IsUnroofed()
        {
            if (Verse.ModLister.HasActiveModWithName("Save Our Ship 2"))
            {
                foreach (IntVec3 cell in this.OccupiedRect().Cells)
                {
                    if (cell.Roofed(this.Map) && !this.Map.roofGrid.RoofAt(cell).defName.Equals("RoofShip"))
                    {
                        return false;
                    }
                }
                return true;
            }

            else
            {
                foreach (IntVec3 cell in this.OccupiedRect().Cells)
                {
                    if (cell.Roofed(this.Map))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool IsShuttleOnPad()
        {
            if (this.Position.GetFirstThingWithComp<CompShuttle>(this.Map) != null)
            {
                return true;
            }
            if (!this.Position.Standable(this.Map))
            {
                return true;
            }
            return false;
        }

        private void SetAccessState(int val)
        {
            AccessState = val;
        }

        public bool CheckAccessGranted(int val)
        {
            if (AccessState == -1)
            {
                return false;
            }
            if (AccessState == 0 || val == 0)
            {
                return true;
            }
            else return AccessState == val;
        }

        private bool CheckAirspaceLockdown()
        {
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown)
            {
                return false;
            }
            return GenHostility.AnyHostileActiveThreatToPlayer(this.Map, true);
        }

        private Texture2D GetAccessIcon()
        {
            if (AccessState == -1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_a", true);
            }
            else if (AccessState == 0)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_b", true);
            }
            else if (AccessState == 1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_c", true);
            }
            else if (AccessState == 2)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpotOverlay/ChillSpot_d", true);
            }
            else if (AccessState == 3)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpot_guests", true);
            }
            return null;
        }

    }
}
