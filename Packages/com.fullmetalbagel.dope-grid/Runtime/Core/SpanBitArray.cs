using System;
using System.Runtime.InteropServices;

namespace DopeGrid;

public ref struct SpanBitArray
{
    private readonly Span<ulong> _words;

    public int Length { get; }
    public int WordCount => SpanBitArrayUtility.WordCount(Length);
    public int Capacity => _words.Length * SpanBitArrayUtility.BitsPerWord;
    public bool IsEmpty => Length == 0;

    public SpanBitArray(Span<ulong> words, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");

        if (length > words.Length * SpanBitArrayUtility.BitsPerWord)
            throw new ArgumentException("Provided span is too small for the requested length.", nameof(words));

        _words = words;
        Length = length;
    }

    public SpanBitArray(Span<byte> bytes, int length)
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

    public Span<ulong> Words => _words.Slice(0, WordCount);
    public Span<byte> AsBytes() => MemoryMarshal.AsBytes(Words);
    public Span<byte> AsTrimmedBytes() => AsBytes().Slice(0, (Length + 7) / 8);
    public ReadOnlySpanBitArray AsReadOnly() => new ReadOnlySpanBitArray(_words, Length);

    public bool Get(int index) => AsReadOnly().Get(index);
    public ulong GetBits(int index, int bitCount = 1) => AsReadOnly().GetBits(index, bitCount);
    public bool TestAny(int index, int bitCount) => AsReadOnly().TestAny(index, bitCount);
    public bool TestAll(int index, int bitCount) => AsReadOnly().TestAll(index, bitCount);
    public int CountBits(int index, int bitCount) => AsReadOnly().CountBits(index, bitCount);

    public void Set(int index, bool value)
    {
        SpanBitArrayUtility.ValidateIndex(Length, index);
        var wordIndex = index >> 6;
        var bitIndex = index & 63;
        var mask = 1UL << bitIndex;
        ref var word = ref _words[wordIndex];
        word = value ? word | mask : word & ~mask;
    }

    public void Clear()
    {
        var active = Words;
        if (!active.IsEmpty)
            active.Clear();
    }

    public void SetAll(bool value)
    {
        SetRange(0, Length, value);
    }

    public void SetRange(int index, int bitCount, bool value)
    {
        SpanBitArrayUtility.ValidateRange(Length, index, bitCount);
        if (bitCount == 0)
            return;

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var remaining = bitCount;

        while (remaining > 0)
        {
            var bits = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, remaining);
            var mask = SpanBitArrayUtility.Mask(bits) << bitOffset;
            ref var word = ref _words[wordIndex];
            word = value ? word | mask : word & ~mask;

            remaining -= bits;
            wordIndex++;
            bitOffset = 0;
        }
    }

    public void SetBits(int index, ulong value, int bitCount)
    {
        SpanBitArrayUtility.ValidateLimitedRange(Length, index, bitCount);
        if (bitCount == 0)
            return;

        var wordIndex = index >> 6;
        var bitOffset = index & 63;
        var bitsInFirstWord = Math.Min(SpanBitArrayUtility.BitsPerWord - bitOffset, bitCount);

        var lowerMask = SpanBitArrayUtility.Mask(bitsInFirstWord) << bitOffset;
        var lowerValue = (value & SpanBitArrayUtility.Mask(bitsInFirstWord)) << bitOffset;
        ref var lowerWord = ref _words[wordIndex];
        lowerWord = (lowerWord & ~lowerMask) | lowerValue;

        var remaining = bitCount - bitsInFirstWord;
        if (remaining > 0)
        {
            var upperMask = SpanBitArrayUtility.Mask(remaining);
            var upperValue = (value >> bitsInFirstWord) & upperMask;
            ref var upperWord = ref _words[wordIndex + 1];
            upperWord = (upperWord & ~upperMask) | upperValue;
        }
    }

    public void CopyFrom(ReadOnlySpan<byte> source)
    {
        var byteCount = (Length + 7) / 8;
        if (source.Length < byteCount)
            throw new ArgumentException("Source span is too small.", nameof(source));

        if (byteCount == 0)
            return;

        source[..byteCount].CopyTo(AsBytes());
    }

    public void CopyFrom(ReadOnlySpanBitArray source)
    {
        if (source.Length != Length)
            throw new ArgumentException("Source must have the same length as destination.", nameof(source));

        var byteCount = (Length + 7) / 8;
        if (byteCount == 0)
            return;

        source.AsBytes()[..byteCount].CopyTo(AsBytes());
    }

    public void CopyTo(SpanBitArray destination) => AsReadOnly().CopyTo(destination);
    public void CopyTo(Span<byte> destination) => AsReadOnly().CopyTo(destination);
}
