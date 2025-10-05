using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DopeGrid;

static class Pool
{
    public static Lazy<ConcurrentDictionary<object, byte>> UsingSet { get; } =
        new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);
}

internal static class ArrayPool<T>
{
    private static readonly Lazy<System.Buffers.ArrayPool<T>> s_pool = new(System.Buffers.ArrayPool<T>.Create,  LazyThreadSafetyMode.ExecutionAndPublication);
    public static T[] Rent(int minimumLength)
    {
        var obj = s_pool.Value.Rent(minimumLength);
        Pool.UsingSet.Value.TryAdd(obj, 0);
        return obj;
    }

    public static void Return(T[] obj)
    {
        if (Pool.UsingSet.Value.TryRemove(obj, out _)) s_pool.Value.Return(obj, clearArray: true);
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class ListPool<T>
{
    private static readonly ConcurrentQueue<List<T>> s_pool = new();

    public static List<T> Rent(int capacity = 0)
    {
        if (!s_pool.TryDequeue(out var list))
        {
            list = capacity > 0 ? new List<T>(capacity) : new List<T>();
        }
        else if (capacity > 0 && list.Capacity < capacity)
        {
            list.Capacity = capacity;
        }
        Pool.UsingSet.Value.TryAdd(list, 0);
        return list;
    }

    public static void Return(List<T> list)
    {
        if (Pool.UsingSet.Value.TryRemove(list, out _))
        {
            list.Clear();
            s_pool.Enqueue(list);
        }
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class DictionaryPool<TKey, TValue>
    where TKey : notnull
{
    private static readonly ConcurrentQueue<Dictionary<TKey, TValue>> s_pool = new();

    public static Dictionary<TKey, TValue> Rent(int capacity = 0)
    {
        if (!s_pool.TryDequeue(out var dictionary))
        {
            dictionary = capacity > 0 ? new Dictionary<TKey, TValue>(capacity) : new Dictionary<TKey, TValue>();
        }
        else if (capacity > 0)
        {
            dictionary.EnsureCapacity(capacity);
        }
        Pool.UsingSet.Value.TryAdd(dictionary, 0);
        return dictionary;
    }

    public static void Return(Dictionary<TKey, TValue> dictionary)
    {
        if (Pool.UsingSet.Value.TryRemove(dictionary, out _))
        {
            dictionary.Clear();
            s_pool.Enqueue(dictionary);
        }
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class HashSetPool<T>
{
    private static readonly ConcurrentQueue<HashSet<T>> s_pool = new();

    public static HashSet<T> Rent(int capacity = 0)
    {
        if (!s_pool.TryDequeue(out var hashSet))
        {
            hashSet = capacity > 0 ? new HashSet<T>(capacity) : new HashSet<T>();
        }
        else if (capacity > 0)
        {
            hashSet.EnsureCapacity(capacity);
        }
        Pool.UsingSet.Value.TryAdd(hashSet, 0);
        return hashSet;
    }

    public static void Return(HashSet<T> hashSet)
    {
        if (Pool.UsingSet.Value.TryRemove(hashSet, out _))
        {
            hashSet.Clear();
            s_pool.Enqueue(hashSet);
        }
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class StackPool<T>
{
    private static readonly ConcurrentQueue<Stack<T>> s_pool = new();

    public static Stack<T> Rent(int capacity = 0)
    {
        if (!s_pool.TryDequeue(out var stack))
        {
            stack = capacity > 0 ? new Stack<T>(capacity) : new Stack<T>();
        }
        Pool.UsingSet.Value.TryAdd(stack, 0);
        return stack;
    }

    public static void Return(Stack<T> stack)
    {
        if (Pool.UsingSet.Value.TryRemove(stack, out _))
        {
            stack.Clear();
            s_pool.Enqueue(stack);
        }
        else throw new InvalidOperationException("Object was not in use");
    }
}

internal static class QueuePool<T>
{
    private static readonly ConcurrentQueue<Queue<T>> s_pool = new();

    public static Queue<T> Rent(int capacity = 0)
    {
        if (!s_pool.TryDequeue(out var queue))
        {
            queue = capacity > 0 ? new Queue<T>(capacity) : new Queue<T>();
        }
        Pool.UsingSet.Value.TryAdd(queue, 0);
        return queue;
    }

    public static void Return(Queue<T> queue)
    {
        if (Pool.UsingSet.Value.TryRemove(queue, out _))
        {
            queue.Clear();
            s_pool.Enqueue(queue);
        }
        else throw new InvalidOperationException("Object was not in use");
    }
}
