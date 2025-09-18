using System;

namespace DopeGrid;

public static class SpanBitArrayUtility
{
    internal const int BitsPerChunk = 64;
    internal const int BitsPerByte = 8;

    public static int ByteCount(int bitLength)
    {
        return bitLength <= 0 ? 0 : (bitLength + BitsPerByte - 1) / BitsPerByte;
    }

    internal static void ValidateIndex(int length, int index)
    {
        if ((uint)index >= (uint)length)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    internal static void ValidateRange(int length, int index, int bitCount)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (bitCount < 0)
            throw new ArgumentOutOfRangeException(nameof(bitCount));

        if (index > length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (bitCount == 0)
            return;

        if ((long)index + bitCount > length)
            throw new ArgumentOutOfRangeException(nameof(bitCount));
    }

    internal static void ValidateLimitedRange(int length, int index, int bitCount)
    {
        if (bitCount <= 0 || bitCount > BitsPerChunk)
            throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 64.");

        ValidateRange(length, index, bitCount);
    }

    internal static int PopCount(ulong value)
    {
        value -= (value >> 1) & 0x5555555555555555UL;
        value = (value & 0x3333333333333333UL) + ((value >> 2) & 0x3333333333333333UL);
        value = (value + (value >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
        return (int)((value * 0x0101010101010101UL) >> 56);
    }
}
