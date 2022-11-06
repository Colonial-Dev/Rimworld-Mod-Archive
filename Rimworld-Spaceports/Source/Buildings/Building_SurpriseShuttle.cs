using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports.Buildings
{
    public class Building_SurpriseShuttle : Building
    {
        //0 - minor positive reward
        //1 - major positive reward
        //2 - nothing
        //3 - departure
        //4 - skeletons
        //5 - psychic scream
        //6 - attack party
        //7 - disguised bomb
        private int SurpriseType;
        private bool SurpriseFired = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SurpriseType, "SurpriseType", 0);
            Scribe_Values.Look(ref SurpriseFired, "SurpriseFired", false);
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                SurpriseType = Rand.RangeInclusive(0, 7);
            }
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override string GetInspectString()
        {
            return "Spaceports_MysteryCargoContents".Translate();
        }

        public void DoSurprise()
        {
            SurpriseFired = true;
            Thing t = this as Thing;
            t.def.building.alwaysDeconstructible = true;

            switch (SurpriseType)
            {
                case 0:
                    {
                        Messages.Message("Spaceports_MysteryCargo0".Translate(), MessageTypeDefOf.PositiveEvent);
                        List<Thing> items = new List<Thing>();
                        Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                        Thing gold = ThingMaker.MakeThing(ThingDefOf.Gold);
                        silver.stackCount = Rand.RangeInclusive(500, 1000);
                        gold.stackCount = Rand.RangeInclusive(100, 500);
                        ThingSetMakerParams parameters = default(ThingSetMakerParams);
                        parameters.qualityGenerator = QualityGenerator.Reward;
                        parameters.totalMarketValueRange = new FloatRange(1000f, 3000f);
                        items.AddRange(ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parameters));
                        items.Add(silver);
                        items.Add(gold);
                        this.GetComp<CompShuttle>().Transporter.innerContainer.TryAddRangeOrTransfer(items);
                        ShipJob_Unload unload = new ShipJob_Unload();
                        unload.loadID = Find.UniqueIDsManager.GetNextShipJobID();
                        this.GetComp<CompShuttle>().shipParent.ForceJob_DelayCurrent(unload);
                        break;
                    }

                case 1:
                    {
                        Messages.Message("Spaceports_MysteryCargo1".Translate(), MessageTypeDefOf.PositiveEvent);
                        List<Thing> items = new List<Thing>();
                        Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                        Thing gold = ThingMaker.MakeThing(ThingDefOf.Gold);
                        silver.stackCount = Rand.RangeInclusive(1000, 2000);
                        gold.stackCount = Rand.RangeInclusive(500, 1000);
                        ThingSetMakerParams parameters = default(ThingSetMakerParams);
                        parameters.qualityGenerator = QualityGenerator.Reward;
                        items.AddRange(ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate(parameters));
                        items.Add(silver);
                        items.Add(gold);
                        this.GetComp<CompShuttle>().Transporter.innerContainer.TryAddRangeOrTransfer(items);
                        ShipJob_Unload unload = new ShipJob_Unload();
                        unload.loadID = Find.UniqueIDsManager.GetNextShipJobID();
                        this.GetComp<CompShuttle>().shipParent.ForceJob_DelayCurrent(unload);
                        break;
                    }

                case 2:
                    Messages.Message("Spaceports_MysteryCargo2".Translate(), MessageTypeDefOf.NeutralEvent);
                    break;

                case 3:
                    Messages.Message("Spaceports_MysteryCargo3".Translate(), MessageTypeDefOf.NeutralEvent);
                    CompShuttle shuttleComp = this.GetComp<CompShuttle>();
                    ShipJob_FlyAway leave = new ShipJob_FlyAway();
                    leave.loadID = Find.UniqueIDsManager.GetNextShipJobID();
                    shuttleComp.shipParent.ForceJob(leave);
                    break;

                case 4:
                    Messages.Message("Spaceports_MysteryCargo4".Translate(), MessageTypeDefOf.NeutralEvent);
                    int skeletons = Rand.RangeInclusive(3, 11);
                    for (int i = 0; i < skeletons; i++)
                    {
                        SpawnCorpse(PawnKindDefOf.AncientSoldier, this.InteractionCell, new IntRange(180000000, 720000000).RandomInRange, this.Map);
                    }
                    break;

                case 5:
                    Messages.Message("Spaceports_MysteryCargo5".Translate(), MessageTypeDefOf.NegativeEvent);
                    foreach (Pawn item in this.Map.mapPawns.AllPawnsSpawned)
                    {
                        item.needs?.mood?.thoughts.memories.TryGainMemory(SpaceportsDefOf.Spaceports_PsychicCharge);
                    }
                    break;

                case 6:
                    Faction enemyFaction = Find.FactionManager.RandomEnemyFaction(minTechLevel: TechLevel.Industrial, allowHidden: true);
                    Messages.Message("Spaceports_MysteryCargo6".Translate(enemyFaction.Name), MessageTypeDefOf.NegativeEvent);
                    List<Pawn> enemies = new List<Pawn>();
                    int enemyCt = Rand.RangeInclusive(5, 10);
                    for (int i = 0; i < enemyCt; i++)
                    {
                        enemies.Add(PawnGenerator.GeneratePawn(enemyFaction.RandomPawnKind(), enemyFaction));
                    }
                    LordJob lordJob = new LordJob_AssaultColony(enemyFaction);
                    LordMaker.MakeNewLord(enemyFaction, lordJob, this.Map, enemies);
                    foreach (Pawn p in enemies)
                    {
                        GenSpawn.Spawn(p, this.InteractionCell, this.Map);
                    }
                    break;

                case 7:
                    Messages.Message("Spaceports_MysteryCargo7".Translate(), MessageTypeDefOf.NegativeEvent);
                    this.GetComp<CompExplosive>().StartWick();
                    break;

                default:
                    Log.Error("[Spaceports] Invalid surprise type!");
                    break;
            }
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 50 == 0 && this.Spawned && !SurpriseFired)
            {
                CompTickRare();
            }
            base.Tick();
        }

        public void CompTickRare()
        {
            Predicate<Thing> predicate = null;
            predicate = (Thing t) => (t as Pawn)?.RaceProps.Humanlike ?? false;
            Thing t = null;
            t = GenClosest.ClosestThingReachable(this.InteractionCell, this.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), 7f, predicate);
            if (t != null)
            {
                DoSurprise();
            }
        }

        private void SpawnCorpse(PawnKindDef pawnKindDef, IntVec3 spawnPosition, int age, Map map)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, -1));
            if (!pawn.Dead)
            {
                pawn.Kill(null, null);
            }
            if (pawn.inventory != null)
            {
                pawn.inventory.DestroyAll();
            }
            if (pawn.apparel != null)
            {
                pawn.apparel.DestroyAll();
            }
            if (pawn.equipment != null)
            {
                pawn.equipment.DestroyAllEquipment();
            }
            pawn.Corpse.Age = age + Rand.Range(0, 900000);
            pawn.relations.hidePawnRelations = true;
            GenSpawn.Spawn(pawn.Corpse, spawnPosition, map);
            pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
        }

        private Faction RandomEnemy(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
        {
            if ((from x in Find.FactionManager.GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
                 where x.HostileTo(Faction.OfPlayer)
                 where x.def.pawnGroupMakers != null
                 select x).TryRandomElement(out var result))
            {
                return result;
            }
            return null;
        }

    }
}
