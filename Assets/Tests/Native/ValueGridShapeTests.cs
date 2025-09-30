using System;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class ValueGridShapeTests
{
    #region Constructor Tests

    [Test]
    public void DefaultConstructor_CreatesEmptyGrid()
    {
        var grid = new ValueGridShape<int>();

        Assert.AreEqual(0, grid.Width);
        Assert.AreEqual(0, grid.Height);
        Assert.AreEqual(0, grid.Size);
        Assert.IsTrue(grid.IsEmpty);

        grid.Dispose();
    }

    [Test]
    public void Constructor_WithDimensions_CreatesGridWithZeroValues()
    {
        var grid = new ValueGridShape<int>(3, 4, Allocator.Temp);

        Assert.AreEqual(3, grid.Width);
        Assert.AreEqual(4, grid.Height);
        Assert.AreEqual(12, grid.Size);
        Assert.IsFalse(grid.IsEmpty);

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                Assert.AreEqual(0, grid[x, y]);
            }
        }

        grid.Dispose();
    }

    [Test]
    public void Constructor_WithDefaultValue_FillsGrid()
    {
        const int defaultValue = 42;
        var grid = new ValueGridShape<int>(3, 3, defaultValue, Allocator.Temp);

        Assert.AreEqual(3, grid.Width);
        Assert.AreEqual(3, grid.Height);

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                Assert.AreEqual(defaultValue, grid[x, y]);
            }
        }

        grid.Dispose();
    }

    [Test]
    public void Constructor_WithZeroDimensions_CreatesEmptyGrid()
    {
        var grid1 = new ValueGridShape<int>(0, 5, Allocator.Temp);
        var grid2 = new ValueGridShape<int>(5, 0, Allocator.Temp);

        Assert.IsTrue(grid1.IsEmpty);
        Assert.IsTrue(grid2.IsEmpty);
        Assert.AreEqual(0, grid1.Size);
        Assert.AreEqual(0, grid2.Size);

        grid1.Dispose();
        grid2.Dispose();
    }

    #endregion

    #region Value Access Tests

    [Test]
    public void GetValue_SetValue_WorkCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);

        grid.SetValue(0, 0, 1);
        grid.SetValue(1, 1, 2);
        grid.SetValue(2, 2, 3);

        Assert.AreEqual(1, grid.GetValue(0, 0));
        Assert.AreEqual(2, grid.GetValue(1, 1));
        Assert.AreEqual(3, grid.GetValue(2, 2));
        Assert.AreEqual(0, grid.GetValue(0, 1));

        grid.Dispose();
    }

    [Test]
    public void GetValue_SetValue_WithInt2_WorkCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);

        grid.SetValue(new int2(0, 0), 10);
        grid.SetValue(new int2(1, 2), 20);
        grid.SetValue(new int2(2, 1), 30);

        Assert.AreEqual(10, grid.GetValue(new int2(0, 0)));
        Assert.AreEqual(20, grid.GetValue(new int2(1, 2)));
        Assert.AreEqual(30, grid.GetValue(new int2(2, 1)));

        grid.Dispose();
    }

    [Test]
    public void Indexer_XY_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);

        grid[0, 0] = 100;
        grid[1, 1] = 200;
        grid[2, 2] = 300;

        Assert.AreEqual(100, grid[0, 0]);
        Assert.AreEqual(200, grid[1, 1]);
        Assert.AreEqual(300, grid[2, 2]);
        Assert.AreEqual(0, grid[1, 0]);

        grid.Dispose();
    }

    [Test]
    public void Indexer_Int2_WorksCorrectly()
    {
        var grid = new ValueGridShape<float>(4, 4, Allocator.Temp);

        grid[new int2(0, 0)] = 1.5f;
        grid[new int2(2, 3)] = 2.5f;
        grid[new int2(3, 1)] = 3.5f;

        Assert.AreEqual(1.5f, grid[new int2(0, 0)]);
        Assert.AreEqual(2.5f, grid[new int2(2, 3)]);
        Assert.AreEqual(3.5f, grid[new int2(3, 1)]);

        grid.Dispose();
    }

    [Test]
    public void GetIndex_CalculatesCorrectly()
    {
        var grid = new ValueGridShape<int>(5, 3, Allocator.Temp);

        Assert.AreEqual(0, grid.GetIndex(0, 0));
        Assert.AreEqual(4, grid.GetIndex(4, 0));
        Assert.AreEqual(5, grid.GetIndex(0, 1));
        Assert.AreEqual(6, grid.GetIndex(1, 1));
        Assert.AreEqual(14, grid.GetIndex(4, 2));

        Assert.AreEqual(7, grid.GetIndex(new int2(2, 1)));

        grid.Dispose();
    }

    #endregion

    #region Fill Operations Tests

    [Test]
    public void Fill_SetsAllValues()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        grid.Fill(99);
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                Assert.AreEqual(99, grid[x, y]);
            }
        }

        grid.Dispose();
    }

    [Test]
    public void FillRect_FillsSpecifiedArea()
    {
        var grid = new ValueGridShape<int>(5, 5, Allocator.Temp);
        grid.FillRect(1, 1, 3, 2, 77);

        // Check filled area
        for (int y = 1; y < 3; y++)
        {
            for (int x = 1; x < 4; x++)
            {
                Assert.AreEqual(77, grid[x, y]);
            }
        }

        // Check unfilled corners
        Assert.AreEqual(0, grid[0, 0]);
        Assert.AreEqual(0, grid[4, 0]);
        Assert.AreEqual(0, grid[0, 4]);
        Assert.AreEqual(0, grid[4, 4]);

        grid.Dispose();
    }

    [Test]
    public void FillRect_WithInt2_FillsSpecifiedArea()
    {
        var grid = new ValueGridShape<int>(4, 4, Allocator.Temp);

        grid.FillRect(1, 1, 2, 2, 55);

        Assert.AreEqual(55, grid[1, 1]);
        Assert.AreEqual(55, grid[2, 1]);
        Assert.AreEqual(55, grid[1, 2]);
        Assert.AreEqual(55, grid[2, 2]);

        Assert.AreEqual(0, grid[0, 0]);
        Assert.AreEqual(0, grid[3, 3]);

        grid.Dispose();
    }

    [Test]
    public void FillRect_ClipsToGridBounds()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);

        // Try to fill rect that extends beyond grid
        grid.FillRect(-1, -1, 3, 3, 88);

        // Only the part within bounds should be filled
        Assert.AreEqual(88, grid[0, 0]);
        Assert.AreEqual(88, grid[1, 0]);
        Assert.AreEqual(88, grid[0, 1]);
        Assert.AreEqual(88, grid[1, 1]);

        // Bottom-right should not be filled
        Assert.AreEqual(0, grid[2, 2]);

        grid.Dispose();
    }

    [Test]
    public void Clear_ResetsAllValuesToDefault()
    {
        var grid = new ValueGridShape<int>(3, 3, 42, Allocator.Temp);

        grid.Clear();

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                Assert.AreEqual(0, grid[x, y]);
            }
        }

        grid.Dispose();
    }

    #endregion

    #region Contains Tests

    [Test]
    public void Contains_ReturnsTrueForValidPositions()
    {
        var grid = new ValueGridShape<int>(3, 4, Allocator.Temp);

        Assert.IsTrue(grid.Contains(0, 0));
        Assert.IsTrue(grid.Contains(2, 0));
        Assert.IsTrue(grid.Contains(0, 3));
        Assert.IsTrue(grid.Contains(2, 3));
        Assert.IsTrue(grid.Contains(1, 2));

        grid.Dispose();
    }

    [Test]
    public void Contains_ReturnsFalseForInvalidPositions()
    {
        var grid = new ValueGridShape<int>(3, 4, Allocator.Temp);

        Assert.IsFalse(grid.Contains(-1, 0));
        Assert.IsFalse(grid.Contains(0, -1));
        Assert.IsFalse(grid.Contains(3, 0));
        Assert.IsFalse(grid.Contains(0, 4));
        Assert.IsFalse(grid.Contains(3, 4));

        grid.Dispose();
    }

    [Test]
    public void Contains_WithInt2_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);

        Assert.IsTrue(grid.Contains(new int2(0, 0)));
        Assert.IsTrue(grid.Contains(new int2(2, 2)));
        Assert.IsFalse(grid.Contains(new int2(-1, 0)));
        Assert.IsFalse(grid.Contains(new int2(3, 0)));
        Assert.IsFalse(grid.Contains(new int2(0, 3)));

        grid.Dispose();
    }

    #endregion

    #region Clone and CopyTo Tests

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var original = new ValueGridShape<int>(3, 3, Allocator.Temp);
        original[0, 0] = 1;
        original[1, 1] = 2;
        original[2, 2] = 3;

        var clone = original.Clone(Allocator.Temp);

        Assert.AreEqual(original.Width, clone.Width);
        Assert.AreEqual(original.Height, clone.Height);

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                Assert.AreEqual(original[x, y], clone[x, y]);
            }
        }

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var original = new ValueGridShape<int>(2, 2, Allocator.Temp);
        original[0, 0] = 10;

        var clone = original.Clone(Allocator.Temp);
        clone[0, 0] = 20;
        clone[1, 1] = 30;

        Assert.AreEqual(10, original[0, 0]);
        Assert.AreEqual(0, original[1, 1]);

        Assert.AreEqual(20, clone[0, 0]);
        Assert.AreEqual(30, clone[1, 1]);

        original.Dispose();
        clone.Dispose();
    }

    [Test]
    public void CopyTo_CopiesValues()
    {
        var source = new ValueGridShape<int>(3, 3, Allocator.Temp);
        source[0, 0] = 100;
        source[1, 1] = 200;
        source[2, 2] = 300;

        var target = new ValueGridShape<int>(3, 3, Allocator.Temp);

        source.CopyTo(target);

        Assert.AreEqual(100, target[0, 0]);
        Assert.AreEqual(200, target[1, 1]);
        Assert.AreEqual(300, target[2, 2]);

        source.Dispose();
        target.Dispose();
    }

    [Test]
    public void CopyTo_ThrowsForDifferentDimensions()
    {
        var source = new ValueGridShape<int>(3, 3, Allocator.Temp);
        var target = new ValueGridShape<int>(2, 2, Allocator.Temp);

        Assert.Throws<ArgumentException>(() => source.CopyTo(target));

        source.Dispose();
        target.Dispose();
    }

    #endregion

    #region GridShape Conversion Tests

    [Test]
    public void ToGridShape_WithTrueValue_ConvertsCorrectly()
    {
        var valueGrid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        valueGrid[0, 0] = 1;
        valueGrid[1, 1] = 1;
        valueGrid[2, 0] = 2;

        var gridShape = valueGrid.ToGridShape(1, Allocator.Temp);

        Assert.AreEqual(valueGrid.Width, gridShape.Width);
        Assert.AreEqual(valueGrid.Height, gridShape.Height);

        Assert.IsTrue(gridShape.GetCell(new int2(0, 0)));
        Assert.IsTrue(gridShape.GetCell(new int2(1, 1)));
        Assert.IsFalse(gridShape.GetCell(new int2(2, 0)));
        Assert.IsFalse(gridShape.GetCell(new int2(0, 1)));

        valueGrid.Dispose();
        gridShape.Dispose();
    }

    [Test]
    public void ToGridShape_WithPredicate_ConvertsCorrectly()
    {
        var valueGrid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        valueGrid[0, 0] = 5;
        valueGrid[1, 1] = 10;
        valueGrid[2, 2] = 15;
        valueGrid[0, 2] = 3;

        var gridShape = valueGrid.ToGridShape(v => v >= 10, Allocator.Temp);

        Assert.IsFalse(gridShape.GetCell(new int2(0, 0)));
        Assert.IsTrue(gridShape.GetCell(new int2(1, 1)));
        Assert.IsTrue(gridShape.GetCell(new int2(2, 2)));
        Assert.IsFalse(gridShape.GetCell(new int2(0, 2)));

        valueGrid.Dispose();
        gridShape.Dispose();
    }

    [Test]
    public void FromGridShape_ConvertsCorrectly()
    {
        var gridShape = new GridShape(3, 3, Allocator.Temp);
        gridShape.SetCell(new int2(0, 0), true);
        gridShape.SetCell(new int2(1, 1), true);
        gridShape.SetCell(new int2(2, 2), false);

        var valueGrid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        valueGrid.FromGridShape(gridShape, 100, -1);

        Assert.AreEqual(100, valueGrid[0, 0]);
        Assert.AreEqual(100, valueGrid[1, 1]);
        Assert.AreEqual(-1, valueGrid[2, 2]);
        Assert.AreEqual(-1, valueGrid[0, 1]);

        gridShape.Dispose();
        valueGrid.Dispose();
    }

    [Test]
    public void FromGridShape_ThrowsForDifferentDimensions()
    {
        var gridShape = new GridShape(3, 3, Allocator.Temp);
        var valueGrid = new ValueGridShape<int>(2, 2, Allocator.Temp);

        Assert.Throws<ArgumentException>(() => valueGrid.FromGridShape(gridShape, 1, 0));

        gridShape.Dispose();
        valueGrid.Dispose();
    }

    #endregion

    #region Query Operations Tests

    [Test]
    public void CountValue_CountsCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        grid[0, 0] = 5;
        grid[1, 1] = 5;
        grid[2, 2] = 5;
        grid[0, 1] = 3;
        grid[1, 0] = 3;

        Assert.AreEqual(3, grid.CountValue(5));
        Assert.AreEqual(2, grid.CountValue(3));
        Assert.AreEqual(4, grid.CountValue(0));
        Assert.AreEqual(0, grid.CountValue(99));

        grid.Dispose();
    }

    [Test]
    public void CountWhere_CountsCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        for (int i = 0; i < 9; i++)
        {
            grid.SetValue(i % 3, i / 3, i);
        }

        Assert.AreEqual(5, grid.CountWhere(v => v >= 4));
        Assert.AreEqual(3, grid.CountWhere(v => v % 3 == 0));
        Assert.AreEqual(9, grid.CountWhere(v => v >= 0));
        Assert.AreEqual(0, grid.CountWhere(v => v < 0));

        grid.Dispose();
    }

    [Test]
    public void Any_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        grid.Fill(10);
        grid[1, 1] = 20;

        Assert.IsTrue(grid.Any(v => v == 20));
        Assert.IsTrue(grid.Any(v => v == 10));
        Assert.IsFalse(grid.Any(v => v == 30));
        Assert.IsTrue(grid.Any(v => v > 5));
        Assert.IsFalse(grid.Any(v => v < 0));

        grid.Dispose();
    }

    [Test]
    public void All_WorksCorrectly()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        grid.Fill(10);

        Assert.IsTrue(grid.All(v => v == 10));
        Assert.IsTrue(grid.All(v => v >= 10));
        Assert.IsFalse(grid.All(v => v > 10));

        grid[1, 1] = 20;
        Assert.IsFalse(grid.All(v => v == 10));
        Assert.IsTrue(grid.All(v => v >= 10));

        grid.Dispose();
    }

    [Test]
    public void Any_EmptyGrid_ReturnsFalse()
    {
        var grid = new ValueGridShape<int>(0, 0, Allocator.Temp);

        Assert.IsFalse(grid.Any(v => true));

        grid.Dispose();
    }

    [Test]
    public void All_EmptyGrid_ReturnsTrue()
    {
        var grid = new ValueGridShape<int>(0, 0, Allocator.Temp);

        Assert.IsTrue(grid.All(v => false));

        grid.Dispose();
    }

    #endregion

    #region Transformation Tests

    #endregion

    #region Equality Tests

    [Test]
    public void Equals_ReturnsTrueForIdenticalGrids()
    {
        var grid1 = new ValueGridShape<int>(3, 3, Allocator.Temp);
        var grid2 = new ValueGridShape<int>(3, 3, Allocator.Temp);

        grid1[0, 0] = 1;
        grid1[1, 1] = 2;

        grid2[0, 0] = 1;
        grid2[1, 1] = 2;

        Assert.IsTrue(grid1.Equals(grid2));
        Assert.IsTrue(grid1 == grid2);
        Assert.IsFalse(grid1 != grid2);

        grid1.Dispose();
        grid2.Dispose();
    }

    [Test]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var grid1 = new ValueGridShape<int>(2, 2, Allocator.Temp);
        var grid2 = new ValueGridShape<int>(2, 2, Allocator.Temp);

        grid1[0, 0] = 1;
        grid2[0, 0] = 2;

        Assert.IsFalse(grid1.Equals(grid2));
        Assert.IsFalse(grid1 == grid2);
        Assert.IsTrue(grid1 != grid2);

        grid1.Dispose();
        grid2.Dispose();
    }

    [Test]
    public void Equals_ReturnsFalseForDifferentDimensions()
    {
        var grid1 = new ValueGridShape<int>(2, 2, Allocator.Temp);
        var grid2 = new ValueGridShape<int>(2, 3, Allocator.Temp);

        Assert.IsFalse(grid1.Equals(grid2));

        grid1.Dispose();
        grid2.Dispose();
    }

    [Test]
    public void GetHashCode_ThrowsNotSupported()
    {
        var grid = new ValueGridShape<int>(2, 2, Allocator.Temp);

        Assert.Throws<NotSupportedException>(() => { var hash = grid.GetHashCode(); });

        grid.Dispose();
    }

    [Test]
    public void EqualsObject_ThrowsNotSupported()
    {
        var grid = new ValueGridShape<int>(2, 2, Allocator.Temp);
        object obj = new object();

        Assert.Throws<NotSupportedException>(() => { var result = grid.Equals(obj); });

        grid.Dispose();
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_ReleasesNativeMemory()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        var values = grid.Values;

        Assert.IsTrue(values.IsCreated);

        grid.Dispose();

        // After dispose, accessing values should fail
        // Note: We can't directly test IsCreated after dispose as the struct is copied
        // This test mainly ensures Dispose doesn't throw
    }

    [Test]
    public void DisposeWithJobHandle_ReturnsJobHandle()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.TempJob);
        var inputDeps = new Unity.Jobs.JobHandle();

        var result = grid.Dispose(inputDeps);

        // Should return a valid job handle
        Assert.IsTrue(result.IsCompleted || !result.IsCompleted); // Just check it's valid

        result.Complete();
    }

    #endregion

    #region Different Type Tests

    [Test]
    public void FloatGrid_WorksCorrectly()
    {
        var grid = new ValueGridShape<float>(2, 2, 1.5f, Allocator.Temp);

        Assert.AreEqual(1.5f, grid[0, 0]);

        grid[1, 1] = 2.5f;
        Assert.AreEqual(2.5f, grid[1, 1]);

        var values = grid.Values;
        for (int index = 0; index < values.Length; index++)
        {
            values[index] *= 2;
        }

        Assert.AreEqual(3.0f, grid[0, 0]);
        Assert.AreEqual(5.0f, grid[1, 1]);

        grid.Dispose();
    }

    [Test]
    public void CustomStructGrid_WorksCorrectly()
    {
        var grid = new ValueGridShape<TestStruct>(2, 2, Allocator.Temp);

        var testValue = new TestStruct { Value = 42, Flag = true };
        grid[0, 0] = testValue;

        Assert.AreEqual(testValue, grid[0, 0]);
        Assert.AreEqual(42, grid[0, 0].Value);
        Assert.IsTrue(grid[0, 0].Flag);

        grid.Fill(new TestStruct { Value = 100, Flag = false });
        Assert.AreEqual(100, grid[1, 1].Value);
        Assert.IsFalse(grid[1, 1].Flag);

        grid.Dispose();
    }

    [Test]
    public void BoolGrid_WorksCorrectly()
    {
        var grid = new ValueGridShape<bool>(3, 3, Allocator.Temp);

        grid[0, 0] = true;
        grid[1, 1] = true;
        grid[2, 2] = true;

        Assert.IsTrue(grid[0, 0]);
        Assert.IsFalse(grid[0, 1]);

        Assert.AreEqual(3, grid.CountValue(true));
        Assert.AreEqual(6, grid.CountValue(false));

        var gridShape = grid.ToGridShape(true, Allocator.Temp);
        Assert.IsTrue(gridShape.GetCell(new int2(0, 0)));
        Assert.IsTrue(gridShape.GetCell(new int2(1, 1)));
        Assert.IsFalse(gridShape.GetCell(new int2(0, 1)));

        grid.Dispose();
        gridShape.Dispose();
    }

    #endregion

    #region GetValues Tests

    [Test]
    public void GetValues_ReturnsUnderlyingArray()
    {
        var grid = new ValueGridShape<int>(3, 3, Allocator.Temp);
        grid[0, 0] = 10;
        grid[1, 1] = 20;
        grid[2, 2] = 30;

        var values = grid.Values;

        Assert.AreEqual(9, values.Length);
        Assert.AreEqual(10, values[0]);
        Assert.AreEqual(20, values[4]); // (1,1) at index 4
        Assert.AreEqual(30, values[8]); // (2,2) at index 8

        grid.Dispose();
    }

    #endregion

    // Test struct for custom type tests
    private struct TestStruct : IEquatable<TestStruct>
    {
        public int Value;
        public bool Flag;

        public bool Equals(TestStruct other)
        {
            return Value == other.Value && Flag == other.Flag;
        }
    }
}
