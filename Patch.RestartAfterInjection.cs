using System;
using System.Collections.Generic;
using System.IO;
using Harmony;

using Process = System.Diagnostics.Process;
using Application = UnityEngine.Application;
using ModManager = XRL.ModManager;
using static HarmonyInjector.Constants;
using static HarmonyInjector.Tools;

namespace HarmonyInjector
{

    /// <summary>
    /// Patches `ModManager.BuildScriptMods` to restart the game if it wants to rebuild the
    /// mod assembly after Harmony has been injected.
    /// </summary>
    [HarmonyPatch(typeof(ModManager), "BuildScriptMods")]
    public static class Patch_RestartAfterInjection
    {

        /// <summary>
        /// The location of the Caves of Qud executable.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// When the executable's location could not be inferred.
        /// </exception>
        public static string QudExecutable
        {
            get
            {
                foreach (var location in PossibleExecutableLocations())
                    if (File.Exists(location))
                        return location;

                throw new FileNotFoundException("The Caves of Qud executable (CoQ.exe) could not be found.");
            }
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            // Do nothing if no compilation has been requested.
            if (ModManager.bCompiled) return false;

            // Otherwise, restart the game.
            // While it is plausible to unpatch everything, it isn't really possible to know
            // how other mods will react to this.  Restarting is the safest thing to do.
            BuildLog.Info("A request to rebuild the mod assembly has been received after Harmony was injected.");
            BuildLog.Info("For the sake of safety, the game executable will be restarted instead.");

            QueueUITask(() => {
                var message = String.Concat(
                    "Harmony has applied patches to the running game.",
                    "\r\n\r\n",
                    "Caves of Qud must be restarted in order to safely rebuild the mods."
                );

                Popup.ShowMessage(message, RestartGame);
            });
            
            return false;
        }

        private static void RestartGame()
        {
            // This operation will cause the "Resolution Dialog" to appear instead of the game's
            // main-menu just reappearing.  Oh well!  It's better than nothing, I guess.
            try { Process.Start(QudExecutable); }
            catch { }
            finally { Application.Quit(); }
        }

        private static IEnumerable<string> PossibleExecutableLocations()
        {
            var loc = default(string);
            var dataPath = Application.dataPath;

            // Expected location on Windows/Linux.
            if (Trial(Path.Combine, Application.dataPath, $"..\\{QudExe}", out loc)) yield return loc;
            // Expected location on OSX.
            if (Trial(Path.Combine, Application.dataPath, $"..\\..\\{QudExe}", out loc)) yield return loc;
            // One more suggestion to get the folder of the executable.
            if (Trial(Path.GetFullPath, $".\\{QudExe}", out loc)) yield return loc;
        }

        // These `Trial` methods are needed because it is not possible to `yield return` from a
        // `try-catch` block.  These convert a `try-catch` into a partial-function.

        private static bool Trial(Func<string, string> func, string arg1, out string retVal)
        {
            try { retVal = func(arg1); }
            catch { retVal = null; }
            return retVal != null;
        }

        private static bool Trial(Func<string, string, string> func, string arg1, string arg2, out string retVal)
        {
            try { retVal = func(arg1, arg2); }
            catch { retVal = null; }
            return retVal != null;
        }

    }

}