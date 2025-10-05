using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class GridExtensionsTests
{
    // BoolGridShapeExtension tests
    [Test]
    public void BoolGridShapeExtension_CopyTo_CopiesCorrectly()
    {
        using var source = new GridShape(3, 3);
        using var target = new GridShape(3, 3);

        source[0, 0] = true;
        source[1, 1] = true;
        source[2, 2] = true;

        source.CopyTo(target);

        Assert.That(target[0, 0], Is.True);
        Assert.That(target[1, 1], Is.True);
        Assert.That(target[2, 2], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_SetCellValue_SetsCorrectly()
    {
        using var grid = new GridShape(3, 3);

        grid.SetCellValue(1, 1, true);

        Assert.That(grid[1, 1], Is.True);
    }

    [Test]
    public void BoolGridShapeExtension_FillAll_FillsAllCells()
    {
        using var grid = new GridShape(3, 3);

        grid.FillAll(true);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(grid[x, y], Is.True);
        }
    }

    [Test]
    public void BoolGridShapeExtension_FillRect_FillsRectangle()
    {
        using var grid = new GridShape(5, 5);

        grid.FillRect(1, 1, 3, 2, true);

        Assert.That(grid[1, 1], Is.True);
        Assert.That(grid[2, 1], Is.True);
        Assert.That(grid[3, 1], Is.True);
        Assert.That(grid[1, 2], Is.True);
        Assert.That(grid[0, 0], Is.False);
    }

    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_FillsShape()
    {
        using var grid = new GridShape(5, 5);
        using var shape = Shapes.Single();

        grid.FillShapeWithValue(shape, 2, 2, true);

        Assert.That(grid[2, 2], Is.True);
        Assert.That(grid[1, 1], Is.False);
    }

    // GridShapeExtension tests
    [Test]
    public void GridShapeExtension_SetCellValue_WorksWithInt()
    {
        using var grid = new ValueGridShape<int>(3, 3);

        grid.SetCellValue(1, 1, 42);

        Assert.That(grid[1, 1], Is.EqualTo(42));
    }

    [Test]
    public void GridShapeExtension_FillAll_WorksWithInt()
    {
        using var grid = new ValueGridShape<int>(3, 3);

        grid.FillAll(7);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(grid[x, y], Is.EqualTo(7));
        }
    }

    [Test]
    public void GridShapeExtension_FillRect_WorksWithInt()
    {
        using var grid = new ValueGridShape<int>(5, 5);

        grid.FillRect(1, 1, 2, 2, 99);

        Assert.That(grid[1, 1], Is.EqualTo(99));
        Assert.That(grid[2, 2], Is.EqualTo(99));
        Assert.That(grid[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void GridShapeExtension_FillShapeWithValue_WorksWithInt()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new ValueGridShape<int>(2, 2, defaultValue: 1, availableValue: 0);

        grid.FillShapeWithValue(shape, 1, 1, 42);

        Assert.That(grid[1, 1], Is.EqualTo(42));
        Assert.That(grid[2, 2], Is.EqualTo(42));
    }

    [Test]
    public void GridShapeExtension_CopyTo_WorksWithInt()
    {
        using var source = new ValueGridShape<int>(3, 3);
        using var target = new ValueGridShape<int>(3, 3);

        source[0, 0] = 10;
        source[1, 1] = 20;

        source.CopyTo(target);

        Assert.That(target[0, 0], Is.EqualTo(10));
        Assert.That(target[1, 1], Is.EqualTo(20));
    }

    // ReadOnlyBoolGridShapeExtensions tests
    [Test]
    public void ReadOnlyBoolGridShapeExtensions_OccupiedSpaceCount_CountsOccupied()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        var readOnly = shape.AsReadOnly();
        var count = readOnly.OccupiedSpaceCount();

        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void ReadOnlyBoolGridShapeExtensions_FreeSpaceCount_CountsFree()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;

        var readOnly = shape.AsReadOnly();
        var count = readOnly.FreeSpaceCount();

        Assert.That(count, Is.EqualTo(8));
    }

    [Test]
    public void ReadOnlyBoolGridShapeExtensions_FreeSpaceCount_WorksCorrectly()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly.FreeSpaceCount(), Is.EqualTo(8));
        Assert.That(readOnly.OccupiedSpaceCount(), Is.EqualTo(1));
    }

    [Test]
    public void ReadOnlyBoolGridShapeExtensions_Clone_CreatesIndependentCopy()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        var readOnly = shape.AsReadOnly();
        using var cloned = readOnly.Clone();

        Assert.That(cloned[1, 1], Is.True);
        Assert.That(cloned.Width, Is.EqualTo(3));
        Assert.That(cloned.Height, Is.EqualTo(3));
    }

    [Test]
    public void ReadOnlyBoolGridShapeExtensions_CopyTo_CopiesData()
    {
        using var source = new GridShape(3, 3);
        using var target = new GridShape(3, 3);

        source[0, 0] = true;
        source[2, 2] = true;

        var readOnly = source.AsReadOnly();
        readOnly.CopyTo(target);

        Assert.That(target[0, 0], Is.True);
        Assert.That(target[2, 2], Is.True);
    }

    [Test]
    public void ReadOnlyBoolGridShapeExtensions_Clone_CreatesNewShape()
    {
        using var original = new GridShape(3, 3);
        original[1, 1] = true;

        var readOnly = original.AsReadOnly();
        using var clone = readOnly.Clone();

        Assert.That(clone[1, 1], Is.True);
        Assert.That(clone.Width, Is.EqualTo(3));
    }

    // ReadOnlyGridShapeExtensions tests
    [Test]
    public void ReadOnlyGridShapeExtensions_GetIndex_CalculatesCorrectly()
    {
        using var shape = new GridShape(5, 5);

        var readOnly = shape.AsReadOnly();
        var index = readOnly.GetIndex(2, 3);

        Assert.That(index, Is.EqualTo(3 * 5 + 2));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_GetIndex_CalculatesCorrectly_WithValidation()
    {
        using var shape = new GridShape(5, 5);

        var readOnly = shape.AsReadOnly();
        var index1 = readOnly.GetIndex(0, 0);
        var index2 = readOnly.GetIndex(4, 4);
        var index3 = readOnly.GetIndex(2, 3);

        Assert.That(index1, Is.EqualTo(0));
        Assert.That(index2, Is.EqualTo(24));
        Assert.That(index3, Is.EqualTo(17));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_HasSameSize_ComparesSizes()
    {
        using var shape1 = new GridShape(5, 5);
        using var shape2 = new GridShape(5, 5);
        using var shape3 = new GridShape(3, 3);

        var readOnly1 = shape1.AsReadOnly();

        Assert.That(readOnly1.HasSameSize(shape2), Is.True);
        Assert.That(readOnly1.HasSameSize(shape3), Is.False);
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_HasSameSize_DetectsSameSize()
    {
        using var shape1 = new GridShape(5, 5);
        using var shape2 = new GridShape(5, 5);

        var readOnly1 = shape1.AsReadOnly();

        Assert.That(readOnly1.HasSameSize(shape2), Is.True);
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_HasSameSize_DetectsDifferentSize()
    {
        using var shape1 = new GridShape(5, 5);
        using var shape2 = new GridShape(3, 3);

        var readOnly1 = shape1.AsReadOnly();

        Assert.That(readOnly1.HasSameSize(shape2), Is.False);
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_Clone_WorksWithValueGrid()
    {
        using var original = new ValueGridShape<int>(3, 3);
        original[1, 1] = 42;

        var readOnly = original.AsReadOnly();
        using var clone = readOnly.Clone();

        Assert.That(clone[1, 1], Is.EqualTo(42));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_CopyTo_WorksWithValueGrid()
    {
        using var source = new ValueGridShape<int>(3, 3);
        using var target = new ValueGridShape<int>(3, 3);

        source[0, 0] = 99;

        var readOnly = source.AsReadOnly();
        readOnly.CopyTo(target);

        Assert.That(target[0, 0], Is.EqualTo(99));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_IsValuesEquals_ComparesCorrectly()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 0] = 5;
        shape2[0, 0] = 5;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.That(readOnly1.IsValuesEquals(readOnly2, 0), Is.True);
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_OccupiedPositions_ReturnsOccupiedCells()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        // OccupiedSpaceCount is the available method on ReadOnly
        var readOnly = shape.AsReadOnly();
        var count = readOnly.OccupiedSpaceCount();

        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void ReadOnlyGridShapeExtensions_CopyTo_WorksWithDifferentTypes()
    {
        using var source = new GridShape(3, 3);
        source[0, 0] = true;
        source[1, 1] = true;

        // Test CopyTo functionality
        var readOnly = source.AsReadOnly();
        using var target = new GridShape(3, 3);

        readOnly.CopyTo(target);

        Assert.That(target[0, 0], Is.True);
        Assert.That(target[1, 1], Is.True);
        Assert.That(target[2, 2], Is.False);
    }

    // Additional GridShapeExtension tests for better coverage
    [Test]
    public void GridShapeExtension_CopyTo_WithWidthHeight_CopiesSubrectangle()
    {
        using var source = new ValueGridShape<int>(5, 5);
        source[0, 0] = 10;
        source[1, 1] = 20;
        using var target = new ValueGridShape<int>(2, 2);

        source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(0, 0, 2, 2, target);

        Assert.That(target[0, 0], Is.EqualTo(10));
        Assert.That(target[1, 1], Is.EqualTo(20));
    }

    [Test]
    public void GridShapeExtension_CopyTo_WithOffset_ThrowsOnNegativeValues()
    {
        using var source = new ValueGridShape<int>(5, 5);
        using var target = new ValueGridShape<int>(2, 2);

        Assert.Throws<ArgumentException>(() => source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(-1, 0, 2, 2, target));
        Assert.Throws<ArgumentException>(() => source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(0, -1, 2, 2, target));
        Assert.Throws<ArgumentException>(() => source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(0, 0, -1, 2, target));
        Assert.Throws<ArgumentException>(() => source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(0, 0, 2, -1, target));
    }

    [Test]
    public void GridShapeExtension_CopyTo_WithOffset_ThrowsOnOutOfBounds()
    {
        using var source = new ValueGridShape<int>(5, 5);
        using var target = new ValueGridShape<int>(10, 10);

        Assert.Throws<ArgumentException>(() => source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(4, 4, 2, 2, target));
    }

    [Test]
    public void GridShapeExtension_CopyTo_WithOffset_CopiesCorrectly()
    {
        using var source = new ValueGridShape<int>(5, 5);
        source[2, 2] = 42;
        source[3, 3] = 99;
        using var target = new ValueGridShape<int>(2, 2);

        source.CopyTo<ValueGridShape<int>, ValueGridShape<int>, int>(2, 2, 2, 2, target);

        Assert.That(target[0, 0], Is.EqualTo(42));
        Assert.That(target[1, 1], Is.EqualTo(99));
    }

    [Test]
    public void GridShapeExtension_FillShape_WithValueType_FillsCorrectly()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new ValueGridShape<int>(2, 2);
        shape[0, 0] = 10;
        shape[1, 1] = 20;

        grid.FillShape<ValueGridShape<int>, ValueGridShape<int>, int>(shape, 1, 1);

        Assert.That(grid[1, 1], Is.EqualTo(10));
        Assert.That(grid[2, 2], Is.EqualTo(20));
        Assert.That(grid[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void GridShapeExtension_FillShape_WithNegativeOffset_ClipsCorrectly()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new ValueGridShape<int>(3, 3, defaultValue: 1, availableValue: 0);

        grid.FillShape<ValueGridShape<int>, ValueGridShape<int>, int>(shape, -1, -1);

        // Only the bottom-right 2x2 portion of shape should be visible
        Assert.That(grid[0, 0], Is.EqualTo(1));
        Assert.That(grid[1, 1], Is.EqualTo(1));
    }

    [Test]
    public void GridShapeExtension_FillShape_WithOffsetBeyondGrid_ClipsCorrectly()
    {
        using var grid = new ValueGridShape<int>(5, 5);
        using var shape = new ValueGridShape<int>(3, 3, defaultValue: 99, availableValue: 0);

        grid.FillShape<ValueGridShape<int>, ValueGridShape<int>, int>(shape, 4, 4);

        // Only top-left corner of shape should be visible at [4,4]
        Assert.That(grid[4, 4], Is.EqualTo(99));
        Assert.That(grid[3, 3], Is.EqualTo(0));
    }

    [Test]
    public void GridShapeExtension_FillShapeWithValue_WithNegativeOffset_UsesCorrectStartY()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(3, 3);
        shape.FillAll(true);

        grid.FillShapeWithValue(shape, -1, -1, true);

        Assert.That(grid[0, 0], Is.True);
        Assert.That(grid[1, 1], Is.True);
    }

    [Test]
    public void GridShapeExtension_RotateShape_WithInvalidDegree_ThrowsException()
    {
        using var input = new ValueGridShape<int>(3, 3);
        using var output = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentException>(() => input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>((RotationDegree)999, output));
    }

    [Test]
    public void GridShapeExtension_RotateShape_WithMismatchedSize_ThrowsException()
    {
        using var input = new ValueGridShape<int>(3, 3);
        using var output = new ValueGridShape<int>(2, 2);

        Assert.Throws<ArgumentException>(() => input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise90, output));
    }

    [Test]
    public void GridShapeExtension_RotateShape_Clockwise90_RotatesCorrectly()
    {
        using var input = new ValueGridShape<int>(2, 3);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;
        input[0, 2] = 5;
        input[1, 2] = 6;

        using var output = new ValueGridShape<int>(3, 2);
        input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise90, output);

        Assert.That(output[0, 0], Is.EqualTo(5));
        Assert.That(output[1, 0], Is.EqualTo(3));
        Assert.That(output[2, 0], Is.EqualTo(1));
        Assert.That(output[0, 1], Is.EqualTo(6));
        Assert.That(output[1, 1], Is.EqualTo(4));
        Assert.That(output[2, 1], Is.EqualTo(2));
    }

    [Test]
    public void GridShapeExtension_RotateShape_Clockwise180_RotatesCorrectly()
    {
        using var input = new ValueGridShape<int>(2, 2);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;

        using var output = new ValueGridShape<int>(2, 2);
        input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise180, output);

        Assert.That(output[0, 0], Is.EqualTo(4));
        Assert.That(output[1, 0], Is.EqualTo(3));
        Assert.That(output[0, 1], Is.EqualTo(2));
        Assert.That(output[1, 1], Is.EqualTo(1));
    }

    [Test]
    public void GridShapeExtension_RotateShape_Clockwise270_RotatesCorrectly()
    {
        using var input = new ValueGridShape<int>(2, 3);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;
        input[0, 2] = 5;
        input[1, 2] = 6;

        using var output = new ValueGridShape<int>(3, 2);
        input.RotateShape<ValueGridShape<int>, ValueGridShape<int>, int>(RotationDegree.Clockwise270, output);

        Assert.That(output[0, 0], Is.EqualTo(2));
        Assert.That(output[1, 0], Is.EqualTo(4));
        Assert.That(output[2, 0], Is.EqualTo(6));
        Assert.That(output[0, 1], Is.EqualTo(1));
        Assert.That(output[1, 1], Is.EqualTo(3));
        Assert.That(output[2, 1], Is.EqualTo(5));
    }

    [Test]
    public void GridShapeExtension_FlipShape_Horizontal_FlipsCorrectly()
    {
        using var input = new ValueGridShape<int>(3, 2);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[2, 0] = 3;
        input[0, 1] = 4;
        input[1, 1] = 5;
        input[2, 1] = 6;

        using var output = new ValueGridShape<int>(3, 2);
        input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>(FlipAxis.Horizontal, output);

        Assert.That(output[0, 0], Is.EqualTo(3));
        Assert.That(output[1, 0], Is.EqualTo(2));
        Assert.That(output[2, 0], Is.EqualTo(1));
        Assert.That(output[0, 1], Is.EqualTo(6));
        Assert.That(output[1, 1], Is.EqualTo(5));
        Assert.That(output[2, 1], Is.EqualTo(4));
    }

    [Test]
    public void GridShapeExtension_FlipShape_Vertical_FlipsCorrectly()
    {
        using var input = new ValueGridShape<int>(2, 3);
        input[0, 0] = 1;
        input[1, 0] = 2;
        input[0, 1] = 3;
        input[1, 1] = 4;
        input[0, 2] = 5;
        input[1, 2] = 6;

        using var output = new ValueGridShape<int>(2, 3);
        input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>(FlipAxis.Vertical, output);

        Assert.That(output[0, 0], Is.EqualTo(5));
        Assert.That(output[1, 0], Is.EqualTo(6));
        Assert.That(output[0, 1], Is.EqualTo(3));
        Assert.That(output[1, 1], Is.EqualTo(4));
        Assert.That(output[0, 2], Is.EqualTo(1));
        Assert.That(output[1, 2], Is.EqualTo(2));
    }

    [Test]
    public void GridShapeExtension_FlipShape_MismatchedSize_ThrowsException()
    {
        using var input = new ValueGridShape<int>(3, 3);
        using var output = new ValueGridShape<int>(2, 2);

        Assert.Throws<ArgumentException>(() => input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>(FlipAxis.Horizontal, output));
    }

    [Test]
    public void GridShapeExtension_FlipShape_InvalidAxis_ThrowsException()
    {
        using var input = new ValueGridShape<int>(3, 3);
        using var output = new ValueGridShape<int>(3, 3);

        Assert.Throws<ArgumentException>(() => input.FlipShape<ValueGridShape<int>, ValueGridShape<int>, int>((FlipAxis)999, output));
    }

    [Test]
    public void GridShapeExtension_Trim_TrimsCorrectly()
    {
        using var input = new ValueGridShape<int>(5, 5);
        input[1, 1] = 1;
        input[2, 1] = 2;
        input[1, 2] = 3;
        input[2, 2] = 4;

        using var output = new ValueGridShape<int>(2, 2);
        input.Trim<ValueGridShape<int>, ValueGridShape<int>, int>(output);

        Assert.That(output[0, 0], Is.EqualTo(1));
        Assert.That(output[1, 0], Is.EqualTo(2));
        Assert.That(output[0, 1], Is.EqualTo(3));
        Assert.That(output[1, 1], Is.EqualTo(4));
    }

    [Test]
    public void GridShapeExtension_Trim_MismatchedOutputSize_ThrowsException()
    {
        using var input = new ValueGridShape<int>(5, 5);
        input[1, 1] = 1;
        input[2, 1] = 1;
        using var output = new ValueGridShape<int>(1, 1);

        Assert.Throws<ArgumentException>(() => input.Trim<ValueGridShape<int>, ValueGridShape<int>, int>(output));
    }

    // BoolGridShapeExtension wrapper tests
    [Test]
    public void BoolGridShapeExtension_FillShapeWithValue_CallsGenericVersion()
    {
        using var grid = new GridShape(5, 5);
        using var shape = new GridShape(2, 2);
        shape[0, 0] = true;
        shape[1, 1] = true;

        grid.FillShapeWithValue(shape, 1, 1, true);

        Assert.That(grid[1, 1], Is.True);
        Assert.That(grid[2, 2], Is.True);
        Assert.That(grid[0, 0], Is.False);
    }
}
