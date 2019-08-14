using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using BuildLog = HarmonyInjector.BuildLog;
using InjectionException = HarmonyInjector.InjectionException;
using StaticInitializationException = HarmonyInjector.StaticInitializationException;
using XRLCore = XRL.Core.XRLCore;
using static HarmonyInjector.Tools;
using static HarmonyInjector.Constants;

namespace XRL.World.Parts
{

    public class ModInjector : IPart
    {

        /// <summary>
        /// Will be true when running in an injected context.
        /// See `HarmonyInjector.Constants.FauxModInjectorCode` to see the version
        /// of this class that is used by the injected context.
        /// </summary>
        public static bool InjectedHarmony => false;

        /// <summary>
        /// Indicates whether an injection attempt is currently taking place.
        /// </summary>
        public static bool CurrentlyInjecting { get; private set; } = false;

        private static ModInfo InjectorModInfo
        {
            get
            {
                var modInfo = ModManager.Mods.FirstOrDefault(m => m.ID == ModID && m.IsApproved);
                if (modInfo != null) return modInfo;
                throw new InjectionException($"The `ModInfo` entry for `{ModID}` could not be located or was not approved.");
            }
        }

        private static string InjectorModPath
        {
            get
            {
                // Find the directory containing this mod.
                var directory = InjectorModInfo.Path;
                if (!String.IsNullOrEmpty(directory) && Directory.Exists(directory)) return directory;
                throw new DirectoryNotFoundException($"Directory containing {ModID} was not found.");
            }
        }

        private static string HarmonyAssemblyLocation
        {
            get
            {
                // Load `0Harmony.dll` from this mod's directory.
                var location = Directory
                    .GetFiles(InjectorModPath)
                    .FirstOrDefault(path => CompareStrings(Path.GetFileName(path), HarmonyDll));

                if (!String.IsNullOrEmpty(location)) return location;
                throw new FileNotFoundException("Harmony DLL was not found.");
            }
        }

        public ModInjector()
        {
            // Only attempt injection once.
            if (ranOnce) return;
            ranOnce = true;

            // Inform an injection attempt is taking place.
            BuildLog.Info("Injecting Harmony...");
            CurrentlyInjecting = true;

            // Queue the injection on the UI-thread.  If initialization using the previous
            // mod assembly is still occuring, this will wait for it to complete before we
            // start mucking things up.
            QueueUITask(() => WaitFor(() => GameIsReady, DoInject, 0));
        }

