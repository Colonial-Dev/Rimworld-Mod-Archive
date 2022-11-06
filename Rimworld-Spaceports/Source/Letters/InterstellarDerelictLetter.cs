using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Spaceports.Letters
{
    internal class InterstellarDerelictLetter : ChoiceLetter
    {
        public Map map;
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }
                DiaOption diaAccept = new DiaOption("Spaceports_InterstellarDerelictAccept".Translate());
                DiaOption diaDeny = new DiaOption("Spaceports_InterstellarDerelictDeny".Translate());
                diaAccept.action = delegate
                {
                    if (Rand.RangeInclusive(0, 100) <= 50)
                    {
                        IntVec3 pad = Utils.FindValidSpaceportPad(map, null, 0);
                        List<Pawn> spacers = new List<Pawn>();
                        GeneratePawns(spacers);
                        ThingSetMakerParams parameters = default(ThingSetMakerParams);
                        parameters.qualityGenerator = QualityGenerator.Reward;
                        List<Thing> things = (ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate(parameters));

                        TransportShip shuttle = Utils.GenerateInboundShuttle(spacers, pad, map, items: things, forcedType: SpaceportsDefOf.Spaceports_ShuttleInert, canLeave: false);

                        Messages.Message("Spaceports_InterstellarDerelictSafe".Translate(), MessageTypeDefOf.PositiveEvent, false);
                    }
                    else
                    {
                        if (Rand.RangeInclusive(0, 100) <= 50)
                        {
                            IntVec3 pad = Utils.FindValidSpaceportPad(map, null, 0);
                            GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(SpaceportsDefOf.ShuttleA_Crashing, ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel)), pad, map, ThingPlaceMode.Near);
                            Messages.Message("Spaceports_InterstellarDerelictBoom".Translate(), MessageTypeDefOf.NegativeEvent, false);
                        }
                        else
                        {
                            int chunks = Rand.RangeInclusive(50, 100);
                            for (int i = 0; i < chunks; i++)
                            {
                                IntVec3 cell = DropCellFinder.RandomDropSpot(map);
                                GenPlace.TryPlaceThing(SkyfallerMaker.MakeSkyfaller(SpaceportsDefOf.Spaceports_Shrapnel, ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel)), cell, map, ThingPlaceMode.Near);
                                Messages.Message("Spaceports_InterstellarDerelictShrapnel".Translate(chunks), MessageTypeDefOf.NegativeEvent, false);
                            }
                        }
                    }
                    Find.LetterStack.RemoveLetter(this);
                };
                diaAccept.resolveTree = true;
                diaAccept.disabledReason = "Spaceports_ShuttleDisabled".Translate();
                if (!Utils.AnyValidSpaceportPad(map, 0))
                {
                    diaAccept.disabled = true;
                }
                diaDeny.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                };
                diaDeny.resolveTree = true;
                yield return diaAccept;
                yield return diaDeny;
                yield return base.Option_Postpone;
            }
        }

        private void GeneratePawns(List<Pawn> pawns)
        {
            int numPawns = Rand.RangeInclusive(2, 6);
            for (int i = 0; i <= numPawns; i++)
            {
                Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, Find.FactionManager.OfAncients, PawnGenerationContext.NonPlayer, -1));
                pawns.Add(newPawn);
            }
        }
    }
}
