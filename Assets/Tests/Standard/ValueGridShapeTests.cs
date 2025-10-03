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
            Assert.AreEqual(0, grid[x, y]);
    }

    [Test]
    public void Constructor_WithDefaultValue_FillsGrid()
    {
        using var grid = new ValueGridShape<int>(3, 3, 42);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.AreEqual(42, grid[x, y]);
    }

    [Test]
    public void SetValue_GetValue_WorkCorrectly()
    {
        var grid = new ValueGridShape<string>(2, 2);
        try
        {
            grid[0, 0] = "A";
            grid[1, 1] = "B";

            Assert.AreEqual("A", grid[0, 0]);
            Assert.AreEqual("B", grid[1, 1]);
            Assert.AreEqual(null, grid[0, 1]);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Indexer_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3);

        grid[1, 1] = 5;
        grid[2, 2] = 10;

        Assert.AreEqual(5, grid[1, 1]);
        Assert.AreEqual(10, grid[2, 2]);
        Assert.AreEqual(0, grid[0, 0]);

        grid.Dispose();
    }

    [Test]
    public void Fill_FillsAllCells()
    {
        var grid = new ValueGridShape<int>(3, 3);
        try
        {
            grid.FillAll(7);

            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                Assert.AreEqual(7, grid[x, y]);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void FillRect_FillsRectangleArea()
    {
        var grid = new ValueGridShape<int>(5, 5);
        try
        {
            grid.FillRect(1, 1, 3, 2, 99);

            Assert.AreEqual(99, grid[1, 1]);
            Assert.AreEqual(99, grid[2, 1]);
            Assert.AreEqual(99, grid[3, 1]);
            Assert.AreEqual(99, grid[1, 2]);
            Assert.AreEqual(0, grid[0, 0]);
            Assert.AreEqual(0, grid[4, 4]);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Clear_ResetsToDefault()
    {
        var grid = new ValueGridShape<int>(3, 3);
        try
        {
            grid.FillAll(5);

            grid.FillAll(0);

            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                Assert.AreEqual(0, grid[x, y]);
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Contains_DetectsBounds()
    {
        var grid = new ValueGridShape<int>(3, 3);
        try
        {
            Assert.IsTrue(grid.Contains(0, 0));
            Assert.IsTrue(grid.Contains(2, 2));
            Assert.IsFalse(grid.Contains(3, 0));
            Assert.IsFalse(grid.Contains(0, 3));
            Assert.IsFalse(grid.Contains(-1, 0));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new ValueGridShape<int>(2, 2);
        try
        {
            original[0, 0] = 1;
            original[1, 1] = 2;

            var clone = original.Clone();
            try
            {
                Assert.AreEqual(original.Width, clone.Width);
                Assert.AreEqual(original.Height, clone.Height);
                Assert.AreEqual(1, clone[0, 0]);
                Assert.AreEqual(2, clone[1, 1]);

                clone[0, 0] = 99;
                Assert.AreEqual(1, original[0, 0]);
            }
            finally
            {
                clone.Dispose();
            }
        }
        finally
        {
            original.Dispose();
        }
    }

    [Test]
    public void CopyTo_CopiesCorrectly()
    {
        var source = new ValueGridShape<int>(2, 2);
        try
        {
            source[0, 0] = 1;
            source[1, 1] = 2;

            using var dest = new ValueGridShape<int>(2, 2);
            source.CopyTo(dest);

            Assert.AreEqual(1, dest[0, 0]);
            Assert.AreEqual(2, dest[1, 1]);
        }
        finally
        {
            source.Dispose();
        }
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
        var grid = new ValueGridShape<int>(3, 3);
        try
        {
            grid[0, 0] = 5;
            grid[1, 1] = 5;
            grid[2, 2] = 10;

            Assert.AreEqual(2, grid.CountValue(5));
            Assert.AreEqual(1, grid.CountValue(10));
            Assert.AreEqual(6, grid.CountValue(0));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void CountWhere_CountsWithPredicate()
    {
        var grid = new ValueGridShape<int>(3, 3);
        try
        {
            grid[0, 0] = 5;
            grid[1, 1] = 10;
            grid[2, 2] = 15;

            Assert.AreEqual(2, grid.CountWhere(x => x > 5));
            Assert.AreEqual(3, grid.CountWhere(x => x > 0));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void Any_DetectsMatchingElements()
    {
        var grid = new ValueGridShape<int>(2, 2);
        try
        {
            grid[1, 1] = 42;

            Assert.IsTrue(grid.Any(x => x == 42));
            Assert.IsFalse(grid.Any(x => x == 100));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void All_ChecksAllElements()
    {
        var grid = new ValueGridShape<int>(2, 2, 5);
        try
        {
            Assert.IsTrue(grid.All(x => x == 5));

            grid[0, 0] = 10;
            Assert.IsFalse(grid.All(x => x == 5));
        }
        finally
        {
            grid.Dispose();
        }
    }

    [Test]
    public void ToGridShape_ConvertsCorrectly()
    {
        var valueGrid = new ValueGridShape<int>(3, 3);
        try
        {
            valueGrid[0, 0] = 1;
            valueGrid[1, 1] = 1;
            valueGrid[2, 2] = 0;

            using var gridShape = valueGrid.ToGridShape(1);

            Assert.AreEqual(3, gridShape.Width);
            Assert.AreEqual(3, gridShape.Height);
            Assert.IsTrue(gridShape[0, 0]);
            Assert.IsTrue(gridShape[1, 1]);
            Assert.IsFalse(gridShape[2, 2]);
        }
        finally
        {
            valueGrid.Dispose();
        }
    }

    [Test]
    public void ToGridShape_WithPredicate_ConvertsCorrectly()
    {
        var valueGrid = new ValueGridShape<int>(3, 3);
        try
        {
            valueGrid[0, 0] = 5;
            valueGrid[1, 1] = 10;
            valueGrid[2, 2] = 3;

            using var gridShape = valueGrid.ToGridShape(x => x > 5);

            Assert.IsFalse(gridShape[0, 0]);
            Assert.IsTrue(gridShape[1, 1]);
            Assert.IsFalse(gridShape[2, 2]);
        }
        finally
        {
            valueGrid.Dispose();
        }
    }

    [Test]
    public void FromGridShape_ConvertsCorrectly()
    {
        using var gridShape = Shapes.LShape();
        var valueGrid = new ValueGridShape<int>(2, 2);
        try
        {
            valueGrid.FromGridShape(gridShape, 1, 0);

            Assert.AreEqual(1, valueGrid[0, 0]);
            Assert.AreEqual(1, valueGrid[0, 1]);
            Assert.AreEqual(1, valueGrid[1, 1]);
            Assert.AreEqual(0, valueGrid[1, 0]);
        }
        finally
        {
            valueGrid.Dispose();
        }
    }

    [Test]
    public void AsReadOnly_ReturnsReadOnlyView()
    {
        var grid = new ValueGridShape<int>(2, 2);
        try
        {
            grid[1, 1] = 42;

            var readOnly = grid.AsReadOnly();

            Assert.AreEqual(2, readOnly.Width);
            Assert.AreEqual(2, readOnly.Height);
            Assert.AreEqual(42, readOnly[1, 1]);
            Assert.AreEqual(0, readOnly[0, 0]);
        }
        finally
        {
            grid.Dispose();
        }
    }
}
