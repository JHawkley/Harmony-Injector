#pragma warning disable CS0162

using System;

using UnityDebug = UnityEngine.Debug;
using QudDebug = global::Logger;
using static HarmonyInjector.Constants;

namespace HarmonyInjector
{

    /// <summary>
    /// The debug logger for game-mode.
    /// </summary>
    public static class GameLog
    {

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message)
        {
            if (UseNewLogging) QudDebug.gameLog.Info(Format(message));
            else UnityDebug.Log(Format(message, "Info"));
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(string message)
        {
            if (UseNewLogging) QudDebug.gameLog.Error(Format(message));
            else UnityDebug.LogError(Format(message, "Error"));
        }

        /// <summary>
        /// Logs an exception as an error.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        public static void Error(Exception exception)
        {
            Error(exception.ToLogMessage());
        }

        private static string Format(object message) =>
            $"[{ModID}] {message}";

        private static string Format(object message, string level) =>
            $"[{ModID}::Game {level}] {message}";

    }

    /// <summary>
    /// The debug logger for mod-compilation.
    /// </summary>
    public static class BuildLog
    {

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message)
        {
            if (UseNewLogging) QudDebug.buildLog.Info(Format(message));
            else UnityDebug.Log(Format(message, "Info"));
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(string message)
        {
            if (UseNewLogging) QudDebug.buildLog.Error(Format(message));
            else UnityDebug.LogError(Format(message, "Error"));
        }

        /// <summary>
        /// Logs an exception as an error.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        public static void Error(Exception exception)
        {
            Error(exception.ToLogMessage());
        }

        private static string Format(object message) =>
            $"[{ModID}] {message}";

        private static string Format(object message, string level) =>
            $"[{ModID}::Build {level}] {message}";

    }

}