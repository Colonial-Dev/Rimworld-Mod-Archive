using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Spaceports.Letters
{
    internal class MysteryCargoLetter : ChoiceLetter
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
                DiaOption diaAccept = new DiaOption("Spaceports_MysteryCargoAccept".Translate());
                DiaOption diaDeny = new DiaOption("Spaceports_MysteryCargoDeny".Translate());
                diaAccept.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, null, 0);
                    TransportShip shuttle = Utils.GenerateInboundShuttle(null, pad, map, forcedType: SpaceportsDefOf.Spaceports_SurpriseShuttle, canLeave: false);
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
            Scribe_References.Look(ref map, "map");
            base.ExposeData();
        }
    }
}
