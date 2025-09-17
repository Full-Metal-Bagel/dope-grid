using System;
using System.Runtime.InteropServices;

namespace DopeGrid;

public readonly ref struct ReadOnlySpanBitArray
{
    private readonly ReadOnlySpan<ulong> _words;

    public int Length { get; }
    public int WordCount => SpanBitArrayUtility.WordCount(Length);
    public int Capacity => _words.Length * SpanBitArrayUtility.BitsPerWord;
    public bool IsEmpty => Length == 0;

    public ReadOnlySpanBitArray(ReadOnlySpan<ulong> words, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");

        if (length > words.Length * SpanBitArrayUtility.BitsPerWord)
            throw new ArgumentException("Provided span is too small for the requested length.", nameof(words));

        _words = words;
        Length = length;
    }

    public ReadOnlySpanBitArray(ReadOnlySpan<byte> bytes, int length)
    {
        if ((bytes.Length & (sizeof(ulong) - 1)) != 0)
            throw new ArgumentException("Byte span length must be a multiple of sizeof(ulong).", nameof(bytes));

        var words = MemoryMarshal.Cast<byte, ulong>(bytes);

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");

        if (length > words.Length * SpanBitArrayUtility.BitsPerWord)
            throw new ArgumentException("Provided span is too small for the requested length.", nameof(bytes));

        _words = words;
        Length = length;
    }

    public ReadOnlySpan<ulong> Words => _words.Slice(0, WordCount);
    public ReadOnlySpan<byte> AsBytes() => MemoryMarshal.AsBytes(Words);
    public ReadOnlySpan<byte> AsTrimmedBytes() => AsBytes().Slice(0, (Length + 7) / 8);

    public bool Get(int index)
    {
        SpanBitArrayUtility.ValidateIndex(Length, index);
        var wordIndex = index >> 6;
        var bitIndex = index & 63;
        return (_words[wordIndex] & (1UL << bitIndex)) != 0;
    }

    public ulong GetBits(int index, int bitCount = 1)
    {
        SpanBitArrayUtility.ValidateLimitedRange(Length, index, bitCount);

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var value = _words[wordIndex] >> bitOffset;

        var bitsInFirstWord = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, bitCount);
        var remaining = bitCount - bitsInFirstWord;

        if (remaining > 0)
        {
            value |= _words[wordIndex + 1] << bitsInFirstWord;
        }

        return bitCount == SpanBitArrayUtility.BitsPerWord ? value : value & SpanBitArrayUtility.Mask(bitCount);
    }

    public bool TestAny(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(Length, index, bitCount);
        if (bitCount == 0)
            return false;

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, remaining);
            var mask = SpanBitArrayUtility.Mask(bits) << bitOffset;
            if ((_words[wordIndex] & mask) != 0)
                return true;

            remaining -= bits;
            wordIndex++;
            bitOffset = 0;
        }

        return false;
    }

    public bool TestAll(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(Length, index, bitCount);
        if (bitCount == 0)
            return true;

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, remaining);
            var mask = SpanBitArrayUtility.Mask(bits) << bitOffset;
            if ((_words[wordIndex] & mask) != mask)
                return false;

            remaining -= bits;
            wordIndex++;
            bitOffset = 0;
        }

        return true;
    }

    public int CountBits(int index, int bitCount)
    {
        SpanBitArrayUtility.ValidateRange(Length, index, bitCount);
        if (bitCount == 0)
            return 0;

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var remaining = bitCount;
        var total = 0;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, remaining);
            var mask = SpanBitArrayUtility.Mask(bits) << bitOffset;
            var data = (_words[wordIndex] & mask) >> bitOffset;
            total += SpanBitArrayUtility.PopCount(data);

            remaining -= bits;
            wordIndex++;
            bitOffset = 0;
        }

        return total;
    }

    public void CopyTo(Span<byte> destination)
    {
        var byteCount = (Length + 7) / 8;
        if (destination.Length < byteCount)
            throw new ArgumentException("Destination span is too small.", nameof(destination));

        if (byteCount == 0)
            return;

        AsBytes()[..byteCount].CopyTo(destination);
    }

    public void CopyTo(SpanBitArray destination)
    {
        destination.CopyFrom(this);
    }
}
