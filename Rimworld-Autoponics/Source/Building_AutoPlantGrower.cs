//Building_AutoPlantGrower
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace AutoPonics {
    public class Building_AutoPlantGrower : Building, IPlantToGrowSettable
    {
        private ThingDef plantDefToGrow;

        private bool growingSuspended;

        private CompPowerTrader compPower;

        IEnumerable<IntVec3> IPlantToGrowSettable.Cells => this.OccupiedRect().Cells;

        public IEnumerable<Plant> PlantsOnMe
        {
            get
            {
                if (!base.Spawned)
                {
                    yield break;
                }

                foreach (IntVec3 item in this.OccupiedRect()) {
                    List<Thing> thingList = base.Map.thingGrid.ThingsListAt(item);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Plant p = thingList[i] as Plant;
                        if (p != null)
                        {
                            yield return p;
                        }
                        if (p == null && !growingSuspended && compPower.PowerOn && compPower != null)
                        {
                            Thing thing = ThingMaker.MakeThing(plantDefToGrow);
                            if (item.GetPlant(Map) == null)
                            {
                                GenPlace.TryPlaceThing(thing, item, Map, ThingPlaceMode.Direct, out Thing lastResultingThing);
                                yield return p;
                            }
                        }
                    }
                }
            }
        }

        private void CutBasinNow() 
        {
            if (compPower != null && compPower.PowerOn)
            {
                foreach (Plant item in PlantsOnMe)
                {
                    if (item != null)
                    {
                        int num2 = item.YieldNow();
                        if (num2 > 0)
                        {
                            Thing thing = ThingMaker.MakeThing(item.def.plant.harvestedThingDef);
                            thing.stackCount = num2;
                            thing.SetForbidden(value: false);
                            GenPlace.TryPlaceThing(thing, item.Position, Map, ThingPlaceMode.Near);
                            item.Destroy();
                        }
                        else
                        {
                            item.Destroy();
                        }
                    }
                }
            }
            else 
            {
                Messages.Message("AutoponicsCannotCutNoPower".Translate(), MessageTypeDefOf.RejectInput);
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            plantDefToGrow = def.building.defaultPlantToGrow;
            growingSuspended = false;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = GetComp<CompPowerTrader>();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
            Scribe_Values.Look(ref growingSuspended, "growingSuspended", false);
        }

        public override void TickRare()
        {
            if (compPower != null && !compPower.PowerOn)
            {
                foreach (Plant item in PlantsOnMe)
                {
                    if (item != null) {
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 1f);
                        item.TakeDamage(dinfo);
                    }

                }
            }
            else if(compPower != null && compPower.PowerOn)
            {
                foreach (Plant item in PlantsOnMe)
                {
                    if (item != null && item.Growth >= 1.0f)
                    {
                        //Log.Message("Should yield harvest!");
                        int num2 = item.YieldNow();
                        if (num2 > 0)
                        {
                            Thing thing = ThingMaker.MakeThing(item.def.plant.harvestedThingDef);
                            thing.stackCount = num2;
                            thing.SetForbidden(value: false);
                            GenPlace.TryPlaceThing(thing, item.Position, Map, ThingPlaceMode.Near);
                            item.Destroy();
                        }
                    }
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (Plant item in PlantsOnMe.ToList())
            {
                if(item != null) {
                    item.Destroy();
                }
            }
            base.DeSpawn(mode);
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (base.Spawned)
            {
                text = ((!PlantUtility.GrowthSeasonNow(base.Position, base.Map, forSowing: true)) ? (text + "\n" + "CannotGrowBadSeasonTemperature".Translate()) : (text + "\n" + "GrowSeasonHereNow".Translate()));
            }
            return text;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);


            yield return new Command_Toggle()
            {
                defaultLabel = "AutoponicsSuspendGrowing".Translate(),
                defaultDesc = "AutoponicsSuspendGrowingDesc".Translate(),
                isActive = () => growingSuspended,
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Suspend", true),
                Order = -100,
                toggleAction = delegate ()
                {
                    growingSuspended = !growingSuspended;
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "AutoponicsCutNow".Translate(),
                defaultDesc = "AutoponicsCutNowDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/CutNow", true),
                Order = -100,
                action = delegate ()
                {
                    CutBasinNow();
                }
            };
        }

        public ThingDef GetPlantDefToGrow()
        {
            return plantDefToGrow;
        }

        public void SetPlantDefToGrow(ThingDef plantDef)
        {
            plantDefToGrow = plantDef;
        }

        public bool CanAcceptSowNow()
        {
            if (compPower != null && !compPower.PowerOn)
            {
                return false;
            }
            return true;
        }
    }

}
