using System;
using System.Collections.Generic;
using System.Threading;

namespace DopeGrid;

internal static class ArrayPool
{
    private static readonly Lazy<HashSet<object>> s_using = new(() => new(128), LazyThreadSafetyMode.ExecutionAndPublication);

    public static T[] Rent<T>(int minimumLength)
    {
        var obj = ArrayPool<T>.Pool.Value.Rent(minimumLength);
        Array.Clear(obj, 0, obj.Length);
        s_using.Value.Add(obj);
        return obj;
    }

    public static void Return<T>(T[] obj)
    {
        if (s_using.Value.Remove(obj)) ArrayPool<T>.Pool.Value.Return(obj);
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class ArrayPool<T>
{
    public static Lazy<System.Buffers.ArrayPool<T>> Pool { get; } = new(System.Buffers.ArrayPool<T>.Create,  LazyThreadSafetyMode.ExecutionAndPublication);
    public static T[] Rent(int minimumLength) => ArrayPool.Rent<T>(minimumLength);
    public static void Return(T[] obj) => ArrayPool.Return(obj);
}
