using DopeInventory;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class GridShape2DTests
{
    [Test]
    public void Constructor_CreatesEmptyGrid()
    {
        var grid = new GridShape2D(5, 5, Allocator.Temp);

        Assert.AreEqual(5, grid.Width);
        Assert.AreEqual(5, grid.Height);
        Assert.AreEqual(25, grid.Size);

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.IsFalse(grid.GetCell(new int2(x, y)));

        grid.Dispose();
    }

    [Test]
    public void SetCell_GetCell_WorkCorrectly()
    {
        var grid = new GridShape2D(3, 3, Allocator.Temp);

        grid.SetCell(new int2(0, 0), true);
        grid.SetCell(new int2(1, 1), true);
        grid.SetCell(new int2(2, 2), true);

        Assert.IsTrue(grid.GetCell(new int2(0, 0)));
        Assert.IsTrue(grid.GetCell(new int2(1, 1)));
        Assert.IsTrue(grid.GetCell(new int2(2, 2)));
        Assert.IsFalse(grid.GetCell(new int2(0, 1)));
        Assert.IsFalse(grid.GetCell(new int2(1, 0)));

        grid.Dispose();
    }

    [Test]
    public void Clear_ResetsAllCells()
    {
        var grid = new GridShape2D(4, 4, Allocator.Temp);

        grid.SetCell(new int2(0, 0), true);
        grid.SetCell(new int2(1, 1), true);
        grid.SetCell(new int2(2, 2), true);
        grid.SetCell(new int2(3, 3), true);

        grid.Clear();

        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.IsFalse(grid.GetCell(new int2(x, y)));

        grid.Dispose();
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var original = new GridShape2D(3, 4, Allocator.Temp);
        original.SetCell(new int2(0, 0), true);
        original.SetCell(new int2(1, 2), true);
        original.SetCell(new int2(2, 3), true);

        var clone = original.Clone(Allocator.Temp);

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);

        for (var y = 0; y < original.Height; y++)
        for (var x = 0; x < original.Width; x++)
        {
            var pos = new int2(x, y);
            Assert.AreEqual(original.GetCell(pos), clone.GetCell(pos));
        }

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var original = new GridShape2D(3, 3, Allocator.Temp);
        original.SetCell(new int2(1, 1), true);

        var clone = original.Clone(Allocator.Temp);
        clone.SetCell(new int2(0, 0), true);
        clone.SetCell(new int2(1, 1), false);

        Assert.IsTrue(original.GetCell(new int2(1, 1)));
        Assert.IsFalse(original.GetCell(new int2(0, 0)));

        Assert.IsFalse(clone.GetCell(new int2(1, 1)));
        Assert.IsTrue(clone.GetCell(new int2(0, 0)));

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void GetIndex_CalculatesCorrectly()
    {
        var grid = new GridShape2D(5, 3, Allocator.Temp);

        Assert.AreEqual(0, grid.GetIndex(new int2(0, 0)));
        Assert.AreEqual(4, grid.GetIndex(new int2(4, 0)));
        Assert.AreEqual(5, grid.GetIndex(new int2(0, 1)));
        Assert.AreEqual(14, grid.GetIndex(new int2(4, 2)));

        grid.Dispose();
    }

    [Test]
    public void IsCreated_ReflectsDisposalState()
    {
        var grid = new GridShape2D(3, 3, Allocator.Temp);

        Assert.IsTrue(grid.IsCreated);

        grid.Dispose();

        Assert.IsFalse(grid.IsCreated);
    }
}