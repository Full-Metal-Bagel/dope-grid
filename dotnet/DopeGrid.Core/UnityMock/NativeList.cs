using System;
using System.Collections.Generic;

namespace Unity.Collections;

public struct NativeList<T> : IDisposable where T : struct
{
    private readonly List<T> _list = new();
    private bool _isCreated = false;

    public int Length => _list.Count;
    public int Capacity => _list.Capacity;
    public bool IsCreated => _isCreated;

    public NativeList(Allocator allocator)
    {
        _list = new List<T>();
        _isCreated = true;
    }

    public NativeList(int capacity, Allocator allocator)
    {
        _list = new List<T>(capacity);
        _isCreated = true;
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public void Add(T item)
    {
        _list.Add(item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public void RemoveAtSwapBack(int index)
    {
        if (_list != null && index >= 0 && index < _list.Count)
        {
            if (index != _list.Count - 1)
            {
                _list[index] = _list[_list.Count - 1];
            }
            _list.RemoveAt(_list.Count - 1);
        }
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void Dispose()
    {
        _list.Clear();
        _isCreated = false;
    }

    public List<T>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public T[] ToArray()
    {
        return _list.ToArray() ?? Array.Empty<T>();
    }

    public void AddRange(NativeList<T> other)
    {
        if (_list != null && other._list != null)
        {
            _list.AddRange(other._list);
        }
    }

    public NativeArray<T>.ReadOnly AsReadOnly()
    {
        return new NativeArray<T>.ReadOnly(_list?.ToArray());
    }
}
