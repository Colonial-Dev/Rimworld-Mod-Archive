using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Spaceports
{
    public class SpaceportsMapComp : MapComponent
    {
        private IncidentQueue incidentQueueVisitors = new IncidentQueue();
        private IncidentQueue incidentQueueTraders = new IncidentQueue();
        private IncidentDef visitorIncident = SpaceportsDefOf.Spaceports_VisitorShuttleArrival;
        private IncidentDef traderIncident = SpaceportsDefOf.Spaceports_TraderShuttleArrival;
        private float visitorInterval = 0f;
        private float traderInterval = 0f;
        private int nextQueueInspection;
        public List<Utils.Tracker> trackers = new List<Utils.Tracker>();
        public bool ForcedLockdown = false;

        public SpaceportsMapComp(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref incidentQueueVisitors, "incidentQueueVisitors");
            Scribe_Deep.Look(ref incidentQueueTraders, "incidentQueueTraders");
            Scribe_Values.Look(ref nextQueueInspection, "nextQueueInspection", 0);
            Scribe_Values.Look(ref visitorInterval, "visitorInterval", 0f);
            Scribe_Values.Look(ref traderInterval, "traderInterval", 0f);
            Scribe_Collections.Look(ref trackers, "trackers", LookMode.Deep);
            Scribe_Values.Look(ref ForcedLockdown, "ForcedLockdown", false);
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (trackers == null)
            {
                trackers = new List<Utils.Tracker>();
            }

            List<Utils.Tracker> deadTrackers = new List<Utils.Tracker>();
            foreach (Utils.Tracker tracker in trackers)
            {
                if(tracker == null)
                {
                    deadTrackers.Add(tracker);
                    continue;
                }
                else if (tracker.Check())
                {
                    deadTrackers.Add(tracker);
                }
            }

            foreach (Utils.Tracker tracker in deadTrackers)
            {
                trackers.Remove(tracker);
            }

            if (incidentQueueVisitors == null)
            {
                incidentQueueVisitors = new IncidentQueue();
            }

            if (incidentQueueTraders == null)
            {
                incidentQueueTraders = new IncidentQueue();
            }

            CheckIfQueueDisabled();
            CheckIfVisitorQueueTimeChanged();
            CheckIfTraderQueueTimeChanged();

            incidentQueueVisitors.IncidentQueueTick();
            incidentQueueTraders.IncidentQueueTick();

            if (GenTicks.TicksGame > nextQueueInspection && Utils.CheckIfSpaceport(this.map))
            {
                nextQueueInspection = (int)(GenTicks.TicksGame + GenDate.TicksPerDay * 0.1f);
                if (incidentQueueVisitors.Count <= 1 && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularVisitors)
                {
                    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(visitorIncident.category, this.map);
                    QueueIncident(new FiringIncident(visitorIncident, null, incidentParms), LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays, incidentQueueVisitors);
                }
                if (incidentQueueTraders.Count <= 1 && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularTraders)
                {
                    IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(traderIncident.category, this.map);
                    QueueIncident(new FiringIncident(traderIncident, null, incidentParms), LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderFrequencyDays, incidentQueueTraders);
                }
            }
        }

        public void QueueIncident(FiringIncident incident, float afterDays, IncidentQueue queue)
        {
            var qi = new QueuedIncident(incident, (int)(Find.TickManager.TicksGame + GenDate.TicksPerDay * afterDays));
            queue.Add(qi);
        }

        public void CheckIfQueueDisabled()
        {
            if (incidentQueueVisitors.Count > 0 && !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularVisitors)
            {
                incidentQueueVisitors.Clear();
            }
            if (incidentQueueTraders.Count > 0 && !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().regularTraders)
            {
                incidentQueueTraders.Clear();
            }
        }

        public void CheckIfVisitorQueueTimeChanged()
        {
            if (visitorInterval == 0f)
            {
                visitorInterval = LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays;
                return;
            }
            else if (visitorInterval == LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays)
            {
                return;
            }
            else if (visitorInterval != LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays)
            {
                incidentQueueVisitors.Clear();
                visitorInterval = LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays;
                return;
            }
        }

        public void CheckIfTraderQueueTimeChanged()
        {
            if (traderInterval == 0f)
            {
                traderInterval = LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderFrequencyDays;
                return;
            }
            else if (traderInterval == LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderFrequencyDays)
            {
                return;
            }
            else if (traderInterval != LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderFrequencyDays)
            {
                incidentQueueTraders.Clear();
                traderInterval = LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().traderFrequencyDays;
                return;
            }
        }

        public void LoadTracker(Utils.Tracker tracker)
        {
            trackers.Add(tracker);
        }

    }
}
