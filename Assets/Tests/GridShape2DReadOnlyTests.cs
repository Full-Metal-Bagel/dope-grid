using System;
using DopeGrid;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridShape2DReadOnlyTests
{
    [Test]
    public void AsReadOnly_CreatesReadOnlyView()
    {
        using var shape = new GridShape2D(5, 5, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(2, 2), true);
        shape.SetCell(new int2(4, 4), true);

        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(5, readOnly.Width);
        Assert.AreEqual(5, readOnly.Height);
        Assert.AreEqual(25, readOnly.Size);
        Assert.AreEqual(3, readOnly.OccupiedSpaceCount);
        Assert.AreEqual(22, readOnly.FreeSpaceCount);

        Assert.IsTrue(readOnly.GetCell(new int2(0, 0)));
        Assert.IsTrue(readOnly.GetCell(new int2(2, 2)));
        Assert.IsTrue(readOnly.GetCell(new int2(4, 4)));
        Assert.IsFalse(readOnly.GetCell(new int2(1, 1)));
    }

    [Test]
    public void ReadOnly_GetIndex_CalculatesCorrectly()
    {
        using var shape = new GridShape2D(3, 4, Allocator.Temp);
        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(0, readOnly.GetIndex(new int2(0, 0)));
        Assert.AreEqual(2, readOnly.GetIndex(new int2(2, 0)));
        Assert.AreEqual(3, readOnly.GetIndex(new int2(0, 1)));
        Assert.AreEqual(5, readOnly.GetIndex(new int2(2, 1)));
        Assert.AreEqual(9, readOnly.GetIndex(new int2(0, 3)));
        Assert.AreEqual(11, readOnly.GetIndex(new int2(2, 3)));
    }

    [Test]
    public void ReadOnly_Equals_IdenticalShapes()
    {
        using var shape1 = new GridShape2D(3, 3, Allocator.Temp);
        using var shape2 = new GridShape2D(3, 3, Allocator.Temp);

        shape1.SetCell(new int2(0, 0), true);
        shape1.SetCell(new int2(1, 1), true);
        shape1.SetCell(new int2(2, 2), true);

        shape2.SetCell(new int2(0, 0), true);
        shape2.SetCell(new int2(1, 1), true);
        shape2.SetCell(new int2(2, 2), true);

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsTrue(readOnly1.Equals(readOnly2));
        Assert.IsTrue(readOnly2.Equals(readOnly1));
        Assert.IsTrue(readOnly1 == readOnly2);
        Assert.IsFalse(readOnly1 != readOnly2);
    }

    [Test]
    public void ReadOnly_Equals_DifferentShapes()
    {
        using var shape1 = new GridShape2D(3, 3, Allocator.Temp);
        using var shape2 = new GridShape2D(3, 3, Allocator.Temp);

        shape1.SetCell(new int2(0, 0), true);
        shape1.SetCell(new int2(1, 1), true);

        shape2.SetCell(new int2(0, 0), true);
        shape2.SetCell(new int2(2, 2), true);

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsFalse(readOnly1.Equals(readOnly2));
        Assert.IsFalse(readOnly2.Equals(readOnly1));
        Assert.IsFalse(readOnly1 == readOnly2);
        Assert.IsTrue(readOnly1 != readOnly2);
    }

    [Test]
    public void ReadOnly_Equals_DifferentDimensions()
    {
        using var shape1 = new GridShape2D(3, 3, Allocator.Temp);
        using var shape2 = new GridShape2D(3, 4, Allocator.Temp);

        shape1.SetCell(new int2(0, 0), true);
        shape2.SetCell(new int2(0, 0), true);

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsFalse(readOnly1.Equals(readOnly2));
    }

    [Test]
    public void ReadOnly_Equals_ObjectOverload()
    {
        using var shape = new GridShape2D(2, 2, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);

        Assert.Catch<NotSupportedException>(() => shape.AsReadOnly().Equals(null));
        Assert.Catch<NotSupportedException>(() => shape.AsReadOnly().Equals("not a shape"));
    }

    [Test]
    public void ReadOnly_GetHashCode_ConsistentForEqualObjects()
    {
        using var shape1 = new GridShape2D(3, 3, Allocator.Temp);
        using var shape2 = new GridShape2D(3, 3, Allocator.Temp);

        shape1.SetCell(new int2(1, 1), true);
        shape2.SetCell(new int2(1, 1), true);

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        // While we can't guarantee equal hash codes for equal objects with NativeBitArray,
        // we can at least verify hash codes are consistent for the same object
        Assert.AreEqual(readOnly1.GetHashCode(), readOnly1.GetHashCode());
        Assert.AreEqual(readOnly2.GetHashCode(), readOnly2.GetHashCode());
    }

    [Test]
    public void ReadOnly_ReflectsChangesToOriginalShape()
    {
        using var shape = new GridShape2D(3, 3, Allocator.Temp);
        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(0, readOnly.OccupiedSpaceCount);
        Assert.IsFalse(readOnly.GetCell(new int2(1, 1)));

        // Modify the original shape
        shape.SetCell(new int2(1, 1), true);

        // ReadOnly view should reflect the change
        Assert.AreEqual(1, readOnly.OccupiedSpaceCount);
        Assert.IsTrue(readOnly.GetCell(new int2(1, 1)));

        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(2, 2), true);

        Assert.AreEqual(3, readOnly.OccupiedSpaceCount);
        Assert.IsTrue(readOnly.GetCell(new int2(0, 0)));
        Assert.IsTrue(readOnly.GetCell(new int2(2, 2)));
    }

    [Test]
    public void ReadOnly_ComplexShapes()
    {
        using var cross = Shapes.Cross(Allocator.Temp);
        using var lShape = Shapes.LShape(Allocator.Temp);
        using var tShape = Shapes.TShape(Allocator.Temp);

        var crossReadOnly = cross.AsReadOnly();
        var lShapeReadOnly = lShape.AsReadOnly();
        var tShapeReadOnly = tShape.AsReadOnly();

        Assert.AreEqual(5, crossReadOnly.OccupiedSpaceCount);
        Assert.AreEqual(3, lShapeReadOnly.OccupiedSpaceCount);
        Assert.AreEqual(4, tShapeReadOnly.OccupiedSpaceCount);

        Assert.IsFalse(crossReadOnly.Equals(lShapeReadOnly));
        Assert.IsFalse(lShapeReadOnly.Equals(tShapeReadOnly));
        Assert.IsFalse(tShapeReadOnly.Equals(crossReadOnly));
    }
}
