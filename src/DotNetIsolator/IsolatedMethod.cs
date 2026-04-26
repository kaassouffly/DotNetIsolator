using MessagePack;

namespace DotNetIsolator;

public class IsolatedMethod(IsolatedRuntime runtimeInstance, string name, int monoMethodPtr)
{
    private readonly IsolatedRuntime _runtimeInstance = runtimeInstance;
    private readonly int _monoMethodPtr = monoMethodPtr;

    public string Name { get; } = name;

    // Single arity-agnostic core. The previous five generic overloads
    // (Invoke<T0..T4,TRes>) collapse here via a params object?[] tail.
    // Ideally we'd serialize directly into guest memory but that probably involves implementing
    // an IBufferWriter<byte> that knows how to allocate chunks of guest memory.
    // We might also want to special-case some basic known parameter types and skip MessagePack
    // for them, instead using ShadowStack and the raw bytes.
    public TRes Invoke<TRes>(IsolatedObject? instance, params object?[] args)
    {
        if (args.Length == 0)
        {
            return _runtimeInstance.InvokeDotNetMethod<TRes>(_monoMethodPtr, instance, ReadOnlySpan<int>.Empty);
        }

        Span<int> argAddresses = args.Length <= 16
            ? stackalloc int[args.Length]
            : new int[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            argAddresses[i] = _runtimeInstance.CopyValueLengthPrefixed(
                MessagePackSerializer.Typeless.Serialize(args[i]));
        }

        try
        {
            return _runtimeInstance.InvokeDotNetMethod<TRes>(_monoMethodPtr, instance, argAddresses);
        }
        finally
        {
            for (var i = 0; i < argAddresses.Length; i++)
            {
                _runtimeInstance.Free(argAddresses[i]);
            }
        }
    }

    public void InvokeVoid(IsolatedObject? instance, params object?[] args)
        => Invoke<object>(instance, args);
}
