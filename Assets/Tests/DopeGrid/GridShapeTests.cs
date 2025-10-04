using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class GridShapeTests
{
    [Test]
    public void Constructor_CreatesShapeWithCorrectDimensions()
    {
        using var shape = new GridShape(3, 4);

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(4));
        Assert.That(shape.Size, Is.EqualTo(12));
    }

    [Test]
    public void Constructor_InitializesAllCellsToFalse()
    {
        using var shape = new GridShape(3, 3);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.False);
        }
    }

    [Test]
    public void Indexer_SetsAndGetsValues()
    {
        using var shape = new GridShape(3, 3);

        shape[1, 2] = true;

        Assert.That(shape[1, 2], Is.True);
        Assert.That(shape[0, 0], Is.False);
    }

    [Test]
    public void Indexer_WithGridPosition_Works()
    {
        using var shape = new GridShape(3, 3);
        var pos = new GridPosition(1, 2);

        shape[pos] = true;

        Assert.That(shape[pos], Is.True);
    }

    [Test]
    public void IsOccupied_ReturnsSameAsValue()
    {
        using var shape = new GridShape(3, 3);

        shape[1, 1] = true;

        Assert.That(shape.IsOccupied(1, 1), Is.True);
        Assert.That(shape.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void OccupiedSpaceCount_ReturnsCorrectCount()
    {
        using var shape = new GridShape(3, 3);

        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(3));
    }

    [Test]
    public void FreeSpaceCount_ReturnsCorrectCount()
    {
        using var shape = new GridShape(3, 3);

        shape[0, 0] = true;
        shape[1, 1] = true;

        Assert.That(shape.FreeSpaceCount(), Is.EqualTo(7));
    }

    [Test]
    public void Clear_ResetsAllCells()
    {
        using var shape = new GridShape(3, 3);

        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        shape.Clear();

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.False);
        }
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        using var original = new GridShape(3, 3);
        original[0, 0] = true;
        original[1, 1] = true;

        using var clone = original.Clone();

        Assert.That(clone.Width, Is.EqualTo(original.Width));
        Assert.That(clone.Height, Is.EqualTo(original.Height));
        Assert.That(clone[0, 0], Is.True);
        Assert.That(clone[1, 1], Is.True);
        Assert.That(clone[2, 2], Is.False);
    }

    [Test]
    public void Clone_IsIndependent()
    {
        using var original = new GridShape(3, 3);
        original[0, 0] = true;

        using var clone = original.Clone();
        clone[1, 1] = true;

        Assert.That(original[1, 1], Is.False);
        Assert.That(clone[1, 1], Is.True);
    }

    [Test]
    public void CopyTo_CopiesAllData()
    {
        using var source = new GridShape(3, 3);
        using var dest = new GridShape(3, 3);

        source[0, 0] = true;
        source[1, 1] = true;

        source.CopyTo(dest);

        Assert.That(dest[0, 0], Is.True);
        Assert.That(dest[1, 1], Is.True);
        Assert.That(dest[2, 2], Is.False);
    }

    [Test]
    public void Equals_WithSameContent_ReturnsTrue()
    {
        using var shape1 = new GridShape(3, 3);
        using var shape2 = new GridShape(3, 3);

        shape1[0, 0] = true;
        shape2[0, 0] = true;

        Assert.That(shape1 == shape2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentContent_ReturnsFalse()
    {
        using var shape1 = new GridShape(3, 3);
        using var shape2 = new GridShape(3, 3);

        shape1[0, 0] = true;
        shape2[1, 1] = true;

        Assert.That(shape1 != shape2, Is.True);
    }

    [Test]
    public void ReadOnly_ProvidesReadAccess()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        GridShape.ReadOnly readOnly = shape.AsReadOnly();

        Assert.That(readOnly.Width, Is.EqualTo(3));
        Assert.That(readOnly.Height, Is.EqualTo(3));
        Assert.That(readOnly[1, 1], Is.True);
    }

    [Test]
    public void ReadOnly_Clone_Works()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        GridShape.ReadOnly readOnly = shape.AsReadOnly();
        using var clone = readOnly.Clone();

        Assert.That(clone[1, 1], Is.True);
    }

    [Test]
    public void ReadOnly_CopyTo_Works()
    {
        using var source = new GridShape(3, 3);
        using var dest = new GridShape(3, 3);
        source[1, 1] = true;

        GridShape.ReadOnly readOnly = source.AsReadOnly();
        readOnly.CopyTo(dest);

        Assert.That(dest[1, 1], Is.True);
    }

    [Test]
    public void ReadOnly_IsOccupied_Works()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        GridShape.ReadOnly readOnly = shape.AsReadOnly();

        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
        Assert.That(readOnly.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void ReadOnly_OccupiedSpaceCount_Works()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;

        GridShape.ReadOnly readOnly = shape.AsReadOnly();

        Assert.That(readOnly.OccupiedSpaceCount(), Is.EqualTo(2));
    }

    [Test]
    public void ReadOnly_FreeSpaceCount_Works()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;

        GridShape.ReadOnly readOnly = shape.AsReadOnly();

        Assert.That(readOnly.FreeSpaceCount(), Is.EqualTo(8));
    }

    [Test]
    public void ReadOnly_Equals_Works()
    {
        using var shape1 = new GridShape(3, 3);
        using var shape2 = new GridShape(3, 3);
        shape1[1, 1] = true;
        shape2[1, 1] = true;

        GridShape.ReadOnly readOnly1 = shape1.AsReadOnly();
        GridShape.ReadOnly readOnly2 = shape2.AsReadOnly();

        Assert.That(readOnly1 == readOnly2, Is.True);
    }

    [Test]
    public void EmptyShape_HasSizeZero()
    {
        using var shape = new GridShape(0, 0);

        Assert.That(shape.Size, Is.EqualTo(0));
    }

    [Test]
    public void LargeShape_WorksCorrectly()
    {
        using var shape = new GridShape(100, 100);

        shape[50, 50] = true;
        shape[99, 99] = true;

        Assert.That(shape[50, 50], Is.True);
        Assert.That(shape[99, 99], Is.True);
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(2));
    }

    [Test]
    public void Dispose_SecondCallThrowsException()
    {
        var shape = new GridShape(3, 3);
        shape.Dispose();

        Assert.Throws<InvalidOperationException>(() => shape.Dispose(),
            "Second dispose should throw because array was already returned to pool");
    }
}
