using System;
using System.Collections;

namespace Unity.Collections;

public struct NativeBitArray : IDisposable
{
    private BitArray? _bits;
    private bool _isCreated;

    public int Length => _bits?.Length ?? 0;
    public bool IsCreated => _isCreated;

    public NativeBitArray(int length, Allocator allocator)
    {
        _bits = new BitArray(length, false);
        _isCreated = true;
    }

    public void Set(int index, bool value)
    {
        if (_bits != null && index >= 0 && index < _bits.Length)
        {
            _bits[index] = value;
        }
    }

    public bool IsSet(int index)
    {
        if (_bits != null && index >= 0 && index < _bits.Length)
        {
            return _bits[index];
        }
        return false;
    }

    public void Clear()
    {
        _bits?.SetAll(false);
    }

    public int CountBits(int start, int count)
    {
        if (_bits == null) return 0;
        
        var setBits = 0;
        var end = Math.Min(start + count, _bits.Length);
        
        for (var i = start; i < end; i++)
        {
            if (_bits[i])
            {
                setBits++;
            }
        }
        
        return setBits;
    }

    public void Dispose()
    {
        _bits = null;
        _isCreated = false;
    }
}
