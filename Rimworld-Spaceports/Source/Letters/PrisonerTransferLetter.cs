using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Spaceports.Letters
{
    public class PrisonerTransferLetter : ChoiceLetter
    {
        public Faction faction;
        public Map map;
        public List<Thing> LowSecReward = new List<Thing>();
        public List<Thing> HighSecReward = new List<Thing>();

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }

                DiaOption diaTakeLowSec = new DiaOption("Spaceports_PrisonerTransferLowSec".Translate());
                DiaOption diaTakeHighSec = new DiaOption("Spaceports_PrisonerTransferHighSec".Translate());
                DiaOption diaDeny = new DiaOption("Spaceports_PrisonerTransferDeny".Translate());

                diaTakeLowSec.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 0);
                    Utils.GenerateInboundShuttle(GenerateLowSecPawns(RandomEnemy()), pad, map, items: LowSecReward, dropAndGo: true);
                    Find.LetterStack.RemoveLetter(this);
                };
                diaTakeLowSec.resolveTree = true;
                diaTakeLowSec.disabledReason = "Spaceports_ShuttleDisabled".Translate();

                diaTakeHighSec.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 0);
                    Utils.GenerateInboundShuttle(GenerateHighSecPawns(RandomEnemy()), pad, map, items: HighSecReward, dropAndGo: true);
                    Find.LetterStack.RemoveLetter(this);
                };
                diaTakeHighSec.resolveTree = true;
                diaTakeHighSec.disabledReason = "Spaceports_ShuttleDisabled".Translate();

                if (!Utils.AnyValidSpaceportPad(map, 0))
                {
                    diaTakeLowSec.disabled = true;
                    diaTakeHighSec.disabled = true;
                }

                diaDeny.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                };
                diaDeny.resolveTree = true;

                yield return diaTakeLowSec;
                yield return diaTakeHighSec;
                yield return diaDeny;
                yield return base.Option_Postpone;
            }
        }

        private Faction RandomEnemy(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
        {
            if ((from x in Find.FactionManager.GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
                 where x.HostileTo(Faction.OfPlayer)
                 where x.HostileTo(faction)
                 where x.def.pawnGroupMakers != null
                 select x).TryRandomElement(out var result))
            {
                return result;
            }
            return null;
        }

        private List<Pawn> GenerateLowSecPawns(Faction prisonerFaction)
        {
            int prisoners = Rand.RangeInclusive(1, 5);
            List<Pawn> pawns = new List<Pawn>();
            for (int i = 0; i < prisoners; i++)
            {
                Pawn p = PawnGenerator.GeneratePawn(prisonerFaction.RandomPawnKind(), prisonerFaction);
                HealthUtility.TryAnesthetize(p);
                Utils.StripPawn(p);
                pawns.Add(p);
            }
            return pawns;
        }

        private List<Pawn> GenerateHighSecPawns(Faction prisonerFaction)
        {
            int prisoners = Rand.RangeInclusive(1, 5);
            List<Pawn> pawns = new List<Pawn>();
            for (int i = 0; i < prisoners; i++)
            {
                Pawn p = PawnGenerator.GeneratePawn(prisonerFaction.RandomPawnKind(), prisonerFaction);
                BadifyPawn(p);
                HealthUtility.TryAnesthetize(p);
                Utils.StripPawn(p);
                pawns.Add(p);
            }
            return pawns;
        }

        private void BadifyPawn(Pawn p)
        {
            List<SkillRecord> skills = p.skills.skills;
            int type = Rand.RangeInclusive(1, 2);
            for (int i = 0; i < p.story.traits.allTraits.Count; i++)
            {
                p.story.traits.allTraits.Remove(p.story.traits.allTraits[i]);
            }
            foreach (SkillRecord s in skills)
            {
                s.Level = 0;
                s.passion = Passion.None;
            }
            p.story.traits.GainTrait(new Trait(TraitDefOf.Tough));
            p.story.traits.GainTrait(new Trait(TraitDefOf.Abrasive));
            p.story.traits.GainTrait(new Trait(TraitDefOf.Psychopath));
            if (type == 1)
            {
                p.story.traits.GainTrait(new Trait(TraitDefOf.Brawler));
                p.skills.GetSkill(SkillDefOf.Melee).Level = Rand.RangeInclusive(10, 15);
            }
            else
            {
                p.story.traits.GainTrait(new Trait(TraitDefOf.ShootingAccuracy, -1));
                p.skills.GetSkill(SkillDefOf.Shooting).Level = Rand.RangeInclusive(10, 15);
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_References.Look(ref map, "map");
            Scribe_Collections.Look(ref LowSecReward, "LowSecReward", LookMode.Deep);
            Scribe_Collections.Look(ref HighSecReward, "HighSecReward", LookMode.Deep);
            base.ExposeData();
        }
    }
}
