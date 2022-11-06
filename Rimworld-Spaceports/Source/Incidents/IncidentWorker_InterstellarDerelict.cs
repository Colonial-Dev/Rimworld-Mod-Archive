using RimWorld;
using Verse;

namespace Spaceports.Incidents
{
    public class IncidentWorker_InterstellarDerelict : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().eventsEnabled || !LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().InterstellarDerelict)
            {
                return false;
            }
            if (!Utils.CheckIfSpaceport((Map)parms.target))
            {
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Letters.InterstellarDerelictLetter letter = (Letters.InterstellarDerelictLetter)LetterMaker.MakeLetter(def.letterLabel, "Spaceports_InterstellarDerelictLetter".Translate(), def.letterDef);
            letter.title = "Spaceports_InterstellarDerelictLetterLabel".Translate();
            letter.radioMode = true;
            letter.map = map;
            letter.StartTimeout(5000);
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }
    }
}
