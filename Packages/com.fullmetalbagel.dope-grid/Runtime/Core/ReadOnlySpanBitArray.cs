using System;

namespace DopeGrid;

public readonly ref struct ReadOnlySpanBitArray
{
    public ReadOnlySpan<byte> Bytes { get; }
    public int BitLength { get; }
    public bool IsEmpty => BitLength == 0;

    public ReadOnlySpanBitArray(ReadOnlySpan<byte> bytes, int bitLength)
    {
        if (bitLength < 0)
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Length cannot be negative.");

        var byteCount = SpanBitArrayUtility.ByteCount(bitLength);
        if (byteCount > bytes.Length)
            throw new ArgumentException("Provided span is too small for the requested length.", nameof(bytes));

        Bytes = bytes[..byteCount];
        BitLength = bitLength;
    }

    public bool Get(int index)
    {
        SpanBitArrayUtility.ValidateIndex(BitLength, index);
        var byteIndex = index >> 3;
        var bitIndex = index & 7;
        return (Bytes[byteIndex] & (1 << bitIndex)) != 0;
    }

    public ulong GetBits(int index, int bitCount = 1)
    {
        SpanBitArrayUtility.ValidateLimitedRange(BitLength, index, bitCount);

        if (bitCount == 0)
            return 0;

        ulong value = 0;
        IterateBitRange(index, bitCount, ref value, static (ref ulong state, byte byteValue, byte mask, int offset, int shift) =>
        {
            var chunk = (byte)((byteValue >> offset) & (mask >> offset));
            state |= (ulong)chunk << shift;
            return true;
        });
        return value;
    }

    public bool TestAny(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return false;

        var result = false;
        IterateBitRange(index, bitCount, ref result, static (ref bool state, byte byteValue, byte mask, int offset, int _) =>
        {
            if ((byteValue & mask) != 0)
            {
                state = true;
                return false; // Early exit
            }
            return true;
        });
        return result;
    }

    public bool TestAll(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return true;

        var result = true;
        IterateBitRange(index, bitCount, ref result, static (ref bool state, byte byteValue, byte mask, int offset, int _) =>
        {
            if ((byteValue & mask) != mask)
            {
                state = false;
                return false; // Early exit
            }
            return true;
        });
        return result;
    }

    public int CountBits(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return 0;

        var total = 0;
        IterateBitRange(index, bitCount, ref total, static (ref int state, byte byteValue, byte mask, int offset, int _) =>
        {
            var data = (byteValue & mask) >> offset;
            state += SpanBitArrayUtility.PopCount((ulong)data);
            return true;
        });
        return total;
    }

    public bool SequenceEqual(ReadOnlySpanBitArray other)
    {
        if (BitLength != other.BitLength)
            return false;

        if (BitLength == 0)
            return true;

        var fullBytes = BitLength / SpanBitArrayUtility.BitsPerByte;
        if (fullBytes > 0)
        {
            if (!Bytes[..fullBytes].SequenceEqual(other.Bytes[..fullBytes]))
                return false;
        }

        var remainingBits = BitLength % SpanBitArrayUtility.BitsPerByte;
        if (remainingBits == 0)
            return true;

        var mask = (byte)((1 << remainingBits) - 1);
        return (Bytes[fullBytes] & mask) == (other.Bytes[fullBytes] & mask);
    }

    public void CopyTo(SpanBitArray other)
    {
        CopyTo(other, 0, 0, BitLength);
    }

    public void CopyTo(SpanBitArray other, int destIndex, int sourceIndex, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, sourceIndex, bitCount);
        SpanBitArrayUtility.ValidateRange(other.BitLength, destIndex, bitCount);

        if (bitCount == 0)
            return;

        var srcIndex = sourceIndex;
        var dstIndex = destIndex;
        var bitsLeft = bitCount;

        // Align destination to byte boundary to enable bulk copying
        var destBitOffset = dstIndex & 7;
        if (destBitOffset != 0)
        {
            var prefix = Math.Min(8 - destBitOffset, bitsLeft);
            var prefixValue = GetBits(srcIndex, prefix);
            other.SetBits(dstIndex, prefixValue, prefix);

            srcIndex += prefix;
            dstIndex += prefix;
            bitsLeft -= prefix;
        }

        if (bitsLeft == 0)
            return;

        // Copy whole bytes while possible
        if (bitsLeft >= 8)
        {
            var srcBitOffset = srcIndex & 7;
            var srcByteIndex = srcIndex >> 3;
            var destByteIndex = dstIndex >> 3;
            var fullBytes = bitsLeft >> 3;

            var destSlice = other.Bytes.Slice(destByteIndex, fullBytes);

            if (srcBitOffset == 0)
            {
                Bytes.Slice(srcByteIndex, fullBytes).CopyTo(destSlice);
            }
            else
            {
                var shiftLeft = 8 - srcBitOffset;
                for (var i = 0; i < fullBytes; i++)
                {
                    var current = Bytes[srcByteIndex + i];
                    var nextIndex = srcByteIndex + i + 1;
                    var next = nextIndex < Bytes.Length ? Bytes[nextIndex] : (byte)0;
                    destSlice[i] = (byte)((current >> srcBitOffset) | (next << shiftLeft));
                }
            }

            var bitsCopied = fullBytes << 3;
            srcIndex += bitsCopied;
            dstIndex += bitsCopied;
            bitsLeft -= bitsCopied;
        }

        if (bitsLeft == 0)
            return;

        // Copy any remaining bits with bit-level helper
        var tail = GetBits(srcIndex, bitsLeft);
        other.SetBits(dstIndex, tail, bitsLeft);
    }

    private delegate bool BitRangeIterator<TState>(ref TState state, byte byteValue, byte mask, int bitOffset, int shift);

    private void IterateBitRange<TState>(int index, int bitCount, ref TState state, BitRangeIterator<TState> action)
    {
        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;
        var shift = 0;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);

            if (!action(ref state, Bytes[byteIndex], mask, bitOffset, shift))
                return;

            remaining -= bits;
            shift += bits;
            byteIndex++;
            bitOffset = 0;
        }
    }
}
