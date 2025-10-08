using System;

namespace DopeGrid;

public readonly ref struct SpanBitArray
{
    public Span<byte> Bytes { get; }
    public int BitLength { get; }
    public bool IsEmpty => BitLength == 0;

    public SpanBitArray(Span<byte> bytes) : this(bytes, bytes.Length * 8) { }

    public SpanBitArray(Span<byte> bytes, int bitLength)
    {
        if (bitLength < 0)
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Length cannot be negative.");

        var byteCount = SpanBitArrayUtility.ByteCount(bitLength);
        if (byteCount > bytes.Length)
            throw new ArgumentException("Provided span is too small for the requested length.", nameof(bytes));

        Bytes = bytes[..byteCount];
        BitLength = bitLength;
    }

    public static implicit operator ReadOnlySpanBitArray(SpanBitArray bits) => bits.AsReadOnly();
    public ReadOnlySpanBitArray AsReadOnly() => new(Bytes, BitLength);

    public bool Get(int index) => AsReadOnly().Get(index);
    public ulong GetBits(int index, int bitCount = 1) => AsReadOnly().GetBits(index, bitCount);
    public bool TestAny(int index, int bitCount) => AsReadOnly().TestAny(index, bitCount);
    public bool TestAll(int index, int bitCount) => AsReadOnly().TestAll(index, bitCount);
    public int CountBits(int index, int bitCount) => AsReadOnly().CountBits(index, bitCount);
    public bool SequenceEqual(SpanBitArray other) => AsReadOnly().SequenceEqual(other.AsReadOnly());
    public bool SequenceEqual(ReadOnlySpanBitArray other) => AsReadOnly().SequenceEqual(other);
    public void CopyTo(SpanBitArray other) => AsReadOnly().CopyTo(other);
    public void CopyTo(SpanBitArray other, int destIndex, int sourceIndex, int bitCount) => AsReadOnly().CopyTo(other, destIndex, sourceIndex, bitCount);

    public void Set(int index, bool value)
    {
        SpanBitArrayUtility.ValidateIndex(BitLength, index);
        var byteIndex = index >> 3;
        var bitIndex = index & 7;
        var mask = (byte)(1 << bitIndex);
        ref var b = ref Bytes[byteIndex];
        b = value ? (byte)(b | mask) : (byte)(b & ~mask);
    }

    public void Clear()
    {
        if (!IsEmpty) Bytes.Clear();
    }

    public void SetAll(bool value)
    {
        SetRange(0, BitLength, value);
    }

    public void SetRange(int index, int bitCount, bool value)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            ref var b = ref Bytes[byteIndex];
            b = value ? (byte)(b | mask) : (byte)(b & ~mask);

            remaining -= bits;
            byteIndex++;
            bitOffset = 0;
        }
    }

    public void SetBits(int index, ulong value, int bitCount)
    {
        SpanBitArrayUtility.ValidateLimitedRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;
        var valueShift = 0;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            var chunk = (byte)((value >> valueShift) & ((1UL << bits) - 1));
            ref var b = ref Bytes[byteIndex];
            b = (byte)((b & ~mask) | ((chunk << bitOffset) & mask));

            remaining -= bits;
            valueShift += bits;
            byteIndex++;
            bitOffset = 0;
        }
    }

    public void Inverse()
    {
        Inverse(0, BitLength);
    }

    public void Inverse(int index, int bitCount = 1)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            ref var b = ref Bytes[byteIndex];
            b ^= mask;

            remaining -= bits;
            byteIndex++;
            bitOffset = 0;
        }
    }
}
