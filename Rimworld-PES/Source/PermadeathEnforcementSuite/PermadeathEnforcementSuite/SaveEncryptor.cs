using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PermadeathEnforcementSuite
{
    public static class SaveEncryptor
    {
        [HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.SaveGame), new Type[] { typeof(string)})]
        public static class Patch_SaveGame
        {
            public static bool Prefix(ref string fileName)
            {

                try
                {
                    Traverse traverse = Traverse.CreateWithType("GameDataSaveLoader").Field("lastSaveTick");
                    SafeSaver.Save(GenFilePaths.FilePathForSavedGame(fileName), "savegame", (Action)delegate
                    {
                        ScribeMetaHeaderUtility.WriteMetaHeader();
                        Game target = Current.Game;
                        Scribe_Deep.Look(ref target, SaveEncryptor.EncryptorUtils.getNodeName());
                    });
                    traverse.SetValue(Find.TickManager.TicksGame);
                }

                catch (Exception e)
                {
                    Log.Error("[PES] Exception in patched saving method!" + e);
                }

                return false;
            }

        }

        [HarmonyPatch(typeof(SavedGameLoaderNow), nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow), new Type[] { typeof(string) })]
        public static class Patch_LoadGame
        {
            public static bool Prefix(ref string fileName)
            {
                string text = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageIdPlayerFacing).ToLineList("  - ");
                Log.Message("Loading game from file " + fileName + " with mods:\n" + text);
                DeepProfiler.Start("Loading game from file " + fileName);
                Current.Game = new Game();
                DeepProfiler.Start("InitLoading (read file)");
                Scribe.loader.InitLoading(GenFilePaths.FilePathForSavedGame(fileName));
                DeepProfiler.End();
                try
                {
                    ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, logVersionConflictWarning: true);
                    if (!Scribe.EnterNode(SaveEncryptor.EncryptorUtils.getNodeName()))
                    {
                        Log.Error("[PES] Could not find PESGame XML node.");
                        Scribe.ForceStop();
                        GenScene.GoToMainMenu();
                        Messages.Message("This save file is not PES-enabled. Please load a PES-enabled save file, or disable PES to access this save file.", MessageTypeDefOf.RejectInput);
                        return false;
                    }
                    Current.Game = new Game();
                    Current.Game.LoadGame();
                }
                catch (Exception)
                {
                    Scribe.ForceStop();
                    throw;
                }
                PermadeathModeUtility.CheckUpdatePermadeathModeUniqueNameOnGameLoad(fileName);
                DeepProfiler.End();
                return false;
            }
        }

        public static class EncryptorUtils 
        {
            public static String getNodeName() {
                if (LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().allowStorytellerAccess)
                {
                    return "PESGame_1";
                }
                else if (LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().disableDoubleAutosave)
                {
                    return "PESGame_2";
                }
                else if (LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().allowStorytellerAccess && LoadedModManager.GetMod<PESMod>().GetSettings<PESSettings>().disableDoubleAutosave)
                {
                    return "PESGame_3";
                }
                else {
                    return "PESGame";
                }
            }
        }
    }
}
