using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Spaceports
{
    public class SpaceportsSettings : ModSettings
    {
        public bool airspaceLockdown = true;
        public bool autoEvacuate = true;
        public bool enableShuttleLimit = false;
        public bool randomFlow = false;
        public int shuttleLimit = 5;

        public bool regularVisitors = true;
        public bool visitorNotifications = false;
        public float visitorFrequencyDays = 1.0f;
        public float visitorMaxTime = 1.0f;

        public bool regularTraders = false;
        public bool traderNotifications = false;
        public float traderFrequencyDays = 1.0f;
        public float traderMaxTime = 1.0f;

        public bool hospitalityEnabled = true;
        public float hospitalityChance = 0.5f;
        public bool dbhEnabled = true;

        public bool padAnimationsGlobal = true;
        public bool rimLightsAnimations = true;
        public bool landingAnimations = true;
        public bool beaconAnimationsGlobal = true;
        public bool beaconRadarAnimations = true;
        public bool beaconRimAnimations = true;

        public bool eventsEnabled = true;
        public bool KesslerSyndrome = true;
        public bool InterstellarDerelict = true;
        public bool MysteryCargo = true;
        public bool PrisonerTransfer = true;
        public bool ShuttleMedevac = true;
        public bool SpicyPawnLending = true;

        public int page = 1;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref airspaceLockdown, "airspaceLockdown", true);
            Scribe_Values.Look(ref autoEvacuate, "autoEvacuate", true);
            Scribe_Values.Look(ref enableShuttleLimit, "enableShuttleLimit", false);
            Scribe_Values.Look(ref randomFlow, "randomFlow", false);
            Scribe_Values.Look(ref shuttleLimit, "shuttleLimit", 5);

            Scribe_Values.Look(ref regularVisitors, "regularVisitors", true);
            Scribe_Values.Look(ref visitorNotifications, "visitorNotifications", false);
            Scribe_Values.Look(ref visitorFrequencyDays, "visitorFrequencyDays", 1.0f);
            Scribe_Values.Look(ref visitorMaxTime, "visitorMaxTime", 1.0f);

            Scribe_Values.Look(ref regularTraders, "regularTraders", false);
            Scribe_Values.Look(ref traderNotifications, "traderNotifications", false);
            Scribe_Values.Look(ref traderFrequencyDays, "traderFrequencyDays", 1.0f);
            Scribe_Values.Look(ref traderMaxTime, "traderMaxTime", 1.0f);

            Scribe_Values.Look(ref hospitalityEnabled, "hospitalityEnabled", true);
            Scribe_Values.Look(ref hospitalityChance, "hospitalityChance", 0.5f);
            Scribe_Values.Look(ref dbhEnabled, "dbhEnabled", true);

            Scribe_Values.Look(ref padAnimationsGlobal, "padAnimationsGlobal", true);
            Scribe_Values.Look(ref rimLightsAnimations, "rimLightsAnimations", true);
            Scribe_Values.Look(ref landingAnimations, "landingAnimations", true);
            Scribe_Values.Look(ref beaconAnimationsGlobal, "beaconAnimationsGlobal", true);
            Scribe_Values.Look(ref beaconRadarAnimations, "beaconRadarAnimations", true);
            Scribe_Values.Look(ref beaconRimAnimations, "beaconRimAnimations", true);

            Scribe_Values.Look(ref eventsEnabled, "eventsEnabled", true);
            Scribe_Values.Look(ref KesslerSyndrome, "KesslerSyndrome", true);
            Scribe_Values.Look(ref InterstellarDerelict, "InterstellarDerelict", true);
            Scribe_Values.Look(ref MysteryCargo, "MysteryCargo", true);
            Scribe_Values.Look(ref PrisonerTransfer, "PrisonerTransfer", true);
            Scribe_Values.Look(ref ShuttleMedevac, "ShuttleMedevac", true);
            Scribe_Values.Look(ref SpicyPawnLending, "SpicyPawnLending", true);


            base.ExposeData();
        }
    }

    public class SpaceportsMod : Mod
    {
        SpaceportsSettings settings;
        public SpaceportsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SpaceportsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            if (settings.page == 1)
            {
                if (listingStandard.ButtonText("Spaceports_NextPageA".Translate()))
                {
                    settings.page++;
                }
                listingStandard.GapLine();

                listingStandard.Label("Spaceports_TCHeader".Translate());
                listingStandard.CheckboxLabeled("Spaceports_AirspaceLockdownToggle".Translate(), ref settings.airspaceLockdown, "Spaceports_AirspaceLockdownTooltip".Translate());
                listingStandard.CheckboxLabeled("Spaceports_AutoLeaveToggle".Translate(), ref settings.autoEvacuate, "Spaceports_AutoLeaveTooltip".Translate());
                listingStandard.CheckboxLabeled("Spaceports_RandomFlow".Translate(), ref settings.randomFlow, "Spaceports_RandomFlowTooltip".Translate());
                listingStandard.CheckboxLabeled("Spaceports_EnableShuttleLimitToggle".Translate(), ref settings.enableShuttleLimit);
                if (settings.enableShuttleLimit)
                {
                    listingStandard.Label("Spaceports_ShuttleLimitLabel".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.shuttleLimit);
                    settings.shuttleLimit = (int)Math.Round(listingStandard.Slider(settings.shuttleLimit, 1f, 20f));
                }

                listingStandard.GapLine();

                listingStandard.Label("Spaceports_VisitorHeader".Translate());
                listingStandard.CheckboxLabeled("Spaceports_RegularVisitorsToggle".Translate(), ref settings.regularVisitors, "Spaceports_RegularVisitorsTooltip".Translate());
                if (settings.regularVisitors)
                {
                    listingStandard.CheckboxLabeled("Spaceports_VisitorNotificationsToggle".Translate(), ref settings.visitorNotifications, "Spaceports_VisitorNotificationsTooltip".Translate());
                    listingStandard.Label("Spaceports_VisitorFreqSlider".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.visitorFrequencyDays);
                    settings.visitorFrequencyDays = (float)Math.Round(listingStandard.Slider(settings.visitorFrequencyDays, 0.10f, 5f) * 10, MidpointRounding.ToEven) / 10;
                    listingStandard.Label("Spaceports_VisitorStaySlider".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.visitorMaxTime);
                    settings.visitorMaxTime = (float)Math.Round(listingStandard.Slider(settings.visitorMaxTime, 0.10f, 2f) * 10, MidpointRounding.ToEven) / 10;

                }

                listingStandard.GapLine();

                listingStandard.Label("Spaceports_TraderHeader".Translate());
                listingStandard.CheckboxLabeled("Spaceports_RegularTradersToggle".Translate(), ref settings.regularTraders, "Spaceports_RegularTradersTooltip".Translate());
                if (settings.regularTraders)
                {
                    listingStandard.CheckboxLabeled("Spaceports_TraderNotificationsToggle".Translate(), ref settings.traderNotifications, "Spaceports_TraderNotificationsTooltip".Translate());
                    listingStandard.Label("Spaceports_TraderFreqSlider".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.traderFrequencyDays);
                    settings.traderFrequencyDays = (float)Math.Round(listingStandard.Slider(settings.traderFrequencyDays, 0.10f, 5f) * 10, MidpointRounding.ToEven) / 10;
                    listingStandard.Label("Spaceports_TraderStaySlider".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.traderMaxTime);
                    settings.traderMaxTime = (float)Math.Round(listingStandard.Slider(settings.traderMaxTime, 0.10f, 2f) * 10, MidpointRounding.ToEven) / 10;

                }



            }

            else if(settings.page == 2)
            {
                if (listingStandard.ButtonText("Spaceports_NextPageB".Translate()))
                {
                    settings.page++;
                }
                listingStandard.GapLine();

                listingStandard.Label("Spaceports_AnimHeader".Translate());
                listingStandard.CheckboxLabeled("Spaceports_GlobalAnimationToggle".Translate(), ref settings.padAnimationsGlobal, "Spaceports_GlobalAnimationToggle_Label".Translate());
                if (settings.padAnimationsGlobal)
                {
                    listingStandard.CheckboxLabeled("Spaceports_RimLightsToggle".Translate(), ref settings.rimLightsAnimations, "Spaceports_RimLightsToggleTooltip".Translate());
                    listingStandard.CheckboxLabeled("Spaceports_LandingPatternToggle".Translate(), ref settings.landingAnimations, "Spaceports_LandingPatternToggleTooltip".Translate());
                }

                listingStandard.CheckboxLabeled("Spaceports_BeaconAnimationToggle".Translate(), ref settings.beaconAnimationsGlobal, "Spaceports_BeaconAnimationTooltip".Translate());
                if (settings.beaconAnimationsGlobal)
                {
                    listingStandard.CheckboxLabeled("Spaceports_BeaconDishToggle".Translate(), ref settings.beaconRadarAnimations, "Spaceports_BeaconDishTooltip".Translate());
                    listingStandard.CheckboxLabeled("Spaceports_BeaconRimLightsToggle".Translate(), ref settings.beaconRimAnimations, "Spaceports_BeaconRimLightsTooltip".Translate());
                }

                listingStandard.GapLine();

                listingStandard.Label("Spaceports_EventHeader".Translate());
                listingStandard.CheckboxLabeled("Spaceports_EventToggle".Translate(), ref settings.eventsEnabled, "Spaceports_EventToggleTooltip".Translate());
                if (settings.eventsEnabled)
                {
                    listingStandard.CheckboxLabeled("Spaceports_KesslerSyndromeToggle".Translate(), ref settings.KesslerSyndrome);
                    listingStandard.CheckboxLabeled("Spaceports_InterstellarDerelictToggle".Translate(), ref settings.InterstellarDerelict);
                    listingStandard.CheckboxLabeled("Spaceports_MysteryCargoToggle".Translate(), ref settings.MysteryCargo);
                    listingStandard.CheckboxLabeled("Spaceports_PrisonerTransferToggle".Translate(), ref settings.PrisonerTransfer);
                    listingStandard.CheckboxLabeled("Spaceports_ShuttleMedevacToggle".Translate(), ref settings.ShuttleMedevac);
                    listingStandard.CheckboxLabeled("Spaceports_SpicyPawnLendingToggle".Translate(), ref settings.SpicyPawnLending);
                }

                listingStandard.GapLine();
            }

            else if (settings.page == 3)
            {
                if (listingStandard.ButtonText("Spaceports_PrevPage".Translate()))
                {
                    settings.page = 1;
                }
                listingStandard.GapLine();

                if (Verse.ModLister.HasActiveModWithName("Hospitality"))
                {
                    listingStandard.Label("Spaceports_HospitalityHeader".Translate());
                    listingStandard.CheckboxLabeled("Spaceports_HospitalityModeToggle".Translate(), ref settings.hospitalityEnabled, "Spaceports_HospitalityModeToggleTooltip".Translate());
                    if (settings.hospitalityEnabled)
                    {
                        listingStandard.Label("Spaceports_HospitalityModeFreqSlider".Translate() + "Spaceports_VisitorFreqSlider_Count".Translate() + settings.hospitalityChance);
                        settings.hospitalityChance = (float)Math.Round(listingStandard.Slider(settings.hospitalityChance, 0.01f, 1f) * 10, MidpointRounding.ToEven) / 10;
                    }
                    listingStandard.GapLine();
                }
                if (Verse.ModLister.HasActiveModWithName("Dubs Bad Hygiene"))
                {
                    listingStandard.Label("Spaceports_DBHHeader".Translate());
                    listingStandard.CheckboxLabeled("Spaceports_DBHReflection".Translate(), ref settings.dbhEnabled, "Spaceports_DBHReflectionTooltip".Translate());
                    listingStandard.GapLine();
                }
                if (listingStandard.ButtonText("Spaceports_ResetToDefault".Translate()))
                {
                    ResetToDefaults();
                }
            }
            listingStandard.End();
        }

        public void ResetToDefaults()
        {
            settings.airspaceLockdown = true;
            settings.autoEvacuate = true;
            settings.enableShuttleLimit = false;
            settings.randomFlow = false;
            settings.shuttleLimit = 5;

            settings.regularVisitors = true;
            settings.visitorNotifications = false;
            settings.visitorFrequencyDays = 1.0f;
            settings.visitorMaxTime = 1.0f;

            settings.regularTraders = false;
            settings.traderNotifications = false;
            settings.traderFrequencyDays = 1.0f;
            settings.traderMaxTime = 1.0f;

            settings.hospitalityEnabled = true;
            settings.hospitalityChance = 0.5f;
            settings.dbhEnabled = true;

            settings.padAnimationsGlobal = true;
            settings.rimLightsAnimations = true;
            settings.landingAnimations = true;
            settings.beaconAnimationsGlobal = true;
            settings.beaconRadarAnimations = true;
            settings.beaconRimAnimations = true;

            settings.eventsEnabled = true;
            settings.KesslerSyndrome = true;
            settings.InterstellarDerelict = true;
            settings.MysteryCargo = true;
            settings.PrisonerTransfer = true;
            settings.ShuttleMedevac = true;
            settings.SpicyPawnLending = true;
        }

        public override string SettingsCategory()
        {
            return "SpaceportsModName".Translate();
        }

    }
}