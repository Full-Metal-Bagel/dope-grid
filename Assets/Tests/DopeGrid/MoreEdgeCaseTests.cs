using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class MoreEdgeCaseTests
{
    // Additional BoolGridShapeExtension tests - need to cover FillShapeWithValue with negative offsets
    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_WithOffsetBeyondGrid()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        grid.FillShapeWithValue(shape, 4, 4, true);

        Assert.That(grid[4, 4], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_WithPartialClipping()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        grid.FillShapeWithValue(shape, 0, 0, true);

        Assert.That(grid[0, 0], Is.True);
        Assert.That(grid[1, 1], Is.True);
    }

    // Additional SpanBitArrayUtility tests - test ByteCount with negative numbers
    [Test]
    public void SpanBitArrayUtility_ByteCount_NegativeValue()
    {
        Assert.That(SpanBitArrayUtility.ByteCount(-10), Is.EqualTo(0));
        Assert.That(SpanBitArrayUtility.ByteCount(0), Is.EqualTo(0));
    }

    // Additional ImmutableGridShape2DList tests - test edge cases of GetOrCreateShape
    [Test]
    public void ImmutableGridShape2DList_GetOrCreateImmutable_SameShapeTwice_ReturnsSameId()
    {
        using var shape1 = new GridShape(2, 2);
        shape1[0, 0] = true;
        shape1[1, 1] = true;

        using var shape2 = new GridShape(2, 2);
        shape2[0, 0] = true;
        shape2[1, 1] = true;

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.That(immutable1.Id, Is.EqualTo(immutable2.Id));
    }

    // Additional ReadOnlySpanBitArray edge cases
    [Test]
    public void ReadOnlySpanBitArray_TestAny_ZeroBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0xFF;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAny(0, 0), Is.False);
    }

    [Test]
    public void ReadOnlySpanBitArray_TestAll_ZeroBitCount()
    {
        Span<byte> bytes = stackalloc byte[1];
        bytes[0] = 0x00;
        var bitArray = new ReadOnlySpanBitArray(bytes, 8);

        Assert.That(bitArray.TestAll(0, 0), Is.True);
    }

    // Additional SpanBitArray edge cases
    [Test]
    public void SpanBitArray_SetBits_WithSingleBit()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetBits(4, 0x1, 1);

        Assert.That(bitArray.Get(4), Is.True);
        Assert.That(bitArray.Get(5), Is.False);
    }

    [Test]
    public void SpanBitArray_SetRange_SingleBit()
    {
        Span<byte> bytes = stackalloc byte[2];
        var bitArray = new SpanBitArray(bytes, 16);

        bitArray.SetRange(4, 1, true);

        Assert.That(bitArray.Get(4), Is.True);
        Assert.That(bitArray.Get(3), Is.False);
        Assert.That(bitArray.Get(5), Is.False);
    }

    // Additional RotationDegreeExtensions tests
    [Test]
    public void RotationDegree_CalculateRotatedSize_AllDegrees()
    {
        var (w1, h1) = RotationDegree.None.CalculateRotatedSize(4, 6);
        Assert.That(w1, Is.EqualTo(4));
        Assert.That(h1, Is.EqualTo(6));

        var (w2, h2) = RotationDegree.Clockwise90.CalculateRotatedSize(4, 6);
        Assert.That(w2, Is.EqualTo(6));
        Assert.That(h2, Is.EqualTo(4));

        var (w3, h3) = RotationDegree.Clockwise180.CalculateRotatedSize(4, 6);
        Assert.That(w3, Is.EqualTo(4));
        Assert.That(h3, Is.EqualTo(6));

        var (w4, h4) = RotationDegree.Clockwise270.CalculateRotatedSize(4, 6);
        Assert.That(w4, Is.EqualTo(6));
        Assert.That(h4, Is.EqualTo(4));
    }

    // Additional GridShapeExtension tests - FillShapeWithValue with offset at boundaries
    [Test]
    public void GridShapeExtension_FillShapeWithValue_AtBoundary()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        grid.FillShapeWithValue(shape, 3, 3, 99);

        // Cells should be filled at boundary
        Assert.That(grid[3, 3], Is.EqualTo(99));
        Assert.That(grid[4, 4], Is.EqualTo(99));
        Assert.That(grid[3, 4], Is.EqualTo(99));
        Assert.That(grid[4, 3], Is.EqualTo(99));
    }

    // Additional ReadOnlyGridShapeExtensions tests
    [Test]
    public void ReadOnlyGridShapeExtensions_CheckShapeCells_WithNullPredicate_ThrowsException()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);

        Assert.Throws<ArgumentNullException>(() =>
            grid.CheckShapeCells<GridShape, GridShape, bool, int>(shape, 0, 0, 0, null!));
    }

    // Additional ValueGridShape tests
    [Test]
    public void ValueGridShape_ReadOnly_WithDifferentDefault()
    {
        using var shape = new ValueGridShape<int>(3, 3, emptyValue: 5);
        var readOnly = shape.AsReadOnly();

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(readOnly[x, y], Is.EqualTo(5));
        }
    }
}
