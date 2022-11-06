﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Clonebay
{
    /// <summary>
    /// Stores relevant information about the genome of a pawn.
    /// </summary>
    public class GenomeSequence : ThingWithComps
    {
        //Relevant for all genomes.
        public string sourceName = null;
        public NameTriple pawnName = null;
        public PawnKindDef pawnKindDef;
        public Gender gender = Gender.None;

        //Only relevant for humanoids.
        public BodyTypeDef bodyType = null;
        public CrownType crownType = CrownType.Undefined;
        public List<ExposedTraitEntry> traits = new List<ExposedTraitEntry>();
        public Color hairColor = new Color();
        public float skinMelanin = 0f;

        //AlienRace compatibility.
        /// <summary>
        /// If true then Alien Race attributes should be shown.
        /// </summary>
        public bool isAlien = false;
        public Color skinColor = new Color();
        public Color skinColorSecond = new Color();
        public Color hairColorSecond = new Color();
        public string crownTypeAlien = "";

        public override void ExposeData()
        {
            base.ExposeData();

            //Basic.
            Scribe_Values.Look(ref sourceName, "sourceName");
            Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
            Scribe_Values.Look(ref gender, "gender");

            //Humanoid only.
            Scribe_Defs.Look(ref bodyType, "bodyType");
            Scribe_Values.Look(ref crownType, "crownType");
            Scribe_Values.Look(ref hairColor, "hairColor");
            Scribe_Values.Look(ref skinMelanin, "skinMelanin");
            Scribe_Collections.Look(ref traits, "traits", LookMode.Deep);

            //Alien Compat.
            Scribe_Values.Look(ref isAlien, "isAlien");
            Scribe_Values.Look(ref skinColor, "skinColor");
            Scribe_Values.Look(ref skinColorSecond, "skinColorSecond");
            Scribe_Values.Look(ref hairColorSecond, "hairColorSecond");
            Scribe_Values.Look(ref crownTypeAlien, "crownTypeAlien");
        }

        public override bool CanStackWith(Thing other)
        {
            if(other is GenomeSequence otherGenome &&
                sourceName == otherGenome.sourceName &&
                pawnKindDef == otherGenome.pawnKindDef &&
                gender == otherGenome.gender &&
                bodyType == otherGenome.bodyType &&
                crownType == otherGenome.crownType &&
                hairColor == otherGenome.hairColor &&
                skinMelanin == otherGenome.skinMelanin &&
                isAlien == otherGenome.isAlien &&
                skinColor == otherGenome.skinColor &&
                skinColorSecond == otherGenome.skinColorSecond &&
                hairColorSecond == otherGenome.hairColorSecond &&
                crownTypeAlien == otherGenome.crownTypeAlien &&
                traits.SequenceEqual(otherGenome.traits))
            {
                return base.CanStackWith(other);
            }

            return false;
        }

        /*public bool TraitsAreEqual(IEnumerable<ExposedTraitEntry> otherTraits)
        {
            foreach(var entry in otherTraits)
            {

            }

            return true;
        }*/

        public override Thing SplitOff(int count)
        {
            Thing splitThing = base.SplitOff(count);
            if(splitThing != this && splitThing is GenomeSequence splitThingStack)
            {
                //Basic.
                splitThingStack.sourceName = sourceName;
                splitThingStack.pawnKindDef = pawnKindDef;
                splitThingStack.gender = gender;

                //Humanoid only.
                splitThingStack.bodyType = bodyType;
                splitThingStack.crownType = crownType;
                splitThingStack.hairColor = hairColor;
                splitThingStack.skinMelanin = skinMelanin;
                foreach (ExposedTraitEntry traitEntry in traits)
                {
                    splitThingStack.traits.Add(new ExposedTraitEntry(traitEntry));
                }

                //Alien Compat.
                splitThingStack.isAlien = isAlien;
                splitThingStack.skinColor = skinColor;
                splitThingStack.skinColorSecond = skinColorSecond;
                splitThingStack.hairColorSecond = hairColorSecond;
                splitThingStack.crownTypeAlien = crownTypeAlien;
            }

            return splitThing;
        }

        public override string LabelNoCount
        {
            get
            {
                if (GetComp<CustomNameComp>() is CustomNameComp nameComp && nameComp.customName.NullOrEmpty())
                {
                    if (sourceName != null)
                    {
                        return sourceName + " " + base.LabelNoCount;
                    }
                    else
                    {
                        return base.LabelNoCount;
                    }
                }

                return base.LabelNoCount;
            }
        }

        public override string DescriptionDetailed
        {
            get
            {
                StringBuilder builder = new StringBuilder(base.DescriptionDetailed);

                builder.AppendLine();
                if(sourceName != null)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Name".Translate() + ": " + sourceName);
                }
                builder.AppendLine("QE_GenomeSequencerDescription_Race".Translate() + ": " + pawnKindDef.race.LabelCap);
                builder.AppendLine("QE_GenomeSequencerDescription_Gender".Translate() + ": " + GenderUtility.GetLabel(gender, pawnKindDef.race.race.Animal).CapitalizeFirst());

                //Traits
                if(traits.Count > 0)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Traits".Translate());
                    foreach (ExposedTraitEntry traitEntry in traits)
                    {
                        builder.AppendLine("    " + traitEntry.def.DataAtDegree(traitEntry.degree).label);
                    }
                }

                return builder.ToString().TrimEndNewlines();
            }
        }

        public override string DescriptionFlavor
        {
            get
            {
                StringBuilder builder = new StringBuilder(base.DescriptionFlavor);

                builder.AppendLine();
                builder.AppendLine();
                if (sourceName != null)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Name".Translate() + ": " + sourceName);
                }
                builder.AppendLine("QE_GenomeSequencerDescription_Race".Translate() + ": " + pawnKindDef.race.LabelCap);
                builder.AppendLine("QE_GenomeSequencerDescription_Gender".Translate() + ": " + GenderUtility.GetLabel(gender, pawnKindDef.race.race.Animal).CapitalizeFirst());

                //Traits
                if (traits.Count > 0)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Traits".Translate());
                    foreach (ExposedTraitEntry traitEntry in traits)
                    {
                        builder.AppendLine("    " + traitEntry.def.DataAtDegree(traitEntry.degree).label.CapitalizeFirst());
                    }
                }

                return builder.ToString().TrimEndNewlines();
            }
        }
    }
}
