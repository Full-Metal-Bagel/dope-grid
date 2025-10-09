using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class UncoveredLinesTests
{
    // BoolGridShapeExtension line 186 - CopyTo wrapper
    [Test]
    public void BoolGridShapeExtension_CopyTo_Wrapper()
    {
        using var source = new GridShape(3, 3);
        source[0, 0] = true;
        source[1, 1] = true;
        source[2, 2] = true;

        using var target = new GridShape(3, 3);

        source.CopyTo(target);

        Assert.That(target[0, 0], Is.True);
        Assert.That(target[1, 1], Is.True);
        Assert.That(target[2, 2], Is.True);
    }

    // GridShapeExtension lines 19-25 - Full grid copy
    [Test]
    public void GridShapeExtension_CopyTo_FullGrid()
    {
        using var source = new ValueGridShape<int>(3, 3);
        source[0, 0] = 10;
        source[1, 1] = 20;
        source[2, 2] = 30;

        using var target = new ValueGridShape<int>(3, 3);

        source.WriteTo<ValueGridShape<int>, ValueGridShape<int>, int>(target);

        Assert.That(target[0, 0], Is.EqualTo(10));
        Assert.That(target[1, 1], Is.EqualTo(20));
        Assert.That(target[2, 2], Is.EqualTo(30));
    }

    // GridShapeExtension lines 30-32 - Copy with width/height
    [Test]
    public void GridShapeExtension_CopyTo_WithWidthHeight()
    {
        using var source = new ValueGridShape<int>(5, 5);
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
        {
            source[x, y] = x + y * 10;
        }

        using var target = new ValueGridShape<int>(3, 3);

        source.WriteTo<ValueGridShape<int>, ValueGridShape<int>, int>(3, 3, target);

        Assert.That(target[0, 0], Is.EqualTo(0));
        Assert.That(target[1, 1], Is.EqualTo(11));
        Assert.That(target[2, 2], Is.EqualTo(22));
    }

    // ReadOnlySpanBitArray line 120 - CountBits with bitCount = 0
    [Test]
    public void ReadOnlySpanBitArray_CountBits_ZeroBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0xFF;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        var result = bitArray.CountBits(0, 0);

        Assert.That(result, Is.EqualTo(0));
    }

    // ReadOnlySpanBitArray line 148 - SequenceEqual with empty arrays
    [Test]
    public void ReadOnlySpanBitArray_SequenceEqual_Empty()
    {
        Span<byte> bytes1 = stackalloc byte[0];
        Span<byte> bytes2 = stackalloc byte[0];
        var bitArray1 = new ReadOnlySpanBitArray(bytes1, 0);
        var bitArray2 = new ReadOnlySpanBitArray(bytes2, 0);

        var result = bitArray1.SequenceEqual(bitArray2);

        Assert.That(result, Is.True);
    }

    // ReadOnlySpanBitArray lines 213-222 - CopyTo with misaligned source
    [Test]
    public void ReadOnlySpanBitArray_CopyTo_MisalignedSource()
    {
        Span<byte> sourceBytes = stackalloc byte[3];
        sourceBytes[0] = 0b11111111;
        sourceBytes[1] = 0b00001111;
        sourceBytes[2] = 0b00000000;

        var source = new ReadOnlySpanBitArray(sourceBytes, 24);

        Span<byte> destBytes = stackalloc byte[2];
        var dest = new SpanBitArray(destBytes, 16);

        // Copy 12 bits starting from bit index 4 (misaligned)
        source.CopyTo(dest, 0, 4, 12);

        // Verify bits were copied correctly with alignment
        Assert.That(dest.Get(0), Is.True);
        Assert.That(dest.Get(1), Is.True);
        Assert.That(dest.Get(2), Is.True);
        Assert.That(dest.Get(3), Is.True);
    }

    // RotationDegreeExtensions line 25 - Invalid enum in GetNextClockwiseRotation
    [Test]
    public void RotationDegree_GetNextClockwiseRotation_InvalidEnum()
    {
        var invalidRotation = (RotationDegree)999;
        var result = invalidRotation.GetNextClockwiseRotation();
        Assert.That(result, Is.EqualTo(RotationDegree.Clockwise0));
    }

    // RotationDegreeExtensions line 38 - Invalid enum in GetPreviousClockwiseRotation
    [Test]
    public void RotationDegree_GetPreviousClockwiseRotation_InvalidEnum()
    {
        var invalidRotation = (RotationDegree)999;
        var result = invalidRotation.GetPreviousClockwiseRotation();
        Assert.That(result, Is.EqualTo(RotationDegree.Clockwise0));
    }

    // RotationDegreeExtensions line 49 - Invalid enum in GetZRotation
    [Test]
    public void RotationDegree_GetZRotation_InvalidEnum()
    {
        var invalidRotation = (RotationDegree)999;
        var result = invalidRotation.GetZRotation();
        Assert.That(result, Is.EqualTo(0f));
    }

    // SpanBitArray line 29 - TestAny wrapper
    [Test]
    public void SpanBitArray_TestAny_Wrapper()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);
        bitArray.Set(5, true);
        bitArray.Set(10, true);

        Assert.That(bitArray.TestAny(4, 4), Is.True);
        Assert.That(bitArray.TestAny(0, 4), Is.False);
    }

    // SpanBitArray line 30 - TestAll wrapper
    [Test]
    public void SpanBitArray_TestAll_Wrapper()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);
        bitArray.SetRange(4, 4, true);

        Assert.That(bitArray.TestAll(4, 4), Is.True);
        Assert.That(bitArray.TestAll(3, 4), Is.False);
    }

    // SpanBitArray line 31 - CountBits wrapper
    [Test]
    public void SpanBitArray_CountBits_Wrapper()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);
        bitArray.Set(0, true);
        bitArray.Set(2, true);
        bitArray.Set(4, true);

        var count = bitArray.CountBits(0, 8);

        Assert.That(count, Is.EqualTo(3));
    }

    // SpanBitArrayUtility line 18 - ValidateIndex with negative index
    [Test]
    public void SpanBitArrayUtility_ValidateIndex_NegativeIndex()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            _ = bitArray.Get(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 18 - ValidateIndex with index >= length
    [Test]
    public void SpanBitArrayUtility_ValidateIndex_IndexOutOfBounds()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            _ = bitArray.Get(8);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 24 - ValidateRange with negative index
    [Test]
    public void SpanBitArrayUtility_ValidateRange_NegativeIndex()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            bitArray.SetRange(-1, 4, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 27 - ValidateRange with negative bitCount
    [Test]
    public void SpanBitArrayUtility_ValidateRange_NegativeBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            bitArray.SetRange(0, -1, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 30 - ValidateRange with index > length
    [Test]
    public void SpanBitArrayUtility_ValidateRange_IndexExceedsLength()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            bitArray.SetRange(9, 1, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 36 - ValidateRange with range exceeding bounds
    [Test]
    public void SpanBitArrayUtility_ValidateRange_RangeExceedsBounds()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            bitArray.SetRange(4, 5, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 42 - ValidateLimitedRange with bitCount <= 0
    [Test]
    public void SpanBitArrayUtility_ValidateLimitedRange_ZeroBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);

        try
        {
            bitArray.SetBits(0, 0xFF, 0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // SpanBitArrayUtility line 42 - ValidateLimitedRange with bitCount > 64
    [Test]
    public void SpanBitArrayUtility_ValidateLimitedRange_BitCountTooLarge()
    {
        Span<byte> bytes = stackalloc byte[10];
        var bitArray = new SpanBitArray(bytes, 80);

        try
        {
            bitArray.SetBits(0, 0xFFFFFFFFFFFFFFFFUL, 65);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
    }

    // Additional tests for SpanBitArray early returns that need to be tested via public API
    [Test]
    public void SpanBitArray_Inverse_ZeroBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);
        bitArray.Set(0, true);

        // This should return early without changing anything
        bitArray.Inverse(0, 0);

        // Verify bit 0 is still true (wasn't inversed)
        Assert.That(bitArray.Get(0), Is.True);
    }

    [Test]
    public void SpanBitArray_Inverse_NonZero()
    {
        Span<byte> bytes = stackalloc byte[1];
        var bitArray = new SpanBitArray(bytes, 8);
        bitArray.Set(0, true);
        bitArray.Set(1, false);

        bitArray.Inverse(0, 2);

        Assert.That(bitArray.Get(0), Is.False);
        Assert.That(bitArray.Get(1), Is.True);
    }
}
