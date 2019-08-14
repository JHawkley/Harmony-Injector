using System;

namespace HarmonyInjector
{

    /// <summary>
    /// Constants used throughout the mod.
    /// </summary>
    public static class Constants
    {

        /// <summary>
        /// The new version of CoQ comes with a new logging system based on
        /// a package called NLog.  For whatever reason, it does not appear
        /// to work when called from mods, and I can't figure out why.
        /// So, if that ever changes, this can be used to switch between
        /// the two logging systems.
        /// </summary>
        public const bool UseNewLogging = false;

        public const string ModID = "Harmony Injector";
        public const string SharedHarmonyID = "nk.compiled_injector";
        public const string HarmonyDll = "0Harmony.dll";
        public const string QudExe = "CoQ.exe";

        /// <summary>
        /// Code to fill in for `XRL.World.Parts.ModInjector` in the
        /// Harmony-injected assembly.
        /// </summary>
        public const string FauxModInjectorCode = @"
            using System;
            namespace XRL.World.Parts
            {
                public class ModInjector : XRL.World.IPart
                {
                    public static bool InjectedHarmony => true;
                    public static bool CurrentlyInjecting => false;
                }
            }
        ";

        public static readonly Action NoopFn = () => { };

    }

}