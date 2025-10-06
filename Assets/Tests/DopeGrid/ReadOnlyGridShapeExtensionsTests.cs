using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class ReadOnlyGridShapeExtensionsTests
{
    [Test]
    public void Size_CalculatesCorrectly()
    {
        using var shape = new GridShape(3, 5);
        Assert.That(shape.Size(), Is.EqualTo(15));
    }

    [Test]
    public void IsEmpty_DetectsEmpty()
    {
        using var shape1 = new GridShape(0, 5);
        using var shape2 = new GridShape(5, 0);
        using var shape3 = new GridShape(3, 3);

        Assert.That(shape1.IsZeroSize(), Is.True);
        Assert.That(shape2.IsZeroSize(), Is.True);
        Assert.That(shape3.IsZeroSize(), Is.False);
    }

    [Test]
    public void Contains_ChecksBounds()
    {
        using var shape = new GridShape(5, 5);

        Assert.That(shape.Contains(0, 0), Is.True);
        Assert.That(shape.Contains(4, 4), Is.True);
        Assert.That(shape.Contains(-1, 0), Is.False);
        Assert.That(shape.Contains(0, -1), Is.False);
        Assert.That(shape.Contains(5, 0), Is.False);
        Assert.That(shape.Contains(0, 5), Is.False);
    }

    [Test]
    public void GetCellValue_GetsValue()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[1, 1] = 42;

        Assert.That(shape.GetCellValue<ValueGridShape<int>, int>(1, 1), Is.EqualTo(42));
    }

    [Test]
    public void CountValue_CountsMatchingValues()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 1;
        shape[1, 1] = 1;
        shape[2, 2] = 2;

        Assert.That(shape.CountValue<ValueGridShape<int>, int>(1), Is.EqualTo(2));
        Assert.That(shape.CountValue<ValueGridShape<int>, int>(2), Is.EqualTo(1));
        Assert.That(shape.CountValue<ValueGridShape<int>, int>(0), Is.EqualTo(6));
    }

    [Test]
    public void CountWhere_CountsMatchingPredicate()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 5;
        shape[1, 1] = 10;
        shape[2, 2] = 15;

        Assert.That(shape.CountWhere<ValueGridShape<int>, int>(v => v > 5), Is.EqualTo(2));
        Assert.That(shape.CountWhere<ValueGridShape<int>, int>(v => v == 0), Is.EqualTo(6));
    }

    [Test]
    public void Count_WithCaptureData_CountsCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 5;
        shape[1, 1] = 10;
        shape[2, 2] = 15;

        var threshold = 7;
        Assert.That(shape.Count<ValueGridShape<int>, int, int>((v, t) => v > t, threshold), Is.EqualTo(2));
    }

    [Test]
    public void Count_WithNullPredicate_ThrowsException()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentNullException>(() => shape.Count<ValueGridShape<int>, int, int>(null!, 0));
    }

    [Test]
    public void Any_WithPredicate_ReturnsTrue()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[1, 1] = 10;

        Assert.That(shape.Any<ValueGridShape<int>, int>(v => v == 10), Is.True);
        Assert.That(shape.Any<ValueGridShape<int>, int>(v => v == 99), Is.False);
    }

    [Test]
    public void Any_WithCaptureData_ReturnsTrue()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[1, 1] = 10;

        var target = 10;
        Assert.That(shape.Any<ValueGridShape<int>, int, int>((v, t) => v == t, target), Is.True);
    }

    [Test]
    public void Any_WithNullPredicate_ThrowsException()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentNullException>(() => shape.Any<ValueGridShape<int>, int, int>(null!, 0));
    }

    [Test]
    public void All_WithPredicate_ReturnsCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.That(shape.All<ValueGridShape<int>, int>(v => v == 0), Is.True);

        shape[1, 1] = 1;
        Assert.That(shape.All<ValueGridShape<int>, int>(v => v == 0), Is.False);
    }

    [Test]
    public void All_WithCaptureData_ReturnsCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        var value = 0;
        Assert.That(shape.All<ValueGridShape<int>, int, int>((v, val) => v == val, value), Is.True);
    }

    [Test]
    public void All_WithNullPredicate_ThrowsException()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentNullException>(() => shape.All<ValueGridShape<int>, int, int>(null!, 0));
    }

    [Test]
    public void IsTrimmed_DetectsTrimmedShape()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        shape1[0, 0] = 1;
        shape1[2, 2] = 1;
        Assert.That(shape1.IsTrimmed<ValueGridShape<int>, int>(), Is.True);

        using var shape2 = new ValueGridShape<int>(5, 5);
        shape2[2, 2] = 1;
        Assert.That(shape2.IsTrimmed<ValueGridShape<int>, int>(), Is.False);
    }

    [Test]
    public void GetTrimmedBound_CalculatesCorrectBounds()
    {
        using var shape = new ValueGridShape<int>(5, 5);
        shape[1, 1] = 1;
        shape[3, 3] = 1;

        var (x, y, width, height) = shape.GetTrimmedBound<ValueGridShape<int>, int>();

        Assert.That(x, Is.EqualTo(1));
        Assert.That(y, Is.EqualTo(1));
        Assert.That(width, Is.EqualTo(3));
        Assert.That(height, Is.EqualTo(3));
    }

    [Test]
    public void GetTrimmedBound_EmptyShape_ReturnsZero()
    {
        using var shape = new ValueGridShape<int>(5, 5);

        var (x, y, width, height) = shape.GetTrimmedBound<ValueGridShape<int>, int>();

        Assert.That(x, Is.EqualTo(0));
        Assert.That(y, Is.EqualTo(0));
        Assert.That(width, Is.EqualTo(0));
        Assert.That(height, Is.EqualTo(0));
    }

    [Test]
    public void CanPlaceItem_ChecksPlacement()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new GridShape(2, 2);
        shape[0, 0] = true;
        shape[1, 1] = true;

        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, 0, 0), Is.True);
        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, 3, 3), Is.True);
        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, 4, 4), Is.False);
        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, -1, 0), Is.False);
    }

    [Test]
    public void CanPlaceItem_WithOccupiedCells_ReturnsFalse()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        grid[1, 1] = 1;
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, 0, 0), Is.False);
        Assert.That(grid.CanPlaceItem<ValueGridShape<int>, GridShape, int>(shape, 2, 2), Is.True);
    }
}
