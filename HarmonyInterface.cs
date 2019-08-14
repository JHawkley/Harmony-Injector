#pragma warning disable IDE0044,IDE0051,IDE0060

// just interface of Harmony
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
    //-------------------------------------
    public class ArgumentType
    {
        public static ArgumentType Normal;
        public static ArgumentType Ref;
        public static ArgumentType Out;
        public static ArgumentType Pointer;
    }
    public class HarmonyPatchType
    {
        public static HarmonyPatchType All;
        public static HarmonyPatchType Prefix;
        public static HarmonyPatchType Postfix;
        public static HarmonyPatchType Transpiler;
    }
    public class MethodType
    {
        public static MethodType Normal;
        public static MethodType Getter;
        public static MethodType Setter;
        public static MethodType Constructor;
        public static MethodType StaticConstructor;
    }
    public class PropertyMethod
    {
        public static PropertyMethod Getter;
        public static PropertyMethod Setter;
    }

    public static class PatchInfoSerialization
    {

    }
    public class Patch : IComparable
    {
        //public Patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null) { }
        readonly public int index;
        readonly public string owner;
        readonly public int priority;
        readonly public string[] before;
        readonly public string[] after;
        readonly public MethodInfo patch;
        public Patch(MethodInfo patch, int index, string owner, int priority, string[] before, string[] after) { }
        public MethodInfo GetMethod(MethodBase original) { return null; }
        public override bool Equals(object obj) { return true; }
        public int CompareTo(object obj) { return 0; }
        public override int GetHashCode() { return 0; }
    }

    public class Patches
    {
        public readonly ReadOnlyCollection<Patch> Prefixes;
        public readonly ReadOnlyCollection<Patch> Postfixes;
        public readonly ReadOnlyCollection<Patch> Transpilers;
        public ReadOnlyCollection<string> Owners;
        public Patches(Patch[] prefixes, Patch[] postfixes, Patch[] transpilers) { }
    }

    public class HarmonyInstance
    {
        HarmonyInstance(string id) { }
        public static HarmonyInstance Create(string id) { return null; }
        public IEnumerable<MethodBase> GetPatchedMethods() { return null; }
        public Patches GetPatchInfo(MethodBase method) { return null; }
        public bool HasAnyPatches(string harmonyID) { return false; }
        public DynamicMethod Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null) { return null; }
        public void PatchAll() { }
        public void PatchAll(Assembly assembly) { }
        public void Unpatch(MethodBase original, MethodInfo patch) { }
        public void Unpatch(MethodBase original, int type, string harmonyID = null) { }
        public void UnpatchAll(string harmonyID = null) { }
        public Dictionary<string, Version> VersionInfo(out Version currentVersion) { currentVersion = typeof(HarmonyInstance).Assembly.GetName().Version; return null; }
    }
    public class HarmonyMethod
    {
        void ImportMethod(MethodInfo theMethod) { }
        public HarmonyMethod() { }
        public HarmonyMethod(MethodInfo method) { }
        public HarmonyMethod(Type type, string name, Type[] parameters = null) { }
        public static List<string> HarmonyFields() { return null; }
        public static HarmonyMethod Merge(List<HarmonyMethod> attributes) { return null; }
        public override string ToString() { return ""; }
    }
    public static class HarmonyMethodExtensions
    {
        public static void CopyTo(this HarmonyMethod from, HarmonyMethod to) { }
        public static HarmonyMethod Clone(this HarmonyMethod original) { return null; }
        public static HarmonyMethod Merge(this HarmonyMethod master, HarmonyMethod detail) { return null; }
        public static List<HarmonyMethod> GetHarmonyMethods(this Type type) { return null; }
        public static List<HarmonyMethod> GetHarmonyMethods(this MethodBase method) { return null; }
    }
    public delegate object FastInvokeHandler(object target, object[] paramters);
    public class MethodInvoker
    {
        public static FastInvokeHandler GetHandler(DynamicMethod methodInfo, Module module) { return null; }
        public static FastInvokeHandler GetHandler(MethodInfo methodInfo) { return null; }
    }
    public class ExceptionBlock { }
    public class CodeInstruction
    {
        public CodeInstruction(OpCode opcode, object operand = null) { }
        public CodeInstruction(CodeInstruction instruction) { }
        public CodeInstruction Clone() { return null; }
        public CodeInstruction Clone(OpCode opcode) { return null; }
        public CodeInstruction Clone(OpCode opcode, object operand) { return null; }
        public OpCode opcode;
        public object operand;
        public List<Label> labels = new List<Label>();
        public List<ExceptionBlock> blocks = new List<ExceptionBlock>();
    }
    public static class Priority
    {
        public const int Last = 0;
        public const int VeryLow = 100;
        public const int Low = 200;
        public const int LowerThanNormal = 300;
        public const int Normal = 400;
        public const int HigherThanNormal = 500;
        public const int High = 600;
        public const int VeryHigh = 700;
        public const int First = 800;
    }
    // Annotation
    public class HarmonyAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatch : HarmonyAttribute
    {
        public HarmonyPatch() { }
        public HarmonyPatch(Type declaringType) { }
        public HarmonyPatch(Type declaringType, Type[] argumentTypes) { }
        public HarmonyPatch(Type declaringType, string methodName) { }
        public HarmonyPatch(Type declaringType, string methodName, params Type[] argumentTypes) { }
        public HarmonyPatch(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations) { }
        public HarmonyPatch(Type declaringType, MethodType methodType) { }
        public HarmonyPatch(Type declaringType, MethodType methodType, params Type[] argumentTypes) { }
        public HarmonyPatch(Type declaringType, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations) { }
        public HarmonyPatch(Type declaringType, string methodName, MethodType methodType) { }
        public HarmonyPatch(string methodName) { }
        public HarmonyPatch(string methodName, params Type[] argumentTypes) { }
        public HarmonyPatch(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations) { }
        public HarmonyPatch(string methodName, MethodType methodType) { }
        public HarmonyPatch(MethodType methodType) { }
        public HarmonyPatch(MethodType methodType, params Type[] argumentTypes) { }
        public HarmonyPatch(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations) { }
        public HarmonyPatch(Type[] argumentTypes) { }
        public HarmonyPatch(Type[] argumentTypes, ArgumentType[] argumentVariations) { }
        [Obsolete("This attribute will be removed in the next major version. Use HarmonyPatch together with MethodType.Getter or MethodType.Setter instead")]
        public HarmonyPatch(string propertyName, PropertyMethod type) { }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class HarmonyPatchAll : HarmonyAttribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HarmonyPriority : HarmonyAttribute
    {
        public HarmonyPriority(int priority) { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HarmonyBefore : HarmonyAttribute
    {
        public HarmonyBefore(params string[] before) { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HarmonyAfter : HarmonyAttribute
    {
        public HarmonyAfter(params string[] after) { }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPrepare : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyCleanup : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyTargetMethod : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyTargetMethods : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPrefix : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPostfix : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyTranspiler : Attribute { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HarmonyArgument : Attribute
    {
        public HarmonyArgument(string originalName) { }
        public HarmonyArgument(int index) { }
        public HarmonyArgument(string originalName, string newName) { }
        public HarmonyArgument(int index, string name) { }
    }

    public class AccessCache
    {
        public FieldInfo GetFieldInfo(Type type, string name) { return null; }
        public MethodBase GetMethodInfo(Type type, string name, Type[] arguments) { return null; }
        public PropertyInfo GetPropertyInfo(Type type, string name) { return null; }
    }
    public static class AccessTools
    {
        public static ConstructorInfo Constructor(Type type, Type[] parameters = null) { return null; }
        public static object CreateInstance(Type type) { return null; }
        public static ConstructorInfo DeclaredConstructor(Type type, Type[] parameters = null) { return null; }
        public static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null, Type[] generics = null) { return null; }
        public static PropertyInfo DeclaredProperty(Type type, string name) { return null; }
        public static FieldInfo Field(Type type, string name) { return null; }
        public static FieldInfo Field(Type type, int idx) { return null; }
        // do not support ?
        //public delegate ref U FieldRef<T, U>(T obj);
        //public static FieldRef<T, U> FieldRefAccess<T, U>(string fieldName){return null;}
        //public static ref U FieldRefAccess<T, U>(T instance, string fieldName){return ref FieldRefAccess<T, U>(fieldName)(instance);}
        public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> action) { return default(T); }
        public static T FindIncludingInnerTypes<T>(Type type, Func<Type, T> action) { return default(T); }
        public static ConstructorInfo FirstConstructor(Type type, Func<ConstructorInfo, bool> predicate) { return null; }
        public static Type FirstInner(Type type, Func<Type, bool> predicate) { return null; }
        public static MethodInfo FirstMethod(Type type, Func<MethodInfo, bool> predicate) { return null; }
        public static PropertyInfo FirstProperty(Type type, Func<PropertyInfo, bool> predicate) { return null; }
        public static List<ConstructorInfo> GetDeclaredConstructors(Type type) { return null; }
        public static List<FieldInfo> GetDeclaredFields(Type type) { return null; }
        public static List<MethodInfo> GetDeclaredMethods(Type type) { return null; }
        public static List<PropertyInfo> GetDeclaredProperties(Type type) { return null; }
        public static object GetDefaultValue(Type type) { return null; }
        public static List<string> GetFieldNames(Type type) { return null; }
        public static List<string> GetFieldNames(object instance) { return null; }
        public static List<string> GetMethodNames(Type type) { return null; }
        public static List<string> GetMethodNames(object instance) { return null; }
        public static List<string> GetPropertyNames(Type type) { return null; }
        public static List<string> GetPropertyNames(object instance) { return null; }
        public static Type GetReturnedType(MethodBase method) { return null; }
        public static Type[] GetTypes(object[] parameters) { return null; }
        public static Type Inner(Type type, string name) { return null; }
        public static bool IsClass(Type type) { return false; }
        public static bool IsStruct(Type type) { return false; }
        public static bool IsValue(Type type) { return false; }
        public static bool IsVoid(Type type) { return false; }
        public static object MakeDeepCopy(object source, Type resultType, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "") { return null; }
        public static void MakeDeepCopy<T>(object source, out T result, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "") { result = (T)((object)AccessTools.MakeDeepCopy(source, typeof(T), processor, pathRoot)); }
        public static MethodInfo Method(string typeColonMethodname, Type[] parameters = null, Type[] generics = null) { return null; }
        public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null) { return null; }
        public static PropertyInfo Property(Type type, string name) { return null; }
        public static void ThrowMissingMemberException(Type type, params string[] names) { }
        public static Type TypeByName(string name) { return null; }
        public static BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
    }
    public static class DynamicTools
    {
        public static DynamicMethod CreateDynamicMethod(MethodBase original, string suffix) { return null; }
        public static LocalBuilder[] DeclareLocalVariables(MethodBase original, ILGenerator generator, bool logOutput = true) { return null; }
        public static LocalBuilder DeclareLocalVariable(ILGenerator generator, Type type) { return null; }
        public static void PrepareDynamicMethod(DynamicMethod method) { }
    }
    public class Traverse<T>
    {
        private Traverse traverse;
        private Traverse() { }
        public Traverse(Traverse traverse) { }
        public T Value
        {
            get { return traverse.GetValue<T>(); }
            set { traverse.SetValue(value); }
        }
    }
    public class Traverse
    {
        static AccessCache Cache;
        static Traverse() { }
        public static Traverse Create(Type type) { return null; }
        public static Traverse Create<T>() { return null; }
        public static Traverse Create(object root) { return null; }
        public static Traverse CreateWithType(string name) { return null; }
        Traverse() { }
        public Traverse(Type type) { }
        public Traverse(object root) { }
        Traverse(object root, MemberInfo info, object[] index) { }
        Traverse(object root, MethodInfo method, object[] parameter) { }
        public object GetValue() { return null; }
        public T GetValue<T>() { return default(T); }
        public object GetValue(params object[] arguments) { return null; }
        public T GetValue<T>(params object[] arguments) { return default(T); }
        public Traverse SetValue(object value) { return null; }
        public Type GetValueType() { return null; }
        Traverse Resolve() { return null; }
        public Traverse Type(string name) { return null; }
        public Traverse Field(string name) { return null; }
        public Traverse<T> Field<T>(string name) { return null; }
        public List<string> Fields() { return null; }
        public Traverse Property(string name, object[] index = null) { return null; }
        public Traverse<T> Property<T>(string name, object[] index = null) { return null; }
        public List<string> Properties() { return null; }
        public Traverse Method(string name, params object[] arguments) { return null; }
        public Traverse Method(string name, Type[] paramTypes, object[] arguments = null) { return null; }
        public List<string> Methods() { return null; }
        public bool FieldExists() { return false; }
        public bool MethodExists() { return false; }
        public bool TypeExists() { return false; }
        public static void IterateFields(object source, Action<Traverse> action) { }
        public static void IterateFields(object source, object target, Action<Traverse, Traverse> action) { }
        public static void IterateFields(object source, object target, Action<string, Traverse, Traverse> action) { }
        public static void IterateProperties(object source, Action<Traverse> action) { }
        public static void IterateProperties(object source, object target, Action<Traverse, Traverse> action) { }
        public static void IterateProperties(object source, object target, Action<string, Traverse, Traverse> action) { }
        public override string ToString() { return ""; }
        public static Action<Traverse, Traverse> CopyFields = delegate (Traverse from, Traverse to) { to.SetValue(from.GetValue()); };
    }
}