using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DopeGrid;

internal static class ArrayPool
{
    private static readonly Lazy<ConcurrentDictionary<object, byte>> s_using = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static T[] Rent<T>(int minimumLength)
    {
        var obj = ArrayPool<T>.Pool.Value.Rent(minimumLength);
        Array.Clear(obj, 0, obj.Length);
        s_using.Value.TryAdd(obj, 0);
        return obj;
    }

    public static void Return<T>(T[] obj)
    {
        if (s_using.Value.TryRemove(obj, out _)) ArrayPool<T>.Pool.Value.Return(obj);
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class ArrayPool<T>
{
    public static Lazy<System.Buffers.ArrayPool<T>> Pool { get; } = new(System.Buffers.ArrayPool<T>.Create,  LazyThreadSafetyMode.ExecutionAndPublication);
    public static T[] Rent(int minimumLength) => ArrayPool.Rent<T>(minimumLength);
    public static void Return(T[] obj) => ArrayPool.Return(obj);
}
