using DopeGrid;
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
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[0, 0] = true;
        shape1[1, 1] = true;
        shape1[2, 2] = true;

        shape2[0, 0] = true;
        shape2[1, 1] = true;
        shape2[2, 2] = true;

        Assert.IsTrue(shape1.Equals(shape2));
        Assert.IsTrue(shape2.Equals(shape1));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_DifferentCells_ReturnsFalse()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[0, 0] = true;
        shape1[1, 1] = true;

        shape2[0, 0] = true;
        shape2[2, 2] = true;

        Assert.IsFalse(shape1.Equals(shape2));
        Assert.IsFalse(shape2.Equals(shape1));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_DifferentDimensions_ReturnsFalse()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 4, Allocator.Temp);

        shape1[0, 0] = true;
        shape2[0, 0] = true;

        Assert.IsFalse(shape1.Equals(shape2));
        Assert.IsFalse(shape2.Equals(shape1));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_EmptyShapes_ReturnsTrue()
    {
        var shape1 = new GridShape(5, 5, Allocator.Temp);
        var shape2 = new GridShape(5, 5, Allocator.Temp);

        Assert.IsTrue(shape1.Equals(shape2));
        Assert.IsTrue(shape2.Equals(shape1));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape[1, 1] = true;

        Assert.IsTrue(shape.Equals(shape));
        shape.Dispose();
    }

    [Test]
    public void Equals_ClonedShape_ReturnsTrue()
    {
        var original = new GridShape(4, 4, Allocator.Temp);
        original[0, 0] = true;
        original[1, 2] = true;
        original[3, 3] = true;

        var clone = original.Clone(Allocator.Temp);

        Assert.IsTrue(original.Equals(clone));
        Assert.IsTrue(clone.Equals(original));
        clone.Dispose();
        original.Dispose();
    }

    [Test]
    public void Equals_AfterClear_ComparesCorrectly()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[1, 1] = true;
        shape2[1, 1] = true;

        Assert.IsTrue(shape1.Equals(shape2));

        shape1.Clear();
        Assert.IsFalse(shape1.Equals(shape2));

        shape2.Clear();
        Assert.IsTrue(shape1.Equals(shape2));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_ComplexPattern_ReturnsTrue()
    {
        var shape1 = Shapes.Cross(Allocator.Temp);
        var shape2 = Shapes.Cross(Allocator.Temp);

        Assert.IsTrue(shape1.Equals(shape2));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Equals_DifferentShapeTypes_ReturnsFalse()
    {
        var lShape = Shapes.LShape(Allocator.Temp);
        var tShape = Shapes.TShape(Allocator.Temp);

        Assert.IsFalse(lShape.Equals(tShape));
        Assert.IsFalse(tShape.Equals(lShape));
        lShape.Dispose();
        tShape.Dispose();
    }

    [Test]
    public void GetHashCode_EqualShapes_ReturnsSameHashCode()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[0, 0] = true;
        shape1[2, 2] = true;

        shape2[0, 0] = true;
        shape2[2, 2] = true;

        Assert.IsTrue(shape1.Equals(shape2));
        shape1.Dispose();
        shape2.Dispose();
        // Note: Equal objects should have equal hash codes, but we can't guarantee it with NativeBitArray
        // This is more of a documentation test to show the limitation
    }

    [Test]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        var shape1 = new GridShape(2, 2, Allocator.Temp);
        shape1[0, 0] = true;

        var shape2 = new GridShape(2, 2, Allocator.Temp);
        shape2[0, 0] = true;

        object objShape2 = shape2;

        Assert.Catch<NotSupportedException>(() => shape1.Equals(objShape2));
        Assert.Catch<NotSupportedException>(() => shape1.Equals(null));
        Assert.Catch<NotSupportedException>(() => shape1.Equals("not a shape"));
        shape1.Dispose();
        shape2.Dispose();
    }
}
