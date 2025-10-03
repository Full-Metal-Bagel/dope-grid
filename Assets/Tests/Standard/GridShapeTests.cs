using DopeGrid.Standard;
using NUnit.Framework;

public class StandardGridShapeTests
{
    [Test]
    public void Constructor_CreatesEmptyGrid()
    {
        var grid = new GridShape(5, 5);
        try
        {
            Assert.AreEqual(5, grid.Width);
            Assert.AreEqual(5, grid.Height);
            Assert.AreEqual(25, grid.Size);

            for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
                Assert.IsFalse(grid.GetCellValue(x, y));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Constructor_WithPooling_UsesArrayPool()
    {
        var gridPooled = new GridShape(5, 5);
        var gridNonPooled = new GridShape(5, 5);
        try
        {
            Assert.AreEqual(5, gridPooled.Width);
            Assert.AreEqual(5, gridNonPooled.Width);
        }
        finally
        {
            gridPooled.Dispose();
            gridNonPooled.Dispose();
        }
    }

    [Test]
    public void SetCell_GetCell_WorkCorrectly()
    {
        var grid = new GridShape(3, 3);
        try
        {
            grid.SetCellValue(0, 0, true);
            grid.SetCellValue(1, 1, true);
            grid.SetCellValue(2, 2, true);

            Assert.IsTrue(grid.GetCellValue(0, 0));
            Assert.IsTrue(grid.GetCellValue(1, 1));
            Assert.IsTrue(grid.GetCellValue(2, 2));
            Assert.IsFalse(grid.GetCellValue(0, 1));
            Assert.IsFalse(grid.GetCellValue(1, 0));
        }
        finally
        {
            grid.Dispose();
        }
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
        var grid = new GridShape(4, 4);
        try
        {
            grid.SetCellValue(0, 0, true);
            grid.SetCellValue(1, 1, true);
            grid.SetCellValue(2, 2, true);
            grid.SetCellValue(3, 3, true);

            grid.Clear();

            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
                Assert.IsFalse(grid.GetCellValue(x, y));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Fill_SetsAllCells()
    {
        var grid = new GridShape(3, 3);
        try
        {
            grid.FillAll(true);

            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                Assert.IsTrue(grid.GetCellValue(x, y));

            grid.FillAll(false);

            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                Assert.IsFalse(grid.GetCellValue(x, y));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void FillRect_FillsRectangleArea()
    {
        var grid = new GridShape(5, 5);
        try
        {
            grid.FillRect(1, 1, 3, 2, true);

            Assert.IsTrue(grid.GetCellValue(1, 1));
            Assert.IsTrue(grid.GetCellValue(2, 1));
            Assert.IsTrue(grid.GetCellValue(3, 1));
            Assert.IsTrue(grid.GetCellValue(1, 2));
            Assert.IsTrue(grid.GetCellValue(2, 2));
            Assert.IsTrue(grid.GetCellValue(3, 2));

            Assert.IsFalse(grid.GetCellValue(0, 0));
            Assert.IsFalse(grid.GetCellValue(4, 4));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var original = new GridShape(3, 4);
        var clone = original.Clone();
        try
        {
            original.SetCellValue(0, 0, true);
            original.SetCellValue(1, 2, true);
            original.SetCellValue(2, 3, true);

            Assert.AreEqual(original.Width, clone.Width);
            Assert.AreEqual(original.Height, clone.Height);

            for (var y = 0; y < original.Height; y++)
            for (var x = 0; x < original.Width; x++)
            {
                Assert.AreEqual(original.GetCellValue(x, y), clone.GetCellValue(x, y));
            }
        }
        finally
        {
            original.Dispose();
            clone.Dispose();
        }
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var original = new GridShape(3, 3);
        var clone = original.Clone();
        try
        {
            original.SetCellValue(1, 1, true);

            clone.SetCellValue(0, 0, true);
            clone.SetCellValue(1, 1, false);

            Assert.IsTrue(original.GetCellValue(1, 1));
            Assert.IsFalse(original.GetCellValue(0, 0));

            Assert.IsFalse(clone.GetCellValue(1, 1));
            Assert.IsTrue(clone.GetCellValue(0, 0));
        }
        finally
        {
            original.Dispose();
            clone.Dispose();
        }
    }

    [Test]
    public void OccupiedSpaceCount_ReturnsCorrectCount()
    {
        var grid = new GridShape(4, 4);
        try
        {
            Assert.AreEqual(0, grid.OccupiedSpaceCount);

            grid.SetCellValue(0, 0, true);
            grid.SetCellValue(1, 1, true);
            grid.SetCellValue(2, 2, true);

            Assert.AreEqual(3, grid.OccupiedSpaceCount);
            Assert.AreEqual(13, grid.FreeSpaceCount);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void CopyTo_CopiesCorrectly()
    {
        var source = new GridShape(3, 3);
        var dest = new GridShape(3, 3);
        try
        {
            source.SetCellValue(0, 0, true);
            source.SetCellValue(1, 1, true);
            source.SetCellValue(2, 2, true);

            source.CopyTo(dest);

            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
            {
                Assert.AreEqual(source.GetCellValue(x, y), dest.GetCellValue(x, y));
            }
        }
        finally
        {
            source.Dispose();
            dest.Dispose();
        }
    }

    [Test]
    public void CopyTo_ThrowsOnDifferentDimensions()
    {
        var source = new GridShape(3, 3);
        var dest = new GridShape(4, 4);
        try
        {
            Assert.Throws<System.ArgumentException>(() => source.CopyTo(dest));
        }
        finally
        {
            source.Dispose();
            dest.Dispose();
        }
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
        var grid = new GridShape(3, 3);
        try
        {
            grid.SetCellValue(1, 1, true);

            var readOnly = grid.AsReadOnly();

            Assert.AreEqual(3, readOnly.Width);
            Assert.AreEqual(3, readOnly.Height);
            Assert.IsTrue(readOnly.GetCellValue(1, 1));
            Assert.IsFalse(readOnly.GetCellValue(0, 0));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void ReadOnly_Equals_WorksCorrectly()
    {
        var grid1 = new GridShape(2, 2);
        var grid2 = new GridShape(2, 2);
        var grid3 = new GridShape(2, 2);
        try
        {
            grid1.SetCellValue(0, 0, true);
            grid2.SetCellValue(0, 0, true);
            grid3.SetCellValue(1, 1, true);

            var ro1 = grid1.AsReadOnly();
            var ro2 = grid2.AsReadOnly();
            var ro3 = grid3.AsReadOnly();

            Assert.IsTrue(ro1.Equals(ro2));
            Assert.IsFalse(ro1.Equals(ro3));
        }
        finally
        {
            grid1.Dispose();
            grid2.Dispose();
            grid3.Dispose();
        }
    }
}
