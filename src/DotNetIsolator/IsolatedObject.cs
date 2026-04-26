namespace DotNetIsolator;

public class IsolatedObject(
    IsolatedRuntime runtimeInstance,
    int gcHandle,
    string assemblyName,
    string? @namespace,
    string? declaringTypeName,
    string typeName)
{
    private readonly IsolatedRuntime _runtimeInstance = runtimeInstance;
    private readonly string _assemblyName = assemblyName;
    private readonly string? _namespace = @namespace;
    private readonly string? _declaringTypeName = declaringTypeName;
    private readonly string _typeName = typeName;

    internal int GuestGCHandle { get; private set; } = gcHandle;

    public IsolatedMethod FindMethod(string methodName, int numArgs = -1)
    {
        if (GuestGCHandle == 0)
        {
            throw new InvalidOperationException("Cannot look up instance method because the object has already been released.");
        }

        return _runtimeInstance.GetMethod(_assemblyName, _namespace, _declaringTypeName, _typeName, methodName);
    }

    // Single arity-agnostic core. Replaces the previous five Invoke<T0..T4,TRes>
    // and five InvokeVoid<T0..T4> overloads with a params object?[] core.
    public TRes Invoke<TRes>(string methodName, params object?[] args)
        => FindMethod(methodName, args.Length).Invoke<TRes>(this, args);

    public void InvokeVoid(string methodName, params object?[] args)
        => FindMethod(methodName, args.Length).InvokeVoid(this, args);

    public void ReleaseGCHandle()
    {
        if (GuestGCHandle != 0)
        {
            _runtimeInstance.ReleaseGCHandle(GuestGCHandle);
            GuestGCHandle = 0;
        }
    }

    ~IsolatedObject()
    {
        ReleaseGCHandle();
    }
}
