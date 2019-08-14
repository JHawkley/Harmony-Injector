using System;
using System.Reflection;
using Harmony;

using static HarmonyInjector.Constants;

namespace HarmonyInjector
{

    public static class Injector
    {

        /// <summary>
        /// Whether this assembly is running in a Harmony-injected context.
        /// </summary>
        public static bool HarmonyInjected => XRL.World.Parts.ModInjector.InjectedHarmony;

        /// <summary>
        /// Whether the shared harmony instance has been created and attribute-based patches applied.
        /// </summary>
        public static bool HarmonyStaticInstalled => instance != null;

        /// <summary>
        /// The shared Harmony instance.
        /// You can use this to apply manual patches or make you own local instance;
        /// both options work fine.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// When `HarmonyStaticInstalled` is `false`; the Harmony instance has not yet been created.
        /// </exception>
        public static HarmonyInstance Shared
        {
            get
            {
                if (HarmonyStaticInstalled) return instance;
                throw new NotSupportedException("The shared Harmony instance is not currently available.");
            }
        }

        static Injector()
        {
            // Verify that we're currently in the Harmony-injected context.
            if (!HarmonyInjected) return;

            BuildLog.Info("Starting initialization.");

            try
            {
                // Init harmony instance.
                BuildLog.Info("Creating the shared Harmony instance and applying attribute-based patches.");
                instance = HarmonyInstance.Create(SharedHarmonyID);
                instance.PatchAll(typeof(Injector).Assembly);

                BuildLog.Info("Initialization successful.");
            }
            catch (Exception ex)
            {
                BuildLog.Error("An exception was raised during initialization.");
                BuildLog.Error(ex);

                if (instance != null)
                {
                    // Note: `UnpatchAll` may not work properly with the current version of Harmony.
                    // I ran into problems reversing patches when I was testing this feature.
                    // Supposedly, Harmony 2.0 will resolve the issues I encountered.
                    BuildLog.Info("Reversing all Harmony patches that may have been applied before the error.");
                    try { instance.UnpatchAll(SharedHarmonyID); }
                    catch {}
                    finally { instance = null; }
                }

                BuildLog.Error("Initialization failed.");
            }
        }

        // Consider changing these to be read-only properties?
        // Left as is, since they're part of the current public API.

        /// <summary>
        /// Gets Caves of Qud's assembly, `Assembly-CSharp`.
        /// </summary>
        /// <returns>The `Assembly` instance for the game.</returns>
        public static Assembly GetUnityAssembly()
        {
            if (unityAssembly != null) return unityAssembly;
            unityAssembly = typeof(XRL.ModManager).Assembly;
            return unityAssembly;
        }

        /// <summary>
        /// Gets the absolute path to Caves of Qud's assembly, `Assembly-CSharp`.
        /// </summary>
        /// <returns>The absolute path to the game's assembly.</returns>
        public static string GetUnityAssemblyLocation() => GetUnityAssembly().Location;

        /// <summary>
        /// Tries to get the specified type from the main game assembly.
        /// Returns `null` in the case the lookup failed.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to get.</param>
        /// <returns>The `Type` instance or `null` if it could not be found.</returns>
        public static Type GetUnityType(string typeName)
        {
            var type = GetUnityAssembly().GetType(typeName, false);
            if (type == null) GameLog.Error($"Failed to get Unity type: {typeName}");
            return type;
        }

        /// <summary>
        /// Tries to get the specified type from the main game assembly.
        /// This version operates as a partial-function, returning `false` if it failed.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to get.</param>
        /// <param name="type">The reference to assign the `Type` instance to.</param>
        /// <returns>Whether the type was found.</returns>
        public static bool TryGetUnityType(string typeName, out Type type)
        {
            type = GetUnityType(typeName);
            return type != null;
        }

        private static HarmonyInstance instance;
        private static Assembly unityAssembly;

    }

}