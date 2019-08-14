using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using HarmonyInjector;

namespace HarmonyShim
{

    /// <summary>
    /// Provides extremely basic patching capabilities utilizing Harmony.
    /// </summary>
    public static class Harmony
    {

        /// <summary>
        /// Patches a method, applying the given prefix, postfix, and transpiler methods as patches.
        /// </summary>
        /// <param name="original">The method to be patched.</param>
        /// <param name="prefix">The prefix method, or `null` if no prefix patch is to be applied.</param>
        /// <param name="postfix">The postfix method, or `null` if no postfix is to be applied.</param>
        /// <param name="transpiler">The transpiler method, or `null` if no transpiler is to be applied.</param>
        public static void Patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            Injector.Shared.Patch(
                original, 
                prefix == null ? null : new HarmonyMethod(prefix), 
                postfix == null ? null : new HarmonyMethod(postfix), 
                transpiler == null ? null : new HarmonyMethod(transpiler)
            );
        }

        /// <summary>
        /// Patches a method, applying the given method as a prefix patch.
        /// </summary>
        /// <param name="original">The method to be patched.</param>
        /// <param name="prefix">The prefix method.</param>
        public static void ApplyPrefix(MethodBase original, MethodInfo prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            Patch(original, prefix: prefix);
        }

        /// <summary>
        /// Patches a method, applying the given method as a postfix patch.
        /// </summary>
        /// <param name="original">The method to be patched.</param>
        /// <param name="postfix">The postfix method.</param>
        public static void ApplyPostfix(MethodBase original, MethodInfo postfix)
        {
            if (postfix == null) throw new ArgumentNullException(nameof(postfix));
            Patch(original, postfix: postfix);
        }

        /// <summary>
        /// Patches a method, applying the given method as a transpiler patch.
        /// </summary>
        /// <param name="original">The method to be patched.</param>
        /// <param name="transpiler">The transpiler method.</param>
        public static void ApplyTranspiler(MethodBase original, MethodInfo transpiler)
        {
            if (transpiler == null) throw new ArgumentNullException(nameof(transpiler));
            Patch(original, transpiler: transpiler);
        }

        /// <summary>
        /// Unpatches a method when given the method that was used in its patch.
        /// </summary>
        /// <param name="original">The method to be unpatched.</param>
        /// <param name="patch">The method that was used as the patch.</param>
        public static void Unpatch(MethodBase original, MethodInfo patch)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (patch == null) throw new ArgumentNullException(nameof(patch));
            Injector.Shared.Unpatch(original, patch);
        }

        /// <summary>
        /// Gets an enumeration of all patched methods.
        /// </summary>
        /// <returns>An enumeration of all patched methods.</returns>
        public static IEnumerable<MethodBase> GetPatchedMethods() =>
            Injector.Shared.GetPatchedMethods();

    }


}
