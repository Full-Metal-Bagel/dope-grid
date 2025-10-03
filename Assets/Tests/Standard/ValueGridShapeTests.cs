using DopeGrid.Standard;
using NUnit.Framework;
using System;

public class StandardValueGridShapeTests
{
    [Test]
    public void Constructor_CreatesEmptyGrid()
    {
        using var grid = new ValueGridShape<int>(3, 4);

        Assert.AreEqual(3, grid.Width);
        Assert.AreEqual(4, grid.Height);
        Assert.AreEqual(12, grid.Size);

        for (int y = 0; y < 4; y++)
        for (int x = 0; x < 3; x++)
            Assert.AreEqual(0, grid.GetValue((x, y)));
    }

    [Test]
    public void Constructor_WithDefaultValue_FillsGrid()
    {
        using var grid = new ValueGridShape<int>(3, 3, 42);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.AreEqual(42, grid.GetValue((x, y)));
    }

    [Test]
    public void SetValue_GetValue_WorkCorrectly()
    {
        using var grid = new ValueGridShape<string>(2, 2);

        grid.SetValue((0, 0), "A");
        grid.SetValue((1, 1), "B");

        Assert.AreEqual("A", grid.GetValue((0, 0)));
        Assert.AreEqual("B", grid.GetValue((1, 1)));
        Assert.AreEqual(null, grid.GetValue((0, 1)));
    }

