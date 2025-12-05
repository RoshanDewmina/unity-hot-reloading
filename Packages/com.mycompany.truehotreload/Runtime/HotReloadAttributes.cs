using System;

namespace TrueHotReload.Runtime
{
    /// <summary>
    /// Optional marker attribute to explicitly opt-in a class for hot reloading.
    /// By default, all classes are hot-reloadable unless excluded by settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class HotReloadableAttribute : Attribute
    {
    }

    /// <summary>
    /// Marker attribute to explicitly exclude a method from hot reloading.
    /// Useful for methods that should never be patched (e.g., critical initialization code).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NoHotReloadAttribute : Attribute
    {
    }

    /// <summary>
    /// Marker attribute to preserve static field values across hot reloads.
    /// Without this, static field values will reset when methods are patched.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PreserveOnHotReloadAttribute : Attribute
    {
    }
}
