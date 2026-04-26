using Wasmtime;

namespace DotNetIsolator;

/// <summary>
/// Small helpers around <see cref="Wasmtime.Instance"/> that centralize the
/// "missing required export" error-throwing pattern.
/// </summary>
internal static class WasmtimeExtensions
{
    public static Memory RequireMemory(this Instance instance, string name)
        => instance.GetMemory(name) ?? Missing<Memory>(name);

    // The CS8619 suppressions below are needed because Wasmtime's GetFunction<...>(name)
    // overloads return Func<..., TResult?> (the result element is annotated nullable to
    // model "the export wasn't found"). Once we've null-checked the delegate itself we
    // know the inner element is non-null too, but generic-variance rules still flag the
    // assignment. The runtime semantics are identical.
#pragma warning disable CS8619
    public static Func<TResult> RequireFunction<TResult>(this Instance instance, string name)
        => instance.GetFunction<TResult>(name) ?? Missing<Func<TResult>>(name);

    public static Func<T1, TResult> RequireFunction<T1, TResult>(this Instance instance, string name)
        => instance.GetFunction<T1, TResult>(name) ?? Missing<Func<T1, TResult>>(name);

    public static Func<T1, T2, TResult> RequireFunction<T1, T2, TResult>(this Instance instance, string name)
        => instance.GetFunction<T1, T2, TResult>(name) ?? Missing<Func<T1, T2, TResult>>(name);

    public static Func<T1, T2, T3, T4, TResult> RequireFunction<T1, T2, T3, T4, TResult>(this Instance instance, string name)
        => instance.GetFunction<T1, T2, T3, T4, TResult>(name) ?? Missing<Func<T1, T2, T3, T4, TResult>>(name);

    public static Func<T1, T2, T3, T4, T5, T6, TResult> RequireFunction<T1, T2, T3, T4, T5, T6, TResult>(this Instance instance, string name)
        => instance.GetFunction<T1, T2, T3, T4, T5, T6, TResult>(name) ?? Missing<Func<T1, T2, T3, T4, T5, T6, TResult>>(name);
#pragma warning restore CS8619

    public static Action<T1> RequireAction<T1>(this Instance instance, string name)
        => instance.GetAction<T1>(name) ?? Missing<Action<T1>>(name);

    private static T Missing<T>(string name)
        => throw new InvalidOperationException($"Missing required export '{name}'");
}
