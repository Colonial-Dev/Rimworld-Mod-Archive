using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Spaceports.Letters
{
    public class LoanedPawnTracker : Utils.Tracker
    {
        private Pawn LoanedPawn;
        private Map map;
        private int TicksPassed = 0;
        private int ReturnAfterTicks;
        private List<Thing> rewards;
        private bool WasInjured;

        public LoanedPawnTracker()
        {

        }

        public LoanedPawnTracker(Pawn pawn, Map map, List<Thing> rewards, bool WasInjured)
        {
            LoanedPawn = pawn;
            this.map = map;
            this.rewards = rewards;
            this.WasInjured = WasInjured;
            ReturnAfterTicks = Rand.RangeInclusive(GenDate.TicksPerDay * 10, GenDate.TicksPerDay * 15);
        }

        public override bool Check()
        {
            if (TicksPassed < ReturnAfterTicks)
            {
                TicksPassed++;
                return false;
            }
            else if (TicksPassed >= ReturnAfterTicks && Utils.AnyValidSpaceportPad(map, 0))
            {
                if (WasInjured)
                {
                    try
                    {
                        List<Apparel> cache = new List<Apparel>();
                        for (int i = 0; i < LoanedPawn.apparel.WornApparel.Count; i++) //IMPORTANT - stops NREs when pawn has certain apparel like shield belts
                        {
                            cache.Add((Apparel)LoanedPawn.apparel.WornApparel[i].SplitOff(1));

                        }
                        LoanedPawn.apparel.DestroyAll();
                        HealthUtility.DamageUntilDowned(LoanedPawn, allowBleedingWounds: false);
                        Messages.Message("Spaceports_SpicyPawnLendingInjured".Translate(LoanedPawn.NameShortColored, TryApplyExtraInjures()), MessageTypeDefOf.NegativeHealthEvent);
                        foreach (Apparel ap in cache)
                        {
                            LoanedPawn.apparel.Wear(ap);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[Spaceports] Caught exception " + e + " when attempting to injure pawn!");
                    }

                }
                else
                {
                    Messages.Message("Spaceports_SpicyPawnLendingReturnSafe".Translate(LoanedPawn.NameShortColored), MessageTypeDefOf.PositiveEvent);
                }
                IntVec3 pad = Utils.FindValidSpaceportPad(map, null, 0);
                List<Pawn> pawn = new List<Pawn>();
                pawn.Add(LoanedPawn);
                Utils.GenerateInboundShuttle(pawn, pad, map, dropAndGo: true, items: rewards);
                return true;
            }
            return false;
        }

        private string TryApplyExtraInjures()
        {
            string AddlInjuries = "";
            bool AnyAddlInjuries = false;
            if (Rand.Chance(0.5f) && ModsConfig.RoyaltyActive)
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Abasia, LoanedPawn);
                LoanedPawn.health.AddHediff(hediff);
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (Rand.Chance(0.5f))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Blindness, LoanedPawn);
                LoanedPawn.health.AddHediff(hediff, LoanedPawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.SightSource).RandomElement());
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (Rand.Chance(0.5f))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Carcinoma, LoanedPawn);
                LoanedPawn.health.AddHediff(hediff, LoanedPawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.BreathingSource).RandomElement());
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (Rand.Chance(0.5f))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, LoanedPawn);
                LoanedPawn.health.AddHediff(hediff, LoanedPawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbCore).RandomElement());
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (Rand.Chance(0.5f))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, LoanedPawn);
                LoanedPawn.health.AddHediff(hediff, LoanedPawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.MovingLimbCore).RandomElement());
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (Rand.Chance(0.5f))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Plague, LoanedPawn);
                hediff.Severity = 0.1f;
                LoanedPawn.health.AddHediff(hediff);
                AddlInjuries += hediff.Label + ", ";
                AnyAddlInjuries = true;
            }
            if (!AnyAddlInjuries)
            {
                AddlInjuries += "Spaceports_SpicyPawnLendingNoAddlInjury".Translate();
            }
            else
            {
                AddlInjuries = "Spaceports_SpicyPawnLendingAddlInjury".Translate() + AddlInjuries;
                AddlInjuries = AddlInjuries.TrimEnd(' ');
                AddlInjuries = AddlInjuries.TrimEnd(',');
            }
            return AddlInjuries;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref LoanedPawn, "LoanedPawn");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref TicksPassed, "TicksPassed");
            Scribe_Values.Look(ref ReturnAfterTicks, "ReturnAfterTicks");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Reference);
        }
    }

    internal class SpicyPawnLendingLetter : ChoiceLetter
    {
        public Faction faction;
        public Map map;
        public Pawn RequestedPawn;
        public List<Thing> rewards;
        public bool WasInjured;
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }
                DiaOption diaAccept = new DiaOption("Spaceports_SpicyPawnLendingAccept".Translate());
                DiaOption diaDeny = new DiaOption("Spaceports_SpicyPawnLendingDeny".Translate());
                diaAccept.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 0);
                    TransportShip shuttle = Utils.GenerateInboundShuttle(null, pad, map);
                    List<Pawn> pawn = new List<Pawn>();
                    pawn.Add(RequestedPawn);
                    shuttle.ShuttleComp.requiredPawns = pawn;
                    Messages.Message("Spaceports_PickupInbound".Translate(RequestedPawn.Name + ""), new LookTargets(shuttle.shipThing), MessageTypeDefOf.NeutralEvent);
                    map.GetComponent<SpaceportsMapComp>().LoadTracker(new LoanedPawnTracker(RequestedPawn, map, rewards, WasInjured));
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

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref RequestedPawn, "RequestedPawn");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
            base.ExposeData();
        }
    }
}
