using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class SpanBitArrayTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesArray()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 10);

        Assert.That(bitArray.BitLength, Is.EqualTo(10));
        Assert.That(bitArray.IsEmpty, Is.False);
    }

    [Test]
    public void Constructor_WithZeroLength_CreatesEmptyArray()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 0);

        Assert.That(bitArray.BitLength, Is.EqualTo(0));
        Assert.That(bitArray.IsEmpty, Is.True);
    }

    [Test]
    public void Constructor_WithNegativeLength_ThrowsException()
    {
        var bytes = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpanBitArray(bytes, -1));
    }

    [Test]
    public void Constructor_WithInsufficientBytes_ThrowsException()
    {
        var bytes = new byte[1];
        Assert.Throws<ArgumentException>(() => new SpanBitArray(bytes, 16));
    }

    [Test]
    public void Set_ValidIndex_SetsCorrectBit()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 8);

        bitArray.Set(3, true);

        Assert.That(bitArray.Get(3), Is.True);
        Assert.That(bitArray.Get(2), Is.False);
        Assert.That(bitArray.Get(4), Is.False);
    }

    [Test]
    public void Set_MultipleBits_SetsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.Set(0, true);
        bitArray.Set(7, true);
        bitArray.Set(8, true);
        bitArray.Set(15, true);

        Assert.That(bitArray.Get(0), Is.True);
        Assert.That(bitArray.Get(7), Is.True);
        Assert.That(bitArray.Get(8), Is.True);
        Assert.That(bitArray.Get(15), Is.True);
    }

    [Test]
    public void Set_ToFalse_ClearsBit()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes.Fill(0xFF);
        var bitArray = new SpanBitArray(bytes, 8);

        bitArray.Set(3, false);

        Assert.That(bitArray.Get(3), Is.False);
        Assert.That(bitArray.Get(2), Is.True);
    }

    [Test]
    public void Clear_EmptiesAllBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Fill(0xFF);
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.Clear();

        for (int i = 0; i < 16; i++)
        {
            Assert.That(bitArray.Get(i), Is.False, $"Bit {i} should be false");
        }
    }

    [Test]
    public void SetAll_True_SetsAllBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetAll(true);

        for (int i = 0; i < 16; i++)
        {
            Assert.That(bitArray.Get(i), Is.True, $"Bit {i} should be true");
        }
    }

    [Test]
    public void SetAll_False_ClearsAllBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Fill(0xFF);
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetAll(false);

        for (int i = 0; i < 16; i++)
        {
            Assert.That(bitArray.Get(i), Is.False, $"Bit {i} should be false");
        }
    }

    [Test]
    public void SetRange_ValidRange_SetsCorrectBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetRange(3, 5, true);

        for (int i = 0; i < 16; i++)
        {
            if (i >= 3 && i < 8)
                Assert.That(bitArray.Get(i), Is.True, $"Bit {i} should be true");
            else
                Assert.That(bitArray.Get(i), Is.False, $"Bit {i} should be false");
        }
    }

    [Test]
    public void SetRange_SpanningBytes_SetsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetRange(6, 6, true);

        for (int i = 0; i < 16; i++)
        {
            if (i >= 6 && i < 12)
                Assert.That(bitArray.Get(i), Is.True, $"Bit {i} should be true");
            else
                Assert.That(bitArray.Get(i), Is.False, $"Bit {i} should be false");
        }
    }

    [Test]
    public void SetBits_SingleByte_SetsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetBits(0, 0b10110, 5);

        Assert.That(bitArray.GetBits(0, 5), Is.EqualTo(0b10110));
    }

    [Test]
    public void SetBits_SpanningBytes_SetsCorrectValue()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetBits(6, 0b11010110, 8);

        Assert.That(bitArray.GetBits(6, 8), Is.EqualTo(0b11010110));
    }

    [Test]
    public void Inverse_InvertsBits()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 8);
        bitArray.Set(0, true);
        bitArray.Set(3, true);

        bitArray.Inverse();

        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(1), Is.True);
        Assert.That(bitArray.Get(2), Is.True);
        Assert.That(bitArray.Get(3), Is.False);
    }

    [Test]
    public void Inverse_WithRange_InvertsSpecificBits()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 8);
        bitArray.Set(2, true);
        bitArray.Set(5, true);

        bitArray.Inverse(1, 4);

        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(1), Is.True);
        Assert.That(bitArray.Get(2), Is.False);
        Assert.That(bitArray.Get(3), Is.True);
        Assert.That(bitArray.Get(4), Is.True);
        Assert.That(bitArray.Get(5), Is.True);
    }

    [Test]
    public void SequenceEqual_SameContent_ReturnsTrue()
    {
        Span<byte> bytes1 = stackalloc byte[2];
        Span<byte> bytes2 = stackalloc byte[2];
        bytes1.Clear();
        bytes2.Clear();

        var bitArray1 = new SpanBitArray(bytes1, 10);
        var bitArray2 = new SpanBitArray(bytes2, 10);

        bitArray1.Set(3, true);
        bitArray2.Set(3, true);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.True);
    }

    [Test]
    public void SequenceEqual_DifferentContent_ReturnsFalse()
    {
        Span<byte> bytes1 = stackalloc byte[2];
        Span<byte> bytes2 = stackalloc byte[2];
        bytes1.Clear();
        bytes2.Clear();

        var bitArray1 = new SpanBitArray(bytes1, 10);
        var bitArray2 = new SpanBitArray(bytes2, 10);

        bitArray1.Set(3, true);
        bitArray2.Set(4, true);

        Assert.That(bitArray1.SequenceEqual(bitArray2), Is.False);
    }

    [Test]
    public void CopyTo_CopiesAllBits()
    {
        Span<byte> source = stackalloc byte[2];
        Span<byte> dest = stackalloc byte[2];
        source.Clear();
        dest.Clear();

        var sourceArray = new SpanBitArray(source, 12);
        var destArray = new SpanBitArray(dest, 12);

        sourceArray.Set(0, true);
        sourceArray.Set(5, true);
        sourceArray.Set(11, true);

        sourceArray.CopyTo(destArray);

        Assert.That(destArray.Get(0), Is.True);
        Assert.That(destArray.Get(5), Is.True);
        Assert.That(destArray.Get(11), Is.True);
        Assert.That(destArray.Get(3), Is.False);
    }

    [Test]
    public void CopyTo_WithIndexes_CopiesPartialBits()
    {
        Span<byte> source = stackalloc byte[3];
        Span<byte> dest = stackalloc byte[3];
        source.Clear();
        dest.Clear();

        var sourceArray = new SpanBitArray(source, 20);
        var destArray = new SpanBitArray(dest, 20);

        sourceArray.SetRange(5, 8, true);

        sourceArray.CopyTo(destArray, 10, 5, 8);

        for (int i = 0; i < 20; i++)
        {
            if (i >= 10 && i < 18)
                Assert.That(destArray.Get(i), Is.True, $"Bit {i} should be true");
            else
                Assert.That(destArray.Get(i), Is.False, $"Bit {i} should be false");
        }
    }

    [Test]
    public void ImplicitConversion_ToReadOnly_Works()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 8);
        bitArray.Set(3, true);

        ReadOnlySpanBitArray readOnly = bitArray;

        Assert.That(readOnly.Get(3), Is.True);
    }

    [Test]
    public void SetBits_LargeValue_SetsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[4];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 32);

        bitArray.SetBits(0, 0xABCD, 16);

        Assert.That(bitArray.GetBits(0, 16), Is.EqualTo(0xABCD));
    }

    [Test]
    public void Clear_WithPartialByte_ClearsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Fill(0xFF);
        var bitArray = new SpanBitArray(bytes, 12);

        bitArray.Clear();

        for (int i = 0; i < 12; i++)
        {
            Assert.That(bitArray.Get(i), Is.False);
        }
    }

    [Test]
    public void Inverse_WithMultipleBytes_InvertsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Clear();
        var bitArray = new SpanBitArray(bytes, 16);
        bitArray.Set(0, true);
        bitArray.Set(15, true);

        bitArray.Inverse();

        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(1), Is.True);
        Assert.That(bitArray.Get(14), Is.True);
        Assert.That(bitArray.Get(15), Is.False);
    }

    [Test]
    public void SetRange_ToFalse_ClearsRange()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes.Fill(0xFF);
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetRange(4, 8, false);

        for (int i = 0; i < 16; i++)
        {
            if (i >= 4 && i < 12)
                Assert.That(bitArray.Get(i), Is.False);
            else
                Assert.That(bitArray.Get(i), Is.True);
        }
    }
}
