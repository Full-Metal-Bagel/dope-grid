using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class ValueGridShapeTests
{
    [Test]
    public void Constructor_CreatesShapeWithCorrectDimensions()
    {
        using var shape = new ValueGridShape<int>(3, 4);

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(4));
        Assert.That(shape.Size, Is.EqualTo(12));
    }

    [Test]
    public void Constructor_WithAvailableValue_SetsCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3, availableValue: 0);

        // All cells should be 0 (the available value) initially
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.EqualTo(0));
            Assert.That(shape.IsOccupied(x, y), Is.False, $"Cell [{x},{y}] should not be occupied initially");
        }
    }

    [Test]
    public void Constructor_WithDefaultAndAvailableValue_FillsCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape.FillAll(5);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.EqualTo(5));
            Assert.That(shape.IsOccupied(x, y), Is.True);
        }
    }

    [Test]
    public void Indexer_SetAndGet_WorksCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        shape[0, 0] = 10;
        shape[2, 2] = 20;
        shape[1, 1] = 30;

        Assert.That(shape[0, 0], Is.EqualTo(10));
        Assert.That(shape[2, 2], Is.EqualTo(20));
        Assert.That(shape[1, 1], Is.EqualTo(30));
        Assert.That(shape[1, 0], Is.EqualTo(0));
    }

    [Test]
    public void IsOccupied_DetectsOccupiedCells()
    {
        using var shape = new ValueGridShape<int>(3, 3, availableValue: 0);

        shape[1, 1] = 5;
        shape[2, 2] = 10;

        Assert.That(shape.IsOccupied(1, 1), Is.True);
        Assert.That(shape.IsOccupied(2, 2), Is.True);
        Assert.That(shape.IsOccupied(0, 0), Is.False);
        Assert.That(shape.IsOccupied(1, 0), Is.False);
    }

    [Test]
    public void Clear_ResetsAllCellsToAvailableValue()
    {
        using var shape = new ValueGridShape<int>(3, 3, availableValue: -1);

        shape[0, 0] = 10;
        shape[1, 1] = 20;
        shape[2, 2] = 30;

        shape.Clear();

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.EqualTo(-1));
            Assert.That(shape.IsOccupied(x, y), Is.False);
        }
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        using var original = new ValueGridShape<int>(3, 3);
        original[0, 0] = 10;
        original[1, 1] = 20;

        using var clone = original.Clone();

        Assert.That(clone.Width, Is.EqualTo(original.Width));
        Assert.That(clone.Height, Is.EqualTo(original.Height));
        Assert.That(clone[0, 0], Is.EqualTo(10));
        Assert.That(clone[1, 1], Is.EqualTo(20));

        // Modify clone
        clone[2, 2] = 30;

        Assert.That(clone[2, 2], Is.EqualTo(30));
        Assert.That(original[2, 2], Is.EqualTo(0));
    }

    [Test]
    public void CopyTo_CopiesAllValues()
    {
        using var source = new ValueGridShape<int>(3, 3);
        using var dest = new ValueGridShape<int>(3, 3);

        source[0, 0] = 10;
        source[1, 1] = 20;
        source[2, 2] = 30;

        source.CopyTo(dest);

        Assert.That(dest[0, 0], Is.EqualTo(10));
        Assert.That(dest[1, 1], Is.EqualTo(20));
        Assert.That(dest[2, 2], Is.EqualTo(30));
    }

    [Test]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 0] = 10;
        shape1[1, 1] = 20;

        shape2[0, 0] = 10;
        shape2[1, 1] = 20;

        Assert.That(shape1 == shape2, Is.True);
        Assert.That(shape1.Equals(shape2), Is.True);
    }

    [Test]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 0] = 10;
        shape2[1, 1] = 20;

        Assert.That(shape1 != shape2, Is.True);
        Assert.That(shape1.Equals(shape2), Is.False);
    }

    [Test]
    public void GetHashCode_ThrowsNotSupportedException()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.Throws<NotSupportedException>(() => shape.GetHashCode());
    }

    [Test]
    public void Equals_Object_ThrowsNotSupportedException()
    {
        using var shape = new ValueGridShape<int>(3, 3);

        Assert.Throws<NotSupportedException>(() => shape.Equals((object)shape));
    }

    [Test]
    public void AsReadOnly_CreatesReadOnlyView()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[1, 1] = 10;

        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly.Width, Is.EqualTo(3));
        Assert.That(readOnly.Height, Is.EqualTo(3));
        Assert.That(readOnly[1, 1], Is.EqualTo(10));
        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
    }

    [Test]
    public void ImplicitConversion_ToReadOnly_Works()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[2, 2] = 20;

        ValueGridShape<int>.ReadOnly readOnly = shape;

        Assert.That(readOnly[2, 2], Is.EqualTo(20));
    }

    [Test]
    public void ZeroSizeShape_WorksCorrectly()
    {
        using var shape = new ValueGridShape<int>(0, 0);

        Assert.That(shape.Width, Is.EqualTo(0));
        Assert.That(shape.Height, Is.EqualTo(0));
        Assert.That(shape.Size, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_ReleasesResources()
    {
        var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 10;

        shape.Dispose();

        // Should not throw
        Assert.Pass("Dispose completed successfully");
    }

    // ReadOnly tests
    [Test]
    public void ReadOnly_Indexer_ReturnsCorrectValue()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        shape[0, 0] = 10;
        shape[2, 2] = 20;

        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly[0, 0], Is.EqualTo(10));
        Assert.That(readOnly[2, 2], Is.EqualTo(20));
    }

    [Test]
    public void ReadOnly_IsOccupied_WorksCorrectly()
    {
        using var shape = new ValueGridShape<int>(3, 3, availableValue: 0);
        shape[1, 1] = 5;

        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
        Assert.That(readOnly.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void ReadOnly_CopyTo_CopiesValues()
    {
        using var source = new ValueGridShape<int>(3, 3);
        source[0, 0] = 10;
        source[1, 1] = 20;

        var readOnly = source.AsReadOnly();

        using var dest = new ValueGridShape<int>(3, 3);
        readOnly.CopyTo(dest);

        Assert.That(dest[0, 0], Is.EqualTo(10));
        Assert.That(dest[1, 1], Is.EqualTo(20));
    }

    [Test]
    public void ReadOnly_Clone_CreatesNewShape()
    {
        using var original = new ValueGridShape<int>(3, 3);
        original[1, 1] = 15;

        var readOnly = original.AsReadOnly();
        using var clone = readOnly.Clone();

        Assert.That(clone[1, 1], Is.EqualTo(15));
    }

    [Test]
    public void ReadOnly_Equals_WithSameValues_ReturnsTrue()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 0] = 10;
        shape2[0, 0] = 10;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.That(readOnly1 == readOnly2, Is.True);
        Assert.That(readOnly1.Equals(readOnly2), Is.True);
    }

    [Test]
    public void ReadOnly_Equals_WithDifferentValues_ReturnsFalse()
    {
        using var shape1 = new ValueGridShape<int>(3, 3);
        using var shape2 = new ValueGridShape<int>(3, 3);

        shape1[0, 0] = 10;
        shape2[1, 1] = 20;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.That(readOnly1 != readOnly2, Is.True);
        Assert.That(readOnly1.Equals(readOnly2), Is.False);
    }

    [Test]
    public void ReadOnly_GetHashCode_ThrowsNotSupportedException()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        var readOnly = shape.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly.GetHashCode());
    }

    [Test]
    public void ReadOnly_Equals_Object_ThrowsNotSupportedException()
    {
        using var shape = new ValueGridShape<int>(3, 3);
        var readOnly = shape.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly.Equals((object)readOnly));
    }

    [Test]
    public void DifferentGenericTypes_WorkCorrectly()
    {
        using var byteShape = new ValueGridShape<byte>(3, 3);
        byteShape[0, 0] = 255;
        Assert.That(byteShape[0, 0], Is.EqualTo(255));

        using var charShape = new ValueGridShape<char>(3, 3, availableValue: ' ');
        charShape[1, 1] = 'A';
        Assert.That(charShape[1, 1], Is.EqualTo('A'));
        Assert.That(charShape.IsOccupied(1, 1), Is.True);
    }
}
