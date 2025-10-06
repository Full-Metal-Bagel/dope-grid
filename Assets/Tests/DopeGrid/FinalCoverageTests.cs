using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class FinalCoverageTests
{
    // BoolGridShapeExtension - targeting uncovered lines
    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_FullCoverage()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        grid.FillShapeWithValue(shape, 1, 1, true);

        Assert.That(grid[1, 1], Is.True);
        Assert.That(grid[2, 2], Is.True);
        Assert.That(grid[3, 3], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_WithClipping()
    {
        using var grid = new GridShape(3, 3);
        using var shape = new GridShape(5, 5);
        shape.FillAll(true);

        grid.FillShapeWithValue(shape, 0, 0, true);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(grid[x, y], Is.True);
        }
    }

    // SpanBitArrayUtility - test uncovered byte count cases
    [Test]
    public void SpanBitArrayUtility_ByteCount_LargeBitCounts()
    {
        Assert.That(SpanBitArrayUtility.ByteCount(64), Is.EqualTo(8));
        Assert.That(SpanBitArrayUtility.ByteCount(65), Is.EqualTo(9));
        Assert.That(SpanBitArrayUtility.ByteCount(128), Is.EqualTo(16));
        Assert.That(SpanBitArrayUtility.ByteCount(1000), Is.EqualTo(125));
    }

    // GridShapeExtension - test more rotation and flip cases
    [Test]
    public void GridShapeExtension_RotateShape_Clockwise180()
    {
        using var input = new ValueGridShape<int>(3, 3);
        input[0, 0] = 1;
        input[1, 1] = 2;
        input[2, 2] = 3;

        using var output = new ValueGridShape<int>(3, 3);
        input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise180, output);

        Assert.That(output[2, 2], Is.EqualTo(1));
        Assert.That(output[1, 1], Is.EqualTo(2));
        Assert.That(output[0, 0], Is.EqualTo(3));
    }

    [Test]
    public void GridShapeExtension_RotateShape_Clockwise270()
    {
        using var input = new ValueGridShape<int>(2, 3);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;
        input[0, 2] = 5;
        input[1, 2] = 6;

        using var output = new ValueGridShape<int>(3, 2);
        input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise270, output);

        // After 270 degree rotation, rightmost column becomes bottom row
        Assert.That(output[0, 1], Is.EqualTo(1));
        Assert.That(output[0, 0], Is.EqualTo(2));
    }

    [Test]
    public void GridShapeExtension_FlipShape_Horizontal()
    {
        using var input = new ValueGridShape<int>(3, 2);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[2, 0] = 3;
        input[0, 1] = 4;
        input[1, 1] = 5;
        input[2, 1] = 6;

        using var output = new ValueGridShape<int>(3, 2);
        input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>(FlipAxis.Horizontal, output);

        Assert.That(output[2, 0], Is.EqualTo(1));
        Assert.That(output[1, 0], Is.EqualTo(2));
        Assert.That(output[0, 0], Is.EqualTo(3));
    }

    [Test]
    public void GridShapeExtension_FlipShape_Vertical()
    {
        using var input = new ValueGridShape<int>(2, 3);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;
        input[0, 2] = 5;
        input[1, 2] = 6;

        using var output = new ValueGridShape<int>(2, 3);
        input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>(FlipAxis.Vertical, output);

        Assert.That(output[0, 2], Is.EqualTo(1));
        Assert.That(output[1, 2], Is.EqualTo(2));
        Assert.That(output[0, 0], Is.EqualTo(5));
        Assert.That(output[1, 0], Is.EqualTo(6));
    }

    // ImmutableGridShape2DList - test more edge cases
    [Test]
    public void ImmutableGridShape2DList_GetOrCreateImmutable_SingleCellShape()
    {
        using var shape = new GridShape(1, 1);
        shape[0, 0] = true;

        var immutable = shape.GetOrCreateImmutable();

        Assert.That(immutable.Width, Is.EqualTo(1));
        Assert.That(immutable.Height, Is.EqualTo(1));
        Assert.That(immutable[0, 0], Is.True);
    }

    [Test]
    public void ImmutableGridShape2DList_GetOrCreateImmutable_MultipleSingleCells()
    {
        using var shape1 = new GridShape(1, 1);
        shape1[0, 0] = true;

        using var shape2 = new GridShape(1, 1);
        shape2[0, 0] = true;

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.That(immutable1.Id, Is.EqualTo(immutable2.Id));
    }

    // ReadOnlySpanBitArray - test more edge cases
    [Test]
    public void ReadOnlySpanBitArray_GetBits_PartialByte()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0b10101010;
        bytes[1] = 0b01010101;

        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.GetBits(0, 4), Is.EqualTo(0b1010UL));
        Assert.That(bitArray.GetBits(4, 4), Is.EqualTo(0b1010UL));
        Assert.That(bitArray.GetBits(8, 4), Is.EqualTo(0b0101UL));
    }

    [Test]
    public void ReadOnlySpanBitArray_TestAny_SpanningBytes()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0b10000000; // bit 7 is set
        bytes[1] = 0b00000001; // bit 8 is set

        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.TestAny(7, 2), Is.True);
    }

    [Test]
    public void ReadOnlySpanBitArray_TestAll_SpanningBytes()
    {
        Span<byte> bytes = stackalloc byte[2];
        bytes[0] = 0xFF;
        bytes[1] = 0xFF;

        var bitArray = new ReadOnlySpanBitArray(bytes, 16);

        Assert.That(bitArray.TestAll(6, 4), Is.True);
    }

    // SpanBitArray - test more setter edge cases
    [Test]
    public void SpanBitArray_SetBits_SpanningBytes()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetBits(6, 0b1111, 4);

        Assert.That(bitArray.Get(6), Is.True);
        Assert.That(bitArray.Get(7), Is.True);
        Assert.That(bitArray.Get(8), Is.True);
        Assert.That(bitArray.Get(9), Is.True);
    }

    [Test]
    public void SpanBitArray_SetRange_SpanningMultipleBytes()
    {
        Span<byte> bytes = stackalloc byte[3];
        var bitArray = new SpanBitArray(bytes, 24);

        bitArray.SetRange(6, 12, true);

        for (int i = 6; i < 18; i++)
        {
            Assert.That(bitArray.Get(i), Is.True);
        }
        Assert.That(bitArray.Get(5), Is.False);
        Assert.That(bitArray.Get(18), Is.False);
    }

    [Test]
    public void SpanBitArray_Set_MultipleBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);

        for (int i = 0; i < 16; i += 2)
        {
            bitArray.Set(i, true);
        }

        for (int i = 0; i < 16; i++)
        {
            Assert.That(bitArray.Get(i), Is.EqualTo(i % 2 == 0));
        }
    }

    // RotationDegreeExtensions - test all methods
    [Test]
    public void RotationDegree_GetNextClockwiseRotation_AllDegrees()
    {
        Assert.That(RotationDegree.None.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise90));
        Assert.That(RotationDegree.Clockwise90.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise180));
        Assert.That(RotationDegree.Clockwise180.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise270));
        Assert.That(RotationDegree.Clockwise270.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.None));
    }

    [Test]
    public void RotationDegree_GetPreviousClockwiseRotation_AllDegrees()
    {
        Assert.That(RotationDegree.None.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise270));
        Assert.That(RotationDegree.Clockwise90.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.None));
        Assert.That(RotationDegree.Clockwise180.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise90));
        Assert.That(RotationDegree.Clockwise270.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise180));
    }

    [Test]
    public void RotationDegree_GetZRotation_AllDegrees()
    {
        Assert.That(RotationDegree.None.GetZRotation(), Is.EqualTo(0f));
        Assert.That(RotationDegree.Clockwise90.GetZRotation(), Is.EqualTo(-90f));
        Assert.That(RotationDegree.Clockwise180.GetZRotation(), Is.EqualTo(-180f));
        Assert.That(RotationDegree.Clockwise270.GetZRotation(), Is.EqualTo(-270f));
    }

    // GridShape - test more corner cases
    [Test]
    public void GridShape_SetAndGet_AllPositions()
    {
        using var shape = new GridShape(3, 3);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            shape[x, y] = (x + y) % 2 == 0;
        }

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.EqualTo((x + y) % 2 == 0));
        }
    }

    // ValueGridShape - test more scenarios
    [Test]
    public void ValueGridShape_SetAndGet_DifferentValues()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            shape[x, y] = x * 10 + y;
        }

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.EqualTo(x * 10 + y));
        }
    }

    // Additional ReadOnlyGridShapeExtensions coverage
    [Test]
    public void ReadOnlyGridShapeExtensions_GetTrimmedBound_ComplexShape()
    {
        using var shape = new ValueGridShape<int>(5, 5);
        shape[1, 1] = 10;
        shape[2, 2] = 20;
        shape[3, 3] = 30;

        var (x, y, w, h) = shape.GetTrimmedBound<ValueGridShape<int>, int>();

        Assert.That(x, Is.EqualTo(1));
        Assert.That(y, Is.EqualTo(1));
        Assert.That(w, Is.EqualTo(3));
        Assert.That(h, Is.EqualTo(3));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_IsTrimmed_NotTrimmedShape()
    {
        using var shape = new ValueGridShape<int>(5, 5);
        shape[2, 2] = 10;

        Assert.That(shape.IsTrimmed<ValueGridShape<int>, int>(), Is.False);
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_IsTrimmed_TrimmedShape()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 10;
        shape[2, 2] = 20;

        Assert.That(shape.IsTrimmed<ValueGridShape<int>, int>(), Is.True);
    }

    // Additional edge cases for coverage
    [Test]
    public void ReadOnlySpanBitArray_Get_AllBits()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0b11110000;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        for (int i = 0; i < 4; i++)
        {
            Assert.That(bitArray.Get(i), Is.False);
        }
        for (int i = 4; i < 8; i++)
        {
            Assert.That(bitArray.Get(i), Is.True);
        }
    }

    [Test]
    public void SpanBitArray_Clear_AllBits()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);
        bitArray.SetRange(0, 16, true);

        bitArray.SetRange(0, 16, false);

        for (int i = 0; i < 16; i++)
        {
            Assert.That(bitArray.Get(i), Is.False);
        }
    }
}
