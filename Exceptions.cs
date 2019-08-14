using System;

namespace HarmonyInjector
{
    /// <summary>
    /// Special exception thrown when injection fails.
    /// </summary>
    public sealed class InjectionException : Exception
    {
        public InjectionException(string message) : base(message) { }
        public InjectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Special exception thrown when a static constructor throws.
    /// </summary>
    public sealed class StaticInitializationException : InvalidOperationException
    {
        public Type TargetType { get; private set; }
        public StaticInitializationException(Type type, Exception innerException) :
        base($"The static constructor of `{type.FullName}` raised an exception.", innerException)
        { TargetType = type; }
    }
}