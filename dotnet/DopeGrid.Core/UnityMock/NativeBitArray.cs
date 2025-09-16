using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[SuppressMessage("Design", "CA1024:Use properties where appropriate")]
public struct NativeBitArray : IDisposable
{
    private BitArray? _bits;
    private bool _isCreated;

    public int Length => _bits?.Length ?? 0;
    public bool IsCreated => _isCreated;

    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
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

    public void Copy(int dstIndex, ref NativeBitArray srcBits, int srcIndex, int length)
    {
        if (_bits == null)
            throw new InvalidOperationException("Cannot copy to a non-created NativeBitArray");

        if (!srcBits.IsCreated || srcBits._bits == null)
            throw new InvalidOperationException("Cannot copy from a non-created NativeBitArray");

        if (dstIndex < 0 || dstIndex + length > _bits.Length)
            throw new ArgumentOutOfRangeException(nameof(dstIndex), "Destination range is out of bounds");

        if (srcIndex < 0 || srcIndex + length > srcBits._bits.Length)
            throw new ArgumentOutOfRangeException(nameof(srcIndex), "Source range is out of bounds");

        for (int i = 0; i < length; i++)
        {
            _bits[dstIndex + i] = srcBits._bits[srcIndex + i];
        }
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

    public UnsafeBitArray.ReadOnly GetReadOnlyUnsafeBitArray()
    {
        return new UnsafeBitArray.ReadOnly(_bits);
    }

    public ReadOnly AsReadOnly()
    {
        return new ReadOnly(_bits);
    }

    public int CalculateHashCode()
    {
        return GetReadOnlyUnsafeBitArray().CalculateHashCode();
    }

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public readonly ref struct ReadOnly
    {
        private readonly BitArray? _bits;

        public ReadOnly(BitArray? bits)
        {
            _bits = bits;
        }

        public bool IsSet(int index)
        {
            if (_bits != null && index >= 0 && index < _bits.Length)
            {
                return _bits[index];
            }
            return false;
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

        public UnsafeBitArray.ReadOnly GetReadOnlyUnsafeBitArray()
        {
            return new UnsafeBitArray.ReadOnly(_bits);
        }

        public int CalculateHashCode()
        {
            return GetReadOnlyUnsafeBitArray().CalculateHashCode();
        }
    }
}
