using System;

namespace Unity.Collections;

public struct NativeArray<T> : IDisposable where T : struct
{
    private T[] _array = Array.Empty<T>();
    private bool _isCreated;

    public int Length => _array.Length;
    public bool IsCreated => _isCreated;

    public NativeArray(int length, Allocator allocator)
    {
        _array = new T[length];
        _isCreated = true;
    }

    public NativeArray(T[] array, Allocator allocator)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        _array = new T[array.Length];
        Array.Copy(array, _array, array.Length);
        _isCreated = true;
    }

    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public void Dispose()
    {
        _array = Array.Empty<T>();
        _isCreated = false;
    }

    public T[] ToArray()
    {
        if (_array == null)
        {
            return Array.Empty<T>();
        }
        var copy = new T[_array.Length];
        Array.Copy(_array, copy, _array.Length);
        return copy;
    }

    public void CopyTo(T[] array)
    {
        _array.CopyTo(array, 0);
    }

    public void CopyTo(NativeArray<T> array)
    {
        if (_array != null && array._array != null)
        {
            Array.Copy(_array, array._array, Math.Min(_array.Length, array._array.Length));
        }
    }
}