    [Test]
    public void Indexer_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3);

        grid[1, 1] = 5;
        grid[(2, 2)] = 10;

        Assert.AreEqual(5, grid[1, 1]);
        Assert.AreEqual(10, grid[(2, 2)]);
        Assert.AreEqual(0, grid[0, 0]);

        grid.Dispose();
    }

    [Test]
    public void Fill_FillsAllCells()
    {
        using var grid = new ValueGridShape<int>(3, 3);

        grid.FillAll(7);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.AreEqual(7, grid.GetValue((x, y)));
    }

    [Test]
    public void FillRect_FillsRectangleArea()
    {
        using var grid = new ValueGridShape<int>(5, 5);

        grid.FillRect(1, 1, 3, 2, 99);

        Assert.AreEqual(99, grid.GetValue((1, 1)));
        Assert.AreEqual(99, grid.GetValue((2, 1)));
        Assert.AreEqual(99, grid.GetValue((3, 1)));
        Assert.AreEqual(99, grid.GetValue((1, 2)));
        Assert.AreEqual(0, grid.GetValue((0, 0)));
        Assert.AreEqual(0, grid.GetValue((4, 4)));
    }

    [Test]
    public void Clear_ResetsToDefault()
    {
        using var grid = new ValueGridShape<int>(3, 3);
        grid.FillAll(5);

        grid.Clear();

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.AreEqual(0, grid.GetValue((x, y)));
    }

    [Test]
    public void Contains_DetectsBounds()
    {
        using var grid = new ValueGridShape<int>(3, 3);

        Assert.IsTrue(grid.Contains((0, 0)));
        Assert.IsTrue(grid.Contains((2, 2)));
        Assert.IsFalse(grid.Contains((3, 0)));
        Assert.IsFalse(grid.Contains((0, 3)));
        Assert.IsFalse(grid.Contains((-1, 0)));
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        using var original = new ValueGridShape<int>(2, 2);
        original.SetValue((0, 0), 1);
        original.SetValue((1, 1), 2);

        using var clone = original.Clone();

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);
        Assert.AreEqual(1, clone.GetValue((0, 0)));
        Assert.AreEqual(2, clone.GetValue((1, 1)));

        clone.SetValue((0, 0), 99);
        Assert.AreEqual(1, original.GetValue((0, 0)));
    }

    [Test]
    public void CopyTo_CopiesCorrectly()
    {
        using var source = new ValueGridShape<int>(2, 2);
        source.SetValue((0, 0), 1);
        source.SetValue((1, 1), 2);

        using var dest = new ValueGridShape<int>(2, 2);
        source.CopyTo(dest);

        Assert.AreEqual(1, dest.GetValue((0, 0)));
        Assert.AreEqual(2, dest.GetValue((1, 1)));
    }

    [Test]
    public void CopyTo_ThrowsOnDifferentDimensions()
    {
        using var source = new ValueGridShape<int>(2, 2);
        using var dest = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentException>(() => source.CopyTo(dest));
    }

    [Test]
    public void CountValue_CountsCorrectly()
    {
        using var grid = new ValueGridShape<int>(3, 3);
        grid.SetValue((0, 0), 5);
        grid.SetValue((1, 1), 5);
        grid.SetValue((2, 2), 10);

        Assert.AreEqual(2, grid.CountValue(5));
        Assert.AreEqual(1, grid.CountValue(10));
        Assert.AreEqual(6, grid.CountValue(0));
    }

    [Test]
    public void CountWhere_CountsWithPredicate()
    {
        using var grid = new ValueGridShape<int>(3, 3);
        grid.SetValue((0, 0), 5);
        grid.SetValue((1, 1), 10);
        grid.SetValue((2, 2), 15);

        Assert.AreEqual(2, grid.CountWhere(x => x > 5));
        Assert.AreEqual(3, grid.CountWhere(x => x > 0));
    }

    [Test]
    public void Any_DetectsMatchingElements()
    {
        using var grid = new ValueGridShape<int>(2, 2);
        grid.SetValue((1, 1), 42);

        Assert.IsTrue(grid.Any(x => x == 42));
        Assert.IsFalse(grid.Any(x => x == 100));
    }

    [Test]
    public void All_ChecksAllElements()
    {
        using var grid = new ValueGridShape<int>(2, 2, 5);

        Assert.IsTrue(grid.All(x => x == 5));

        grid.SetValue((0, 0), 10);
        Assert.IsFalse(grid.All(x => x == 5));
    }

    [Test]
    public void ToGridShape_ConvertsCorrectly()
    {
        using var valueGrid = new ValueGridShape<int>(3, 3);
        valueGrid.SetValue((0, 0), 1);
        valueGrid.SetValue((1, 1), 1);
        valueGrid.SetValue((2, 2), 0);

        using var gridShape = valueGrid.ToGridShape(1);

        Assert.AreEqual(3, gridShape.Width);
        Assert.AreEqual(3, gridShape.Height);
        Assert.IsTrue(gridShape.GetCell((0, 0)));
        Assert.IsTrue(gridShape.GetCell((1, 1)));
        Assert.IsFalse(gridShape.GetCell((2, 2)));
    }

    [Test]
    public void ToGridShape_WithPredicate_ConvertsCorrectly()
    {
        using var valueGrid = new ValueGridShape<int>(3, 3);
        valueGrid.SetValue((0, 0), 5);
        valueGrid.SetValue((1, 1), 10);
        valueGrid.SetValue((2, 2), 3);

        using var gridShape = valueGrid.ToGridShape(x => x > 5);

        Assert.IsFalse(gridShape.GetCell((0, 0)));
        Assert.IsTrue(gridShape.GetCell((1, 1)));
        Assert.IsFalse(gridShape.GetCell((2, 2)));
    }

    [Test]
    public void FromGridShape_ConvertsCorrectly()
    {
        using var gridShape = Shapes.LShape();
        using var valueGrid = new ValueGridShape<int>(2, 2);

        valueGrid.FromGridShape(gridShape, 1, 0);

        Assert.AreEqual(1, valueGrid.GetValue((0, 0)));
        Assert.AreEqual(1, valueGrid.GetValue((0, 1)));
        Assert.AreEqual(1, valueGrid.GetValue((1, 1)));
        Assert.AreEqual(0, valueGrid.GetValue((1, 0)));
    }

    [Test]
    public void AsReadOnly_ReturnsReadOnlyView()
    {
        using var grid = new ValueGridShape<int>(2, 2);
        grid.SetValue((1, 1), 42);

        var readOnly = grid.AsReadOnly();

        Assert.AreEqual(2, readOnly.Width);
        Assert.AreEqual(2, readOnly.Height);
        Assert.AreEqual(42, readOnly.GetValue((1, 1)));
        Assert.AreEqual(0, readOnly.GetValue((0, 0)));
    }
}
