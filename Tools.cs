using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

using XRLCore = XRL.Core.XRLCore;

namespace HarmonyInjector
{

    /// <summary>
    /// Provides various supporting methods and properties used throughout the mod.
    /// </summary>
    public static class Tools
    {

        /// <summary>
        /// Whether the game's start-up routine has completed.
        /// When the game's splash screen fades is the best hint of this I could find.
        /// </summary>
        public static bool GameIsReady => GameManager.FadeSplash;

        /// <summary>
        /// Whether the caller is currently on the game-thread.
        /// </summary>
        public static bool OnGameThread => Thread.CurrentThread == XRLCore.CoreThread;

        /// <summary>
        /// Whether the caller is currently on the UI-thread.
        /// Now, this is an assumption, but it's unlikely that this can be called from
        /// anywhere besides the UI-thread.
        /// </summary>
        public static bool OnUIThread => !OnGameThread;

        private static global::GameManager GameManager => global::GameManager.Instance;

        /// <summary>
        /// Performs case-insensitive comparison on two strings.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns>Whether the strings are considered equal.</returns>
        public static bool CompareStrings(string a, string b) =>
            String.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Describes an exception in a log-friendly way.
        /// </summary>
        /// <param name="exception">The exception to build a message for.</param>
        /// <returns>A log-friendly exception message.</returns>
        public static string ToLogMessage(this Exception exception)
        {
            var sb = new StringBuilder();
            sb.Append("Exception report...");
            sb.AppendLine().AppendLine();
            sb.Append(exception.ToString());

            if (exception.Data.Count > 0)
            {
                // List out the custom data stored in the exception.
                sb.AppendLine().AppendLine();
                sb.Append("Custom exception data:");

                var de = exception.Data.GetEnumerator();
                while (de.MoveNext())
                {
                    // Indent the data, if it has new-lines.
                    var value = de.Value.ToString().Replace("\n", "\n    ");
                    var data = $"  {de.Key.ToString()} => {value}";
                    sb.AppendLine().Append(data);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Enumerates an exception and all of its inner-exceptions.
        /// </summary>
        /// <param name="exception">The exception to enumerate.</param>
        /// <returns>A sequence of exceptions.</returns>
        public static IEnumerable<Exception> EnumerateExceptions(Exception ex)
        {
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// Flattens the given strings and sequences-of-string into a single sequence.
        /// </summary>
        /// <param name="args">The strings and/or sequences-of-string to flatten.</param>
        /// <returns>A sequence of strings.</returns>
        /// <exception cref="ArgumentException">
        /// When `args` contains an object that is neither a string nor a sequence-of-string.
        /// </exception>
        public static IEnumerable<string> EnumerateStrings(params object[] args)
        {
            var mString = default(string);
            var mEnumerable = default(IEnumerable<string>);
            foreach (var obj in args)
            {
                // C# 6 pattern-matching!  How awful!  C# 7 pls!
                if (Match_String(obj, out mString)) goto caseString;
                if (Match_StringEnumerable(obj, out mEnumerable)) goto caseEnumerable;
            caseDefault:
                var msg = String.Concat(
                    "Expected to receive only strings or sequences-of-string; ",
                    $"received a `{obj.GetType().FullName}`."
                );
                throw new ArgumentException(msg, nameof(args));
            caseString:
                yield return mString;
                continue;
            caseEnumerable:
                foreach (var str in mEnumerable)
                    yield return str;
                continue;
            }
        }

        // More stuff that would be nicer if generics worked in mods.
        // Could just make a single `Match_Type<T>` instead.

        private static bool Match_String(object obj, out string output)
        {
            output = obj as string;
            return output != null;
        }

        private static bool Match_StringEnumerable(object obj, out IEnumerable<string> output)
        {
            output = obj as IEnumerable<string>;
            if (output != null) return true;

            output = (obj as StringCollection)?.Cast<string>();
            return output != null;
        }

        /// <summary>
        /// Queues a task to be executed by the task queue of the current thread later.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="delay">The count of milliseconds to delay the execution by.</param>
        public static void QueueTask(Action action, int delay = 0)
        {
            if (OnGameThread) QueueGameTask(action, delay);
            else QueueUITask(action, delay);
        }

        /// <summary>
        /// Enqueues a task to be executed on the UI-thread at a later time.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="delay">The count of milliseconds to delay the execution by.</param>
        public static void QueueUITask(Action action, int delay = 0) =>
            GameManager.uiQueue.queueTask(action, delay);
        
        /// <summary>
        /// Enqueues a task to be executed on the game-thread at a later time.
        /// </summary>
        /// <remark>
        /// The game thread's task queue is typically consumed during gameplay.
        /// It is unusual for tasks to be consumed when a game is not running.
        /// This method is mostly here for completion's sake.
        /// </remark>
        /// <param name="action">The action to execute.</param>
        /// <param name="delay">The count of milliseconds to delay the execution by.</param>
        public static void QueueGameTask(Action action, int delay = 0) =>
            GameManager.gameQueue.queueTask(action, delay);
        
        /// <summary>
        /// Blocks a thread while the `predicate` returns true.
        /// The millisecond count between checks can be set with the `msPerCheck` parameter.
        /// </summary>
        /// <param name="predicate">A function that determines when the block should end.</param>
        /// <param name="msPerCheck">The count of milliseconds to sleep the thread between checks.</param>
        public static void BlockWhile(Func<bool> predicate, int msPerCheck = 20)
        {
            while (predicate())
                Thread.Sleep(msPerCheck);
        }

        /// <summary>
        /// Executes an action once the given predicate evaluates to `true`.
        /// The check and subsequent action is performed on the current thread, with the
        /// specified delay between checks.
        /// </summary>
        /// <param name="predicate">A function that determines when the action should run.</param>
        /// <param name="action">The action to run.</param>
        /// <param name="delayPerCheck">The count of milliseconds to wait between checks.</param>
        public static void WaitFor(Func<bool> predicate, Action action, int delayPerCheck = 20)
        {
            if (predicate()) action();
            else QueueTask(() => WaitFor(predicate, action, delayPerCheck), delayPerCheck);
        }

    }

}