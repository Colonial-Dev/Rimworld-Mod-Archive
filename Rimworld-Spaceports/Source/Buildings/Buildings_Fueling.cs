using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using SharpUtils;
using UnityEngine;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace Spaceports.Buildings
{
    public class Building_FuelProcessor : Building
    {
        private int TotalProduced = 0;
        private int ProductionCache = 0;
        private int ProductionMode = 0;
        private int UnitsPerRareTick
        {
            get
            {
                if(GetProductionMode() == 0) { ProductionMode = 0; return 6; }
                else { ProductionMode = 1; return 2; }
            }

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TotalProduced, "TotalProduced", 0);
            Scribe_Values.Look(ref ProductionCache, "ProductionCache", 0);
            Scribe_Values.Look(ref ProductionMode, "ProductionMode", 0);
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (!this.InWater()) { ProductionMode = 1; }
            }
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_TotalFuelProduced".Translate(TotalProduced);
            str += "\n" + "Spaceports_ProductionCache".Translate(ProductionCache);
            if(ProductionMode == 0)
            {
                str += "\n" + "Spaceports_ModeWet".Translate();
            }
            else
            {
                str += "\n" + "Spaceports_ModeDry".Translate();
            }
            return str;
        }

        public override void Tick()
        {
            base.Tick();
            if(Find.TickManager.TicksGame % 500 == 0)
            {
                RareTick();
            }
        }

        public void RareTick()
        {
            if (this.GetComp<CompPowerTrader>().PowerOn) {
                CompRefuelable FuelComp = this.GetComp<CompRefuelable>();
                if (FuelComp != null && FuelComp.HasFuel)
                {
                    int UPRT = UnitsPerRareTick; //Cache value from getter to avoid excessive calls to reflected DBH methods
                    TotalProduced += UPRT;
                    ProductionCache += UPRT;
                }
                if (!GetLinkedTanks().NullOrEmpty() && CanAnyTankAcceptFuelNow()) 
                {
                    TryDistributeFuel(0);
                }
            }
        }

        private int GetProductionMode()
        {
            if (!Verse.ModLister.HasActiveModWithName("Dubs Bad Hygiene"))
            {
                if (!this.InWater())
                {
                    return 1;
                }

                return 0;
            }

            else
            {
                if (this.InWater())
                {
                    return 0;
                }

                if (PullWater())
                {
                    return 0;
                }
                return 1;
            }
        }

        private bool PullWater()
        {
            if(!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().dbhEnabled) { return false; }
            try
            {
                var comp = AllComps.FirstOrDefault(x => x.GetType() == SpaceportsMisc.CompPipe);
                var PipeNet = SpaceportsMisc.PipeNet(comp);

                if (PipeNet == null)
                {
                    return false;
                }

                object[] parms = new object[] { 8f, 0 };
                bool result = (bool)SpaceportsMisc.PullWater(PipeNet, parms);
                return result;
            }
            catch(Exception ex)
            {
                Log.ErrorOnce("[Spaceports] Error when trying to pull water from DBH PlumbingNet via reflection! Exception: " + ex.ToString(), this.thingIDNumber ^ 0x4475CF1F);
                return false;
            }

        }

        private bool InWater()
        {
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                if (!this.Map.terrainGrid.TerrainAt(cell).affordances.Contains(TerrainAffordanceDefOf.MovingFluid) && !this.Map.terrainGrid.TerrainAt(cell).affordances.Contains(SpaceportsDefOf.ShallowWater))
                {
                    return false;
                }
            }
            return true;
        }

        private List<Building_FuelTank> GetLinkedTanks()
        {
            List<Building_FuelTank> LinkedTanks = new List<Building_FuelTank>();
            foreach (Thing t in this.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading)
            {
                Building_FuelTank tank = t as Building_FuelTank;
                if (t != null)
                {
                    LinkedTanks.Add(tank);
                }
            }
            return LinkedTanks;
        }

        private bool CanAnyTankAcceptFuelNow()
        {
            bool result = false;
            foreach(Building_FuelTank tank in GetLinkedTanks())
            {
                if(tank.FuelLevel() < 1000)
                {
                    result = true;
                }
            }
            return result;
        }

        private void TryDistributeFuel(int drops)
        {
            int DropsRemaining = drops;
            DropsRemaining += ProductionCache;
            while(DropsRemaining > 0)
            {
                foreach (Building_FuelTank tank in GetLinkedTanks())
                {
                    if (DropsRemaining > 0 && tank.CanAcceptFuelNow(1))
                    {
                        tank.AddFuel(1);
                        DropsRemaining--;
                    }
                    else if(DropsRemaining <= 0)
                    {
                        break;
                    }
                }
                if (!CanAnyTankAcceptFuelNow())
                {
                    break; //Emergency breakout to prevent game lock
                }
            }
            ProductionCache = DropsRemaining;
        }
    }

    public class PlaceWorker_FuelProcessor : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 item in PlaceCells(loc))
            {
                if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.Heavy) && !map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid) && !map.terrainGrid.TerrainAt(item).affordances.Contains(SpaceportsDefOf.ShallowWater))
                {
                    return new AcceptanceReport("Spaceports_FFTerrain".Translate());
                }
            }
            return true;
        }

        public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
        {
            foreach (IntVec3 item in PlaceCells(loc))
            {
                if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid) && !map.terrainGrid.TerrainAt(item).affordances.Contains(SpaceportsDefOf.ShallowWater))
                {
                    if (!Verse.ModLister.HasActiveModWithName("Dubs Bad Hygiene"))
                    {
                        Messages.Message("Spaceports_DryModeWarning".Translate(), MessageTypeDefOf.CautionInput);
                    }
                    else
                    {
                        Messages.Message("Spaceports_DryModeWarningDBH".Translate(), MessageTypeDefOf.CautionInput);
                    }
                }
            }
            base.PostPlace(map, def, loc, rot);
        }

        public static IEnumerable<IntVec3> PlaceCells(IntVec3 center)
        {
            yield return new IntVec3(center.x - 1, center.y, center.z - 1);
            yield return new IntVec3(center.x, center.y, center.z - 1);
            yield return new IntVec3(center.x + 1, center.y, center.z - 1);

            yield return new IntVec3(center.x - 1, center.y, center.z);
            yield return center;
            yield return new IntVec3(center.x + 1, center.y, center.z);

            yield return new IntVec3(center.x - 1, center.y, center.z + 1);
            yield return new IntVec3(center.x, center.y, center.z + 1);
            yield return new IntVec3(center.x + 1, center.y, center.z + 1);
        }

        public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
        {
            yield return TerrainAffordanceDefOf.Heavy;
            yield return TerrainAffordanceDefOf.MovingFluid;
            yield return SpaceportsDefOf.ShallowWater;
        }
    }

    public class Building_FuelTank : Building
    {
        private int FusionFuelLevel = 0;
        private const int FuelCap = 1000;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref FusionFuelLevel, "FusionFuelLevel", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_FuelLevel".Translate(FusionFuelLevel);
            return str;
        }

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame % 250 == 0)
            {
                RareTick();
            }
        }

        public void RareTick()
        {
            if (!this.GetComp<CompPowerTrader>().PowerOn && this.FusionFuelLevel >= 0)
            {
                this.FusionFuelLevel = Mathf.Clamp(this.FusionFuelLevel - 4, 0, 1000);
            }
        }

        public int FuelLevel()
        {
            return FusionFuelLevel;
        }

        public bool CanAcceptFuelNow(int amount = 0)
        {
            if(FusionFuelLevel + amount > FuelCap)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void AddFuel(int amount)
        {
            FusionFuelLevel += amount;
        }

        public bool CanDrainFuelNow(int amount = 0)
        {
            if (FusionFuelLevel - amount < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void DrainFuel(int amount)
        {
            FusionFuelLevel -= amount;
        }

    }

    public class Building_FuelDispenser : Building
    {
        private int TotalSales = 0;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TotalSales, "TotalSales", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_TotalSales".Translate(TotalSales);
            str += "\n" + GetNetworkInfo();
            return str;
        }

        private string GetNetworkInfo()
        {
            string result = "";
            int NetworkFuelLevel = 0;
            List<Building_FuelTank> tanks = new List<Building_FuelTank>();

            foreach (Building_FuelTank tank in this.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FuelTank>())
            {
                if (tank.PowerComp.PowerNet == this.PowerComp.PowerNet)
                {
                    tanks.Add(tank);
                    NetworkFuelLevel += tank.FuelLevel();
                }
            }

            result += "Spaceports_NetworkInfo".Translate(tanks.Count, NetworkFuelLevel);
            return result;
        }

        private Thing GenSilver(int amount)
        {
            Thing product = ThingMaker.MakeThing(ThingDefOf.Silver);
            product.stackCount = amount;
            return product;
        }

        public bool TrySellFuel()
        {
            int FuelRequested = Rand.RangeInclusive(250, 750);
            List<Building_FuelTank> tanks = new List<Building_FuelTank>();

            foreach(Building_FuelTank tank in this.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FuelTank>())
            {
                if(tank.PowerComp.PowerNet == this.PowerComp.PowerNet)
                {
                    tanks.Add(tank);
                }
            }

            foreach(Building_FuelTank tank in tanks)
            {
                if (tank.CanDrainFuelNow(FuelRequested))
                {
                    tank.DrainFuel(FuelRequested);
                    this.TotalSales += (int)(FuelRequested * 0.25f);
                    Thing silver = GenSilver((int)(FuelRequested * 0.25f));
                    GenPlace.TryPlaceThing(silver, this.InteractionCell, this.Map, ThingPlaceMode.Near);
                    return true;
                }
            }

            List<Building_FuelTank> TankPool = new List<Building_FuelTank>();
            int PoolSize = 0;
            foreach(Building_FuelTank tank in tanks)
            {
                if(tank.FuelLevel() > 0)
                {
                    TankPool.Add(tank);
                    PoolSize += tank.FuelLevel();
                    if(PoolSize >= FuelRequested)
                    {
                        break;
                    }
                }
            }

            if(PoolSize >= FuelRequested)
            {
                this.TotalSales += (int)(FuelRequested * 0.25f);
                Thing silver = GenSilver((int)(FuelRequested * 0.25f));
                GenPlace.TryPlaceThing(silver, this.InteractionCell, this.Map, ThingPlaceMode.Near);
                foreach (Building_FuelTank tank in TankPool)
                {
                    if(FuelRequested >= tank.FuelLevel() && tank.CanDrainFuelNow(tank.FuelLevel()))
                    {
                        FuelRequested -= tank.FuelLevel();
                        tank.DrainFuel(tank.FuelLevel());
                    }
                    else if(FuelRequested < tank.FuelLevel() && tank.CanDrainFuelNow(tank.FuelLevel() - FuelRequested))
                    {
                        FuelRequested -= tank.FuelLevel() - FuelRequested;
                        tank.DrainFuel(tank.FuelLevel() - FuelRequested);
                    }
                    if(FuelRequested <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
