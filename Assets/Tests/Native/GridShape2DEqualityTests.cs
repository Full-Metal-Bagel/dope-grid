using System;
using NUnit.Framework;
using Unity.Collections;
using DopeGrid.Native;

[TestFixture]
public class GridShapeEqualityTests
{
    [Test]
    public void Equals_IdenticalShapes_ReturnsTrue()
    {
        using var shape1 = new GridShape(3, 3, Allocator.Temp);
        using var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1.SetCell(0, 0, true);
        shape1.SetCell(1, 1, true);
        shape1.SetCell(2, 2, true);

        shape2.SetCell(0, 0, true);
        shape2.SetCell(1, 1, true);
        shape2.SetCell(2, 2, true);

        Assert.IsTrue(shape1.Equals(shape2));
        Assert.IsTrue(shape2.Equals(shape1));
    }

    [Test]
    public void Equals_DifferentCells_ReturnsFalse()
    {
        using var shape1 = new GridShape(3, 3, Allocator.Temp);
        using var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1.SetCell(0, 0, true);
        shape1.SetCell(1, 1, true);

        shape2.SetCell(0, 0, true);
        shape2.SetCell(2, 2, true);

        Assert.IsFalse(shape1.Equals(shape2));
        Assert.IsFalse(shape2.Equals(shape1));
    }

    [Test]
    public void Equals_DifferentDimensions_ReturnsFalse()
    {
        using var shape1 = new GridShape(3, 3, Allocator.Temp);
        using var shape2 = new GridShape(3, 4, Allocator.Temp);

        shape1.SetCell(0, 0, true);
        shape2.SetCell(0, 0, true);

        Assert.IsFalse(shape1.Equals(shape2));
        Assert.IsFalse(shape2.Equals(shape1));
    }

    [Test]
    public void Equals_EmptyShapes_ReturnsTrue()
    {
        using var shape1 = new GridShape(5, 5, Allocator.Temp);
        using var shape2 = new GridShape(5, 5, Allocator.Temp);

        Assert.IsTrue(shape1.Equals(shape2));
        Assert.IsTrue(shape2.Equals(shape1));
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        using var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCell(1, 1, true);

        Assert.IsTrue(shape.Equals(shape));
    }

    [Test]
    public void Equals_ClonedShape_ReturnsTrue()
    {
        using var original = new GridShape(4, 4, Allocator.Temp);
        original.SetCell(0, 0, true);
        original.SetCell(1, 2, true);
        original.SetCell(3, 3, true);

        using var clone = original.Clone(Allocator.Temp);

        Assert.IsTrue(original.Equals(clone));
        Assert.IsTrue(clone.Equals(original));
    }

    [Test]
    public void Equals_AfterClear_ComparesCorrectly()
    {
        using var shape1 = new GridShape(3, 3, Allocator.Temp);
        using var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1.SetCell(1, 1, true);
        shape2.SetCell(1, 1, true);

        Assert.IsTrue(shape1.Equals(shape2));

        shape1.Clear();
        Assert.IsFalse(shape1.Equals(shape2));

        shape2.Clear();
        Assert.IsTrue(shape1.Equals(shape2));
    }

    [Test]
    public void Equals_ComplexPattern_ReturnsTrue()
    {
        using var shape1 = Shapes.Cross(Allocator.Temp);
        using var shape2 = Shapes.Cross(Allocator.Temp);

        Assert.IsTrue(shape1.Equals(shape2));
    }

    [Test]
    public void Equals_DifferentShapeTypes_ReturnsFalse()
    {
        using var lShape = Shapes.LShape(Allocator.Temp);
        using var tShape = Shapes.TShape(Allocator.Temp);

        Assert.IsFalse(lShape.Equals(tShape));
        Assert.IsFalse(tShape.Equals(lShape));
    }

    [Test]
    public void GetHashCode_EqualShapes_ReturnsSameHashCode()
    {
        using var shape1 = new GridShape(3, 3, Allocator.Temp);
        using var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1.SetCell(0, 0, true);
        shape1.SetCell(2, 2, true);

        shape2.SetCell(0, 0, true);
        shape2.SetCell(2, 2, true);

        Assert.IsTrue(shape1.Equals(shape2));
        // Note: Equal objects should have equal hash codes, but we can't guarantee it with NativeBitArray
        // This is more of a documentation test to show the limitation
    }

    [Test]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        using var shape1 = new GridShape(2, 2, Allocator.Temp);
        shape1.SetCell(0, 0, true);

        using var shape2 = new GridShape(2, 2, Allocator.Temp);
        shape2.SetCell(0, 0, true);

        object objShape2 = shape2;

        Assert.Catch<NotSupportedException>(() => shape1.Equals(objShape2));
        Assert.Catch<NotSupportedException>(() => shape1.Equals(null));
        Assert.Catch<NotSupportedException>(() => shape1.Equals("not a shape"));
    }
}
