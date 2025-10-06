using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class ReadOnlyBoolGridShapeExtensionsTests
{
    [Test]
    public void IsValuesEquals_ComparesGrids()
    {
        using var grid1 = new GridShape(3, 3);
        using var grid2 = new GridShape(3, 3);
        grid1[0, 0] = true;
        grid2[0, 0] = true;

        Assert.That(grid1.IsValuesEquals(grid2), Is.True);

        grid2[1, 1] = true;
        Assert.That(grid1.IsValuesEquals(grid2), Is.False);
    }

    [Test]
    public void GetCellValue_GetsValue()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        Assert.That(shape.GetCellValue(1, 1), Is.True);
        Assert.That(shape.GetCellValue(0, 0), Is.False);
    }

    [Test]
    public void CountValue_CountsBoolValues()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;

        Assert.That(shape.CountValue(true), Is.EqualTo(2));
        Assert.That(shape.CountValue(false), Is.EqualTo(7));
    }

    [Test]
    public void CountWhere_WithPredicate_Counts()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;

        Assert.That(shape.CountWhere(v => v), Is.EqualTo(2));
        Assert.That(shape.CountWhere(v => !v), Is.EqualTo(7));
    }

    [Test]
    public void Count_WithCaptureData_Counts()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;

        var target = true;
        Assert.That(shape.Count((v, t) => v == t, target), Is.EqualTo(1));
    }

    [Test]
    public void Any_WithPredicate_ReturnsCorrectly()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        Assert.That(shape.Any(v => v), Is.True);

        using var emptyShape = new GridShape(3, 3);
        Assert.That(emptyShape.Any(v => v), Is.False);
    }

    [Test]
    public void Any_WithCaptureData_ReturnsCorrectly()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        var target = true;
        Assert.That(shape.Any((v, t) => v == t, target), Is.True);
    }

    [Test]
    public void All_WithPredicate_ReturnsCorrectly()
    {
        using var shape = new GridShape(3, 3);
        Assert.That(shape.All(v => !v), Is.True);

        shape[0, 0] = true;
        Assert.That(shape.All(v => !v), Is.False);
    }

    [Test]
    public void All_WithCaptureData_ReturnsCorrectly()
    {
        using var shape = new GridShape(3, 3);
        var expected = false;
        Assert.That(shape.All((v, e) => v == e, expected), Is.True);
    }

    [Test]
    public void IsTrimmed_DetectsTrimmedShape()
    {
        using var shape1 = new GridShape(3, 3);
        shape1[0, 0] = true;
        shape1[2, 2] = true;
        Assert.That(shape1.IsTrimmed(), Is.True);

        using var shape2 = new GridShape(5, 5);
        shape2[2, 2] = true;
        Assert.That(shape2.IsTrimmed(), Is.False);
    }

    [Test]
    public void CanPlaceItem_ChecksPlacement()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        Assert.That(grid.CanPlaceItem(shape, 0, 0), Is.True);
        Assert.That(grid.CanPlaceItem(shape, 3, 3), Is.True);
        Assert.That(grid.CanPlaceItem(shape, 4, 4), Is.False);
    }

    [Test]
    public void CheckShapeCells_WithPredicate_Checks()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);
        shape[0, 0] = true;

        var result = grid.CheckShapeCells(shape, 1, 1, (gx, gy, value) => !value || !grid[gx, gy]);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckShapeCells_WithCaptureData_Checks()
    {
        using var grid = new GridShape(5, 5);
        grid[1, 1] = true;
        using var shape = new GridShape(2, 2);
        shape[0, 0] = true;

        var occupied = true;
        var result = grid.CheckShapeCells(shape, 1, 1, occupied, (gx, gy, value, occ) => !value || grid[gx, gy] != occ);

        Assert.That(result, Is.False);
    }

    [Test]
    public void FindFirstFitWithFixedRotation_FindsPosition()
    {
        using var grid = new GridShape(5, 5);
        grid[0, 0] = true;
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        var (x, y) = grid.FindFirstFitWithFixedRotation(shape);

        // The method finds the first fit position
        Assert.That(x, Is.GreaterThanOrEqualTo(0));
        Assert.That(y, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void FindFirstFitWithFixedRotation_NoFit_ReturnsNegative()
    {
        using var grid = new GridShape(2, 2);
        grid.FillAll(true);
        using var shape = new GridShape(2, 2);
        shape.FillAll(true);

        var (x, y) = grid.FindFirstFitWithFixedRotation(shape);

        Assert.That(x, Is.EqualTo(-1));
        Assert.That(y, Is.EqualTo(-1));
    }

    [Test]
    public void IsWithinBounds_ChecksBounds()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);

        Assert.That(grid.IsWithinBounds(shape, 0, 0), Is.True);
        Assert.That(grid.IsWithinBounds(shape, 3, 3), Is.True);
        Assert.That(grid.IsWithinBounds(shape, 4, 4), Is.False);
        Assert.That(grid.IsWithinBounds(shape, -1, 0), Is.False);
    }

    [Test]
    public void FindFirstFitWithFreeRotation_FindsPosition()
    {
        using var grid = new GridShape(5, 5);
        grid[0, 0] = true;
        using var tempShape = new GridShape(2, 3);
        tempShape.FillAll(true);
        var shape = tempShape.GetOrCreateImmutable();

        var (x, y, rotation) = grid.FindFirstFitWithFreeRotation(shape);

        Assert.That(x, Is.GreaterThanOrEqualTo(-1));
        Assert.That(y, Is.GreaterThanOrEqualTo(-1));
    }

    [Test]
    public void FindFirstFitWithFreeRotation_NoFit_ReturnsNegative()
    {
        using var grid = new GridShape(2, 2);
        grid.FillAll(true);
        using var tempShape = new GridShape(2, 2);
        tempShape.FillAll(true);
        var shape = tempShape.GetOrCreateImmutable();

        var (x, y, rotation) = grid.FindFirstFitWithFreeRotation(shape);

        Assert.That(x, Is.EqualTo(-1));
        Assert.That(y, Is.EqualTo(-1));
    }
}
