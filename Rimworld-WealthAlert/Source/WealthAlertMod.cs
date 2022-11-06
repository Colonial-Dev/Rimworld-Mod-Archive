using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WealthAlert
{
    public class WealthThreshold : IExposable
    {
        public int threshold = 0;
        public string EditBuffer = "";

        public void ExposeData()
        {
            Scribe_Values.Look(ref threshold, "threshold", 0);
            Scribe_Values.Look(ref EditBuffer, "EditBuffer", "");
        }
    }

    public class WealthAlertSettings : ModSettings
    {
        public List<WealthThreshold> thresholds = DefaultThresholds();
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref thresholds, "thresholds", LookMode.Deep);
            base.ExposeData();
        }

        public static List<WealthThreshold> DefaultThresholds()
        {
            List<WealthThreshold> wts = new List<WealthThreshold>();
            WealthThreshold WT1 = new WealthThreshold();
            WT1.threshold = 15000;
            WT1.EditBuffer = "15000";
            wts.Add(WT1);
            WealthThreshold WT2 = new WealthThreshold();
            WT2.threshold = 31000;
            WT2.EditBuffer = "31000";
            wts.Add(WT2);
            WealthThreshold WT3 = new WealthThreshold();
            WT3.threshold = 81000;
            WT3.EditBuffer = "81000";
            wts.Add(WT3);
            WealthThreshold WT4 = new WealthThreshold();
            WT4.threshold = 182000;
            WT4.EditBuffer = "182000";
            wts.Add(WT4);
            WealthThreshold WT5 = new WealthThreshold();
            WT5.threshold = 308000;
            WT5.EditBuffer = "308000";
            wts.Add(WT5);
            return wts;
        }
    }

    public class WealthAlertMod : Mod
    {
        WealthAlertSettings settings;
        public WealthAlertMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<WealthAlertSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            if(settings.thresholds == null) { settings.thresholds = DefaultThresholds(); }

            if (listingStandard.ButtonText("WealthAlertAddWT".Translate()))
            {
                if (settings.thresholds.Count >= 18)
                {
                    Messages.Message("WealthAlertFull".Translate(), MessageTypeDefOf.RejectInput);
                }
                else 
                {
                    settings.thresholds.Add(new WealthThreshold());
                }
            }
            if (listingStandard.ButtonText("WealthAlertRemWT".Translate()) && settings.thresholds.Count > 0)
            {
                settings.thresholds.RemoveLast();
            }
            if (listingStandard.ButtonText("WealthAlertDefault".Translate()))
            {
                settings.thresholds = DefaultThresholds();
            }

            listingStandard.GapLine();

            foreach(WealthThreshold wt in settings.thresholds)
            {
                listingStandard.IntEntry(ref wt.threshold, ref wt.EditBuffer, multiplier: 1000);
            }
            listingStandard.End();
        }

        public override string SettingsCategory()
        {
            return "WealthAlertName".Translate();
        }

        public List<WealthThreshold> DefaultThresholds() 
        {
            List<WealthThreshold> wts = new List<WealthThreshold>();
            WealthThreshold WT1 = new WealthThreshold();
            WT1.threshold = 15000;
            WT1.EditBuffer = "15000";
            wts.Add(WT1);
            WealthThreshold WT2 = new WealthThreshold();
            WT2.threshold = 31000;
            WT2.EditBuffer = "31000";
            wts.Add(WT2);
            WealthThreshold WT3 = new WealthThreshold();
            WT3.threshold = 81000;
            WT3.EditBuffer = "81000";
            wts.Add(WT3);
            WealthThreshold WT4 = new WealthThreshold();
            WT4.threshold = 182000;
            WT4.EditBuffer = "182000";
            wts.Add(WT4);
            WealthThreshold WT5 = new WealthThreshold();
            WT5.threshold = 308000;
            WT5.EditBuffer = "308000";
            wts.Add(WT5);
            return wts;
        }

    }
}
