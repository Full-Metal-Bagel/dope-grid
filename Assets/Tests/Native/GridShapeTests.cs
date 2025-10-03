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
            Assert.IsFalse(grid.GetCellValue(x, y));

        grid.Dispose();
    }

    [Test]
    public void SetCell_GetCell_WorkCorrectly()
    {
        var grid = new GridShape(3, 3, Allocator.Temp);

        grid.SetCellValue(0, 0, true);
        grid.SetCellValue(1, 1, true);
        grid.SetCellValue(2, 2, true);

        Assert.IsTrue(grid.GetCellValue(0, 0));
        Assert.IsTrue(grid.GetCellValue(1, 1));
        Assert.IsTrue(grid.GetCellValue(2, 2));
        Assert.IsFalse(grid.GetCellValue(0, 1));
        Assert.IsFalse(grid.GetCellValue(1, 0));

        grid.Dispose();
    }

    [Test]
    public void Clear_ResetsAllCells()
    {
        var grid = new GridShape(4, 4, Allocator.Temp);

        grid.SetCellValue(0, 0, true);
        grid.SetCellValue(1, 1, true);
        grid.SetCellValue(2, 2, true);
        grid.SetCellValue(3, 3, true);

        grid.Clear();

        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.IsFalse(grid.GetCellValue(x, y));

        grid.Dispose();
    }

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var original = new GridShape(3, 4, Allocator.Temp);
        original.SetCellValue(0, 0, true);
        original.SetCellValue(1, 2, true);
        original.SetCellValue(2, 3, true);

        var clone = original.Clone(Allocator.Temp);

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);

        for (var y = 0; y < original.Height; y++)
        for (var x = 0; x < original.Width; x++)
        {
            Assert.AreEqual(original.GetCellValue(x, y), clone.GetCellValue(x, y));
        }

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var original = new GridShape(3, 3, Allocator.Temp);
        original.SetCellValue(1, 1, true);

        var clone = original.Clone(Allocator.Temp);
        clone.SetCellValue(0, 0, true);
        clone.SetCellValue(1, 1, false);

        Assert.IsTrue(original.GetCellValue(1, 1));
        Assert.IsFalse(original.GetCellValue(0, 0));

        Assert.IsFalse(clone.GetCellValue(1, 1));
        Assert.IsTrue(clone.GetCellValue(0, 0));

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void GetIndex_CalculatesCorrectly()
    {
        var grid = new GridShape(5, 3, Allocator.Temp);

        Assert.AreEqual(0, grid.GetIndex(0, 0));
        Assert.AreEqual(4, grid.GetIndex(4, 0));
        Assert.AreEqual(5, grid.GetIndex(0, 1));
        Assert.AreEqual(14, grid.GetIndex(4, 2));

        grid.Dispose();
    }
}
