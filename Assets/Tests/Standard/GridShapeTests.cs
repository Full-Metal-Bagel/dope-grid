using DopeGrid.Standard;
using NUnit.Framework;

public class StandardGridShapeTests
{
    [Test]
    public void Constructor_CreatesEmptyGrid()
    {
        using var grid = new GridShape(5, 5);

        Assert.AreEqual(5, grid.Width);
        Assert.AreEqual(5, grid.Height);
        Assert.AreEqual(25, grid.Size);

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.IsFalse(grid.GetCell(x, y));
    }

    [Test]
    public void Constructor_WithPooling_UsesArrayPool()
    {
        using var gridPooled = new GridShape(5, 5);
        using var gridNonPooled = new GridShape(5, 5);

        Assert.AreEqual(5, gridPooled.Width);
        Assert.AreEqual(5, gridNonPooled.Width);
    }

    [Test]
    public void SetCell_GetCell_WorkCorrectly()
    {
        using var grid = new GridShape(3, 3);

        grid.SetCell(0, 0, true);
        grid.SetCell(1, 1, true);
        grid.SetCell(2, 2, true);

        Assert.IsTrue(grid.GetCell(0, 0));
        Assert.IsTrue(grid.GetCell(1, 1));
        Assert.IsTrue(grid.GetCell(2, 2));
        Assert.IsFalse(grid.GetCell(0, 1));
        Assert.IsFalse(grid.GetCell(1, 0));
    }

    [Test]
    public void Indexer_WorksCorrectly()
    {
        var grid = new GridShape(3, 3);
        try
        {
            grid[1, 1] = true;
            grid[2, 2] = true;

            Assert.IsTrue(grid[1, 1]);
            Assert.IsTrue(grid[2, 2]);
            Assert.IsFalse(grid[0, 0]);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Clear_ResetsAllCells()
    {
        using var grid = new GridShape(4, 4);

        grid.SetCell(0, 0, true);
        grid.SetCell(1, 1, true);
        grid.SetCell(2, 2, true);
        grid.SetCell(3, 3, true);

        grid.Clear();

        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.IsFalse(grid.GetCell(x, y));
    }

    [Test]
    public void Fill_SetsAllCells()
    {
        using var grid = new GridShape(3, 3);

        grid.FillAll(true);

        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            Assert.IsTrue(grid.GetCell(x, y));

        grid.FillAll(false);

        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            Assert.IsFalse(grid.GetCell(x, y));
    }

    [Test]
    public void FillRect_FillsRectangleArea()
    {
        using var grid = new GridShape(5, 5);

        grid.FillRect(1, 1, 3, 2, true);

        Assert.IsTrue(grid.GetCell(1, 1));
        Assert.IsTrue(grid.GetCell(2, 1));
        Assert.IsTrue(grid.GetCell(3, 1));
        Assert.IsTrue(grid.GetCell(1, 2));
        Assert.IsTrue(grid.GetCell(2, 2));
        Assert.IsTrue(grid.GetCell(3, 2));

        Assert.IsFalse(grid.GetCell(0, 0));
        Assert.IsFalse(grid.GetCell(4, 4));
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        using var original = new GridShape(3, 4);
        original.SetCell(0, 0, true);
        original.SetCell(1, 2, true);
        original.SetCell(2, 3, true);

        using var clone = original.Clone();

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);

        for (var y = 0; y < original.Height; y++)
        for (var x = 0; x < original.Width; x++)
        {
            Assert.AreEqual(original.GetCell(x, y), clone.GetCell(x, y));
        }
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        using var original = new GridShape(3, 3);
        original.SetCell(1, 1, true);

        using var clone = original.Clone();
        clone.SetCell(0, 0, true);
        clone.SetCell(1, 1, false);

        Assert.IsTrue(original.GetCell(1, 1));
        Assert.IsFalse(original.GetCell(0, 0));

        Assert.IsFalse(clone.GetCell(1, 1));
        Assert.IsTrue(clone.GetCell(0, 0));
    }

    [Test]
    public void OccupiedSpaceCount_ReturnsCorrectCount()
    {
        using var grid = new GridShape(4, 4);

        Assert.AreEqual(0, grid.OccupiedSpaceCount);

        grid.SetCell(0, 0, true);
        grid.SetCell(1, 1, true);
        grid.SetCell(2, 2, true);

        Assert.AreEqual(3, grid.OccupiedSpaceCount);
        Assert.AreEqual(13, grid.FreeSpaceCount);
    }

    [Test]
    public void CopyTo_CopiesCorrectly()
    {
        using var source = new GridShape(3, 3);
        source.SetCell(0, 0, true);
        source.SetCell(1, 1, true);
        source.SetCell(2, 2, true);

        using var dest = new GridShape(3, 3);
        source.CopyTo(dest);

        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
        {
            Assert.AreEqual(source.GetCell(x, y), dest.GetCell(x, y));
        }
    }

    [Test]
    public void CopyTo_ThrowsOnDifferentDimensions()
    {
        using var source = new GridShape(3, 3);
        using var dest = new GridShape(4, 4);

        Assert.Throws<System.ArgumentException>(() => source.CopyTo(dest));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var grid = new GridShape(3, 3);
        grid.Dispose();
        grid.Dispose(); // Should not throw

        Assert.Pass();
    }

    [Test]
    public void ReadOnly_AsReadOnly_ReturnsReadOnlyView()
    {
        using var grid = new GridShape(3, 3);
        grid.SetCell(1, 1, true);

        var readOnly = grid.AsReadOnly();

        Assert.AreEqual(3, readOnly.Width);
        Assert.AreEqual(3, readOnly.Height);
        Assert.IsTrue(readOnly.GetCell(1, 1));
        Assert.IsFalse(readOnly.GetCell(0, 0));
    }

    [Test]
    public void ReadOnly_Equals_WorksCorrectly()
    {
        using var grid1 = new GridShape(2, 2);
        grid1.SetCell(0, 0, true);

        using var grid2 = new GridShape(2, 2);
        grid2.SetCell(0, 0, true);

        using var grid3 = new GridShape(2, 2);
        grid3.SetCell(1, 1, true);

        var ro1 = grid1.AsReadOnly();
        var ro2 = grid2.AsReadOnly();
        var ro3 = grid3.AsReadOnly();

        Assert.IsTrue(ro1.Equals(ro2));
        Assert.IsFalse(ro1.Equals(ro3));
    }
}