        private static void DoInject()
        {
            // Cache the previous assembly.  It can't be unloaded and isn't going anywhere.
            var previousAssembly = ModManager.modAssembly;

            try
            {
                // Once the harmony assembly is loaded into the app-domain, Caves of Qud
                // will automatically reference it in the mod assembly.
                BuildLog.Info("Pulling the Harmony assembly into the current AppDomain.");
                var harmonyAssembly = Assembly.LoadFrom(HarmonyAssemblyLocation);

                // Add an `AppDomain.AssemblyResolve` handler that matches the mod assembly.
                // Harmony will need this to successfully perform un-patching.
                AppDomain.CurrentDomain.AssemblyResolve += (obj, args) =>
                    args.Name == ModManager.modAssembly?.FullName ? ModManager.modAssembly : null;

                BuildLog.Info("Rebuilding mods into a Harmony-injected assembly.");

                // Refresh the mods; this rebuilds `ModManager.Mods`, where we can then alter
                // this mod's source code into the "Harmony-injected" version.
                ModManager.Refresh();
                BuildLog.Info($"Adjusting the scripts of {ModID} for the injected mod assembly.");
                AdjustModForInjection();
                ModManager.BuildScriptMods();

                // Check to make sure the mod assembly has updated.
                if (ModManager.modAssembly == null)
                    throw new InjectionException($"The Harmony-injected mod assembly failed to build.");
                if (ModManager.modAssembly == typeof(ModInjector).Assembly)
                    throw new InjectionException($"The mod assembly still points to this assembly; this is not expected.");

                BuildLog.Info("Harmony-injected mod assembly built successfully.");

                BuildLog.Info("Calling static constructors in the mod assembly...");
                RunStaticConstructors();

                // A config reload ensures the game begins to use the types from the rebuilt assembly.
                BuildLog.Info("Reloading the game configuration...");
                XRLCore.Core.HotloadConfiguration();

                BuildLog.Info("Injection successful.");
            }
            catch (InjectionException ex)
            {
                BuildLog.Error(ex.Message);
                BuildLog.Error("Injection failed.");

                ShowErrorPopup(
                    "&YHarmony could not be injected properly.",
                    $"&R{ex.Message}"
                );
            }
            catch (StaticInitializationException ex)
            {
                // The mod assembly built successfully, but it is having trouble initializing.
                BuildLog.Error(ex.Message);
                BuildLog.Error(ex.InnerException);
                BuildLog.Error("Injection failed.");

                // Restore the old mod assembly.
                ModManager.modAssembly = previousAssembly;

                // We can't really know what mod failed, but hopefully we can give enough
                // information such that it is obvious to the player.
                var lastEx = EnumerateExceptions(ex.InnerException).Last();
                ShowErrorPopup(
                    $"&YStatic construction of &W{ex.TargetType.FullName}&Y failed.",
                    $"&wError &W{lastEx.GetType().Name}",
                    $"&R{lastEx.Message}",
                    TraceThrough(ex.InnerException).Select((s, i) => $"&w{(i > 0 ? "Via" : "At")} &W{s}")
                );
            }
            catch (Exception ex)
            {
                BuildLog.Error("An unexpected exception was raised during the injection process.");
                BuildLog.Error(ex);
                BuildLog.Error("Injection failed.");

                ShowErrorPopup(
                    "&YHarmony could not be injected properly.",
                    "&RAn unhandled error occurred during the injection process."
                );
            }
            finally
            {
                // Restore the previous assembly, in case of a failure, just in case
                // some kind of error recovery triggers and Qud still expects mod-types
                // to be retrievable.
                if (ModManager.modAssembly == null)
                {
                    ModManager.modAssembly = previousAssembly;
                    ModManager.bCompiled = true;
                }
                CurrentlyInjecting = false;
            }
        }

        private static IEnumerable<string> TraceThrough(Exception exception) =>
            EnumerateExceptions(exception).Reverse()
                .Select(ex => $"{ex.TargetSite.DeclaringType.FullName}:{ex.TargetSite.Name}");

        private static void ShowErrorPopup(params object[] msgParts)
        {
            var sb = new StringBuilder();
            foreach (var part in EnumerateStrings(msgParts)) sb.AppendLine(part);
            sb.AppendLine();
            sb.AppendLine("&YSome mods may not function correctly.");
            sb.Append("Check the output logs for more information.");

            HarmonyInjector.Popup.ShowMessage(sb.ToString());
        }

        private static void AdjustModForInjection()
        {
            var modInfo = InjectorModInfo;

            // Make a few changes to this mod's script files before rebuilding the mod assembly.
            // Since we're changing the collection in the loop, we'll copy it to an array first.
            foreach (var path in modInfo.ScriptFiles.ToArray())
            {
                switch (Path.GetFileName(path))
                {
                    // Remove sources that are not utilized by the injected version.
                    case "Exceptions.cs":
                    case "HarmonyInterface.cs":
                        modInfo.ScriptFiles.Remove(path);
                        modInfo.ScriptFileContents.Remove(path);
                        break;
                    // Replace with a simplified version of `XRL.World.Parts.ModInjector` to keep Qud happy.
                    case "ModInjector.cs":
                        modInfo.ScriptFileContents[path] = FauxModInjectorCode;
                        break;
                }
            }
        }

        private static void RunStaticConstructors()
        {
            foreach (var type in ModManager.modAssembly.GetTypes())
            {
                try { RuntimeHelpers.RunClassConstructor(type.TypeHandle); }
                catch (Exception ex)
                {
                    // Wrap and throw the exception.
                    throw new StaticInitializationException(type, ex);
                }
            }
        }

        private static bool ranOnce = false;

    }

}