using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WealthAlert
{
    internal class WealthAlertMapComp : MapComponent
    {
        private int MostRecentThreshold;
        public WealthAlertMapComp(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MostRecentThreshold, "MostRecentThreshold", 0);
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            foreach(WealthThreshold wt in LoadedModManager.GetMod<WealthAlertMod>().GetSettings<WealthAlertSettings>().thresholds)
            {
                if(this.map.wealthWatcher.WealthTotal >= wt.threshold && MostRecentThreshold != wt.threshold && !(wt.threshold < MostRecentThreshold))
                {
                    Find.LetterStack.ReceiveLetter("WealthAlertTrippedLabel".Translate(), "WealthAlertTripped".Translate(this.map.info.parent.Label, wt.threshold, this.map.wealthWatcher.WealthTotal), LetterDefOf.NeutralEvent);
                    MostRecentThreshold = wt.threshold;
                }
            }

            if(this.map.wealthWatcher.WealthTotal < MostRecentThreshold)
            {
                int LowestNewThreshold = 0;
                foreach (WealthThreshold wt in LoadedModManager.GetMod<WealthAlertMod>().GetSettings<WealthAlertSettings>().thresholds)
                {
                    if(wt.threshold < this.map.wealthWatcher.WealthTotal && wt.threshold > LowestNewThreshold)
                    {
                        LowestNewThreshold = wt.threshold;
                    }
                }
                Find.LetterStack.ReceiveLetter("WealthAlertDroppedLabel".Translate(), "WealthAlertDropped".Translate(MostRecentThreshold, LowestNewThreshold, this.map.wealthWatcher.WealthTotal), LetterDefOf.NeutralEvent);
                MostRecentThreshold = LowestNewThreshold;
            }
        }

	}
}
