using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class GridShapeTests
{
    [Test]
    public void Constructor_CreatesEmptyGrid()
    {
        var grid = new GridShape(5, 5, Allocator.Temp);

        Assert.AreEqual(5, grid.Width);
        Assert.AreEqual(5, grid.Height);
        Assert.AreEqual(25, grid.Size);

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.IsFalse(grid[x, y]);

        grid.Dispose();
    }

    [Test]
    public void SetCell_GetCell_WorkCorrectly()
    {
        var grid = new GridShape(3, 3, Allocator.Temp);

        grid[0, 0] = true;
        grid[1, 1] = true;
        grid[2, 2] = true;

        Assert.IsTrue(grid[0, 0]);
        Assert.IsTrue(grid[1, 1]);
        Assert.IsTrue(grid[2, 2]);
        Assert.IsFalse(grid[0, 1]);
        Assert.IsFalse(grid[1, 0]);

        grid.Dispose();
    }

    [Test]
    public void Clear_ResetsAllCells()
    {
        var grid = new GridShape(4, 4, Allocator.Temp);

        grid[0, 0] = true;
        grid[1, 1] = true;
        grid[2, 2] = true;
        grid[3, 3] = true;

        grid.Clear();

        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.IsFalse(grid[x, y]);

        grid.Dispose();
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var original = new GridShape(3, 4, Allocator.Temp);
        original[0, 0] = true;
        original[1, 2] = true;
        original[2, 3] = true;

        var clone = original.Clone(Allocator.Temp);

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);

        for (var y = 0; y < original.Height; y++)
        for (var x = 0; x < original.Width; x++)
        {
            Assert.AreEqual(original[x, y], clone[x, y]);
        }

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var original = new GridShape(3, 3, Allocator.Temp);
        original[1, 1] = true;

        var clone = original.Clone(Allocator.Temp);
        clone[0, 0] = true;
        clone[1, 1] = false;

        Assert.IsTrue(original[1, 1]);
        Assert.IsFalse(original[0, 0]);

        Assert.IsFalse(clone[1, 1]);
        Assert.IsTrue(clone[0, 0]);

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void GetIndex_CalculatesCorrectly()
    {
        var grid = new GridShape(5, 3, Allocator.Temp);

        Assert.AreEqual(0, grid.GetIndex(new GridPosition(0, 0)));
        Assert.AreEqual(4, grid.GetIndex(new GridPosition(4, 0)));
        Assert.AreEqual(5, grid.GetIndex(new GridPosition(0, 1)));
        Assert.AreEqual(14, grid.GetIndex(new GridPosition(4, 2)));

        grid.Dispose();
    }
}
