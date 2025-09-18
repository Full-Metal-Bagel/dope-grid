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

    public ReadOnlySpan<byte> AsTrimmedBytes() => Bytes;

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

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;
        var shift = 0;
        ulong value = 0;

        while (remaining > 0)
        {
            var bitsAvailable = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)((1 << bitsAvailable) - 1);
            var chunk = (byte)((Bytes[byteIndex] >> bitOffset) & mask);
            value |= (ulong)chunk << shift;

            remaining -= bitsAvailable;
            shift += bitsAvailable;
            byteIndex++;
            bitOffset = 0;
        }

        return value;
    }

    public bool TestAny(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return false;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            if ((Bytes[byteIndex] & mask) != 0)
                return true;

            remaining -= bits;
            byteIndex++;
            bitOffset = 0;
        }

        return false;
    }

    public bool TestAll(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return true;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            if ((Bytes[byteIndex] & mask) != mask)
                return false;

            remaining -= bits;
            byteIndex++;
            bitOffset = 0;
        }

        return true;
    }

    public int CountBits(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, index, bitCount);
        if (bitCount == 0)
            return 0;

        var byteIndex = index >> 3;
        var bitOffset = index & 7;
        var remaining = bitCount;
        var total = 0;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerByte - bitOffset, remaining);
            var mask = (byte)(((1 << bits) - 1) << bitOffset);
            var data = (Bytes[byteIndex] & mask) >> bitOffset;
            total += SpanBitArrayUtility.PopCount((ulong)data);

            remaining -= bits;
            byteIndex++;
            bitOffset = 0;
        }

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

    public void CopyTo(SpanBitArray other, int destIndex, int sourceIndex, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(BitLength, sourceIndex, bitCount);
        SpanBitArrayUtility.ValidateRange(other.BitLength, destIndex, bitCount);

        if (bitCount == 0)
            return;

        var sourceBitOffset = sourceIndex & 7;
        var destBitOffset = destIndex & 7;

        // Fast path: fully byte-aligned copy
        if (sourceBitOffset == 0 && destBitOffset == 0 && (bitCount & 7) == 0)
        {
            var sourceByteIndex = sourceIndex >> 3;
            var destByteIndex = destIndex >> 3;
            var byteCount = bitCount >> 3;
            Bytes.Slice(sourceByteIndex, byteCount).CopyTo(other.Bytes.Slice(destByteIndex, byteCount));
            return;
        }

        // Fast path: same bit alignment - can copy bytes in the middle
        if (sourceBitOffset == destBitOffset && bitCount >= 8)
        {
            var sourceBitIndex = sourceIndex;
            var destBitIndex = destIndex;
            var remaining = bitCount;

            // Handle unaligned prefix bits (if any)
            if (sourceBitOffset != 0)
            {
                var prefixBits = Math.Min(8 - sourceBitOffset, remaining);
                var prefixValue = GetBits(sourceBitIndex, prefixBits);
                other.SetBits(destBitIndex, prefixValue, prefixBits);

                sourceBitIndex += prefixBits;
                destBitIndex += prefixBits;
                remaining -= prefixBits;
            }

            // Copy whole bytes in the middle
            if (remaining >= 8)
            {
                var byteCount = remaining >> 3;
                var sourceByteIndex = sourceBitIndex >> 3;
                var destByteIndex = destBitIndex >> 3;
                Bytes.Slice(sourceByteIndex, byteCount).CopyTo(other.Bytes.Slice(destByteIndex, byteCount));

                var bitsCopied = byteCount << 3;
                sourceBitIndex += bitsCopied;
                destBitIndex += bitsCopied;
                remaining -= bitsCopied;
            }

            // Handle remaining suffix bits (if any)
            if (remaining > 0)
            {
                var suffixValue = GetBits(sourceBitIndex, remaining);
                other.SetBits(destBitIndex, suffixValue, remaining);
            }

            return;
        }

        // General case: different alignments or small copies
        var srcIndex = sourceIndex;
        var dstIndex = destIndex;
        var bitsLeft = bitCount;

        // Process in chunks, optimizing for larger chunks when possible
        while (bitsLeft > 0)
        {
            // Try to process 64 bits at a time for better performance
            var chunkSize = Math.Min(bitsLeft, 64);

            // For smaller remaining bits, process them all at once
            if (bitsLeft <= 8)
            {
                chunkSize = bitsLeft;
            }

            var chunk = GetBits(srcIndex, chunkSize);
            other.SetBits(dstIndex, chunk, chunkSize);

            srcIndex += chunkSize;
            dstIndex += chunkSize;
            bitsLeft -= chunkSize;
        }
    }
}
