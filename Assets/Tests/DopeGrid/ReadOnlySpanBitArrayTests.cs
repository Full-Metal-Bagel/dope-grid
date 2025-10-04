using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class ReadOnlySpanBitArrayTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesArray()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new ReadOnlySpanBitArray(bytes, 10);

        Assert.That(bitArray.BitLength, Is.EqualTo(10));
        Assert.That(bitArray.IsEmpty, Is.False);
    }

    [Test]
    public void Constructor_WithZeroLength_CreatesEmptyArray()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new ReadOnlySpanBitArray(bytes, 0);

        Assert.That(bitArray.BitLength, Is.EqualTo(0));
        Assert.That(bitArray.IsEmpty, Is.True);
    }

    [Test]
    public void Constructor_WithNegativeLength_ThrowsException()
    {
        var bytes = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpanBitArray(bytes, -1));
    }

    [Test]
    public void Constructor_WithInsufficientBytes_ThrowsException()
    {
        var bytes = new byte[1];
        Assert.Throws<ArgumentException>(() => new ReadOnlySpanBitArray(bytes, 16));
    }

    [Test]
    public void Get_ReturnsCorrectBitValue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00001010;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.Get(1), Is.True);
        Assert.That(bitArray.Get(3), Is.True);
        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(2), Is.False);
    }

    [Test]
    public void Get_AcrossMultipleBytes_ReturnsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0b10000000;
        bytes[1] = 0b00000001;
        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.Get(7), Is.True);
        Assert.That(bitArray.Get(8), Is.True);
        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(15), Is.False);
    }

    [Test]
    public void GetBits_SingleBit_ReturnsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00001010;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.GetBits(1, 1), Is.EqualTo(1));
        Assert.That(bitArray.GetBits(0, 1), Is.EqualTo(0));
    }

    [Test]
    public void GetBits_MultipleBits_ReturnsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b10110100;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.GetBits(2, 4), Is.EqualTo(0b1101));
        Assert.That(bitArray.GetBits(0, 3), Is.EqualTo(0b100));
    }

    [Test]
    public void GetBits_SpanningBytes_ReturnsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0b11000000;
        bytes[1] = 0b00000011;
        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.GetBits(6, 4), Is.EqualTo(0b1111));
    }

    [Test]
    public void TestAny_WithMatchingBits_ReturnsTrue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00001000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAny(0, 8), Is.True);
        Assert.That(bitArray.TestAny(3, 1), Is.True);
    }

    [Test]
    public void TestAny_WithoutMatchingBits_ReturnsFalse()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00001000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAny(0, 3), Is.False);
        Assert.That(bitArray.TestAny(4, 4), Is.False);
    }

    [Test]
    public void TestAny_ZeroBitCount_ReturnsFalse()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0xFF;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAny(0, 0), Is.False);
    }

    [Test]
    public void TestAll_WithAllBitsSet_ReturnsTrue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00111000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAll(3, 3), Is.True);
    }

    [Test]
    public void TestAll_WithSomeBitsUnset_ReturnsFalse()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b00101000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAll(3, 3), Is.False);
    }

    [Test]
    public void TestAll_ZeroBitCount_ReturnsTrue()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0x00;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAll(0, 0), Is.True);
    }

    [Test]
    public void CountBits_ReturnsCorrectCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b01010101;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.CountBits(0, 8), Is.EqualTo(4));
    }

    [Test]
    public void CountBits_PartialRange_ReturnsCorrectCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b11110000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.CountBits(4, 4), Is.EqualTo(4));
        Assert.That(bitArray.CountBits(0, 4), Is.EqualTo(0));
    }

    [Test]
    public void CountBits_SpanningBytes_ReturnsCorrectCount()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0b11000000;
        bytes[1] = 0b00000011;
        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.CountBits(6, 4), Is.EqualTo(4));
    }

    [Test]
    public void SequenceEqual_SameContent_ReturnsTrue()
    {
        Span<byte> bytes1 = stackalloc byte[2];
        Span<byte> bytes2 = stackalloc byte[2];
        bytes1[0] = 0b10101010;
        bytes1[1] = 0b01010101;
        bytes2[0] = 0b10101010;
        bytes2[1] = 0b01010101;

        var bitArray1 = new ReadOnlySpanBitArray(bytes1, 16);
        var bitArray2 = new ReadOnlySpanBitArray(bytes2, 16);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.True);
    }

    [Test]
    public void SequenceEqual_DifferentContent_ReturnsFalse()
    {
        Span<byte> bytes1 = stackalloc byte[1];
        Span<byte> bytes2 = stackalloc byte[1];
        bytes1[0] = 0b10101010;
        bytes2[0] = 0b01010101;

        var bitArray1 = new ReadOnlySpanBitArray(bytes1, 8);
        var bitArray2 = new ReadOnlySpanBitArray(bytes2, 8);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.False);
    }

    [Test]
    public void SequenceEqual_DifferentLength_ReturnsFalse()
    {
        Span<byte> bytes1 = stackalloc byte[2];
        Span<byte> bytes2 = stackalloc byte[2];

        var bitArray1 = new ReadOnlySpanBitArray(bytes1, 10);
        var bitArray2 = new ReadOnlySpanBitArray(bytes2, 12);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.False);
    }

    [Test]
    public void SequenceEqual_WithPartialByte_ComparesCorrectly()
    {
        Span<byte> bytes1 = stackalloc byte[1];
        Span<byte> bytes2 = stackalloc byte[1];
        bytes1[0] = 0b00010101;
        bytes2[0] = 0b00010101;

        var bitArray1 = new ReadOnlySpanBitArray(bytes1, 5);
        var bitArray2 = new ReadOnlySpanBitArray(bytes2, 5);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.True);
    }

    [Test]
    public void CopyTo_CopiesAllBits()
    {
        Span<byte> source = stackalloc byte[2];
        Span<byte> dest = stackalloc byte[2];
        source[0] = 0b10101010;
        source[1] = 0b01010101;
        dest.Clear();

        var sourceArray = new ReadOnlySpanBitArray(source, 16);
        var destArray = new SpanBitArray(dest, 16);

        sourceArray.CopyTo(destArray);

        Assert.That(dest[0], Is.EqualTo(0b10101010));
        Assert.That(dest[1], Is.EqualTo(0b01010101));
    }

    [Test]
    public void CopyTo_WithIndexes_CopiesPartialBits()
    {
        Span<byte> source = stackalloc byte[2];
        Span<byte> dest = stackalloc byte[2];
        source[0] = 0b11111111;
        source[1] = 0b11111111;
        dest.Clear();

        var sourceArray = new ReadOnlySpanBitArray(source, 16);
        var destArray = new SpanBitArray(dest, 16);

        sourceArray.CopyTo(destArray, 4, 2, 8);

        Assert.That(destArray.GetBits(4, 8), Is.EqualTo(0b11111111));
        Assert.That(destArray.GetBits(0, 4), Is.EqualTo(0));
        Assert.That(destArray.GetBits(12, 4), Is.EqualTo(0));
    }

    [Test]
    public void CopyTo_UnalignedCopy_WorksCorrectly()
    {
        Span<byte> source = stackalloc byte[2];
        Span<byte> dest = stackalloc byte[2];
        source[0] = 0b11110000;
        source[1] = 0b00001111;
        dest.Clear();

        var sourceArray = new ReadOnlySpanBitArray(source, 16);
        var destArray = new SpanBitArray(dest, 16);

        sourceArray.CopyTo(destArray, 3, 4, 8);

        Assert.That(destArray.GetBits(3, 8), Is.EqualTo(sourceArray.GetBits(4, 8)));
    }
}
