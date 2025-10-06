using System;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class EdgeCaseTests
{
    // ImmutableGridShape2DList edge cases - test GetOrCreateImmutable with non-trimmed shape
    [Test]
    public void ImmutableGridShape2DList_GetOrCreateImmutable_NotTrimmed_ThrowsException()
    {
        using var shape = new GridShape(5, 5);
        shape[2, 2] = true;

        Assert.Throws<ArgumentException>(() => shape.GetOrCreateImmutable());
    }

    // ReadOnlySpanBitArray/SpanBitArray edge cases
    [Test]
    public void SpanBitArray_SetBits_WithLargeValue_WorksCorrectly()
    {
        Span<byte> bytes = stackalloc byte[8];
        var bitArray = new SpanBitArray(bytes, 64);

        bitArray.SetBits(0, 0xFFFFFFFFFFFFFFFFUL, 64);

        for (int i = 0; i < 64; i++)
        {
            Assert.That(bitArray.Get(i), Is.True);
        }
    }

    [Test]
    public void ReadOnlySpanBitArray_GetBits_MaxBitCount_ReturnsCorrectly()
    {
        Span<byte> bytes = stackalloc byte[8];
        bytes.Fill(0xFF);
        var bitArray = new ReadOnlySpanBitArray(bytes, 64);

        var value = bitArray.GetBits(0, 64);
        Assert.That(value, Is.EqualTo(0xFFFFFFFFFFFFFFFFUL));
    }

    // GridShapeExtension edge cases - test various rotations already covered by other tests
    // RotationDegree.None is not handled by RotateShape as it's meant for actual rotations

    // BoolGridShapeExtension edge cases - test wrapper methods
    [Test]
    public void BoolGridShapeExtension_CopyTo_CallsGenericVersion()
    {
        using var source = new GridShape(3, 3);
        source[0, 0] = true;
        using var target = new GridShape(3, 3);

        source.CopyTo(target);

        Assert.That(target[0, 0], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_SetCellValue_CallsGenericVersion()
    {
        using var grid = new GridShape(3, 3);

        grid.SetCellValue(1, 1, true);

        Assert.That(grid[1, 1], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_FillAll_CallsGenericVersion()
    {
        using var grid = new GridShape(3, 3);

        grid.FillAll(true);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(grid[x, y], Is.True);
        }
    }

    [Test]
    public void BoolGridShapeExtension_FillRect_CallsGenericVersion()
    {
        using var grid = new GridShape(5, 5);

        grid.FillRect(1, 1, 2, 2, true);

        Assert.That(grid[1, 1], Is.True);
        Assert.That(grid[2, 2], Is.True);
        Assert.That(grid[0, 0], Is.False);
    }

    // ReadOnlyGridShapeExtensions - IsValuesEquals edge case with height iteration
    [Test]
    public void ReadOnlyGridShapeExtensions_IsValuesEquals_IteratesHeight()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 2] = 5;
        shape2[0, 2] = 5;

        Assert.That(shape1.IsValuesEquals<ValueGridShape<int>, ValueGridShape<int>, int>(shape2), Is.True);

        shape2[0, 2] = 6;
        Assert.That(shape1.IsValuesEquals<ValueGridShape<int>, ValueGridShape<int>, int>(shape2), Is.False);
    }

    // Test CheckShapeCells
    [Test]
    public void ReadOnlyGridShapeExtensions_CheckShapeCells_WithOutOfBoundsPredicate()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(3, 3);
        shape.FillAll(true);

        var result = grid.CheckShapeCells<GridShape, GridShape, bool>(
            shape, -1, -1,
            false, (gx, gy, value, _) => gx >= 0 && gy >= 0 && gx < 5 && gy < 5);

        Assert.That(result, Is.False);
    }

    // Test FindFirstFitWithFixedRotation with various scenarios
    [Test]
    public void ReadOnlyGridShapeExtensions_FindFirstFitWithFixedRotation_AtEdge()
    {
        using var grid = new GridShape(3, 3);
        using var shape = new GridShape(1, 1);
        shape[0, 0] = true;

        var (x, y) = grid.FindFirstFitWithFixedRotation<GridShape, GridShape, bool>(shape);

        Assert.That(x, Is.EqualTo(0));
        Assert.That(y, Is.EqualTo(0));
    }

    // Test IsWithinBounds
    [Test]
    public void ReadOnlyGridShapeExtensions_IsWithinBounds_EdgeCases()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(1, 1);

        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, 0, 0), Is.True);
        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, 4, 4), Is.True);
        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, 5, 0), Is.False);
        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, 0, 5), Is.False);
        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, -1, 0), Is.False);
        Assert.That(grid.IsWithinBounds<GridShape, GridShape, bool>(shape, 0, -1), Is.False);
    }

    // SpanBitArrayUtility edge cases
    [Test]
    public void SpanBitArrayUtility_ByteCount_EdgeCases()
    {
        Assert.That(SpanBitArrayUtility.ByteCount(-5), Is.EqualTo(0));
        Assert.That(SpanBitArrayUtility.ByteCount(0), Is.EqualTo(0));
        Assert.That(SpanBitArrayUtility.ByteCount(1), Is.EqualTo(1));
        Assert.That(SpanBitArrayUtility.ByteCount(7), Is.EqualTo(1));
        Assert.That(SpanBitArrayUtility.ByteCount(8), Is.EqualTo(1));
        Assert.That(SpanBitArrayUtility.ByteCount(9), Is.EqualTo(2));
    }
}
