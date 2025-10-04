using DopeGrid;
using System;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridShapeReadOnlyTests
{
    [Test]
    public void AsReadOnly_CreatesReadOnlyView()
    {
        var shape = new GridShape(5, 5, Allocator.Temp);
        shape[0, 0] = true;
        shape[2, 2] = true;
        shape[4, 4] = true;

        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(5, readOnly.Width);
        Assert.AreEqual(5, readOnly.Height);
        Assert.AreEqual(25, readOnly.Size());
        Assert.AreEqual(3, readOnly.OccupiedSpaceCount);
        Assert.AreEqual(22, readOnly.FreeSpaceCount);

        Assert.IsTrue(readOnly.GetCellValue(0, 0));
        Assert.IsTrue(readOnly.GetCellValue(2, 2));
        Assert.IsTrue(readOnly.GetCellValue(4, 4));
        Assert.IsFalse(readOnly.GetCellValue(1, 1));
        shape.Dispose();
    }

    [Test]
    public void ReadOnly_GetIndex_CalculatesCorrectly()
    {
        var shape = new GridShape(3, 4, Allocator.Temp);
        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(0, readOnly.GetIndex(0, 0));
        Assert.AreEqual(2, readOnly.GetIndex(2, 0));
        Assert.AreEqual(3, readOnly.GetIndex(0, 1));
        Assert.AreEqual(5, readOnly.GetIndex(2, 1));
        Assert.AreEqual(9, readOnly.GetIndex(0, 3));
        Assert.AreEqual(11, readOnly.GetIndex(2, 3));
        shape.Dispose();
    }

    [Test]
    public void ReadOnly_Equals_IdenticalShapes()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[0, 0] = true;
        shape1[1, 1] = true;
        shape1[2, 2] = true;

        shape2[0, 0] = true;
        shape2[1, 1] = true;
        shape2[2, 2] = true;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsTrue(readOnly1.Equals(readOnly2));
        Assert.IsTrue(readOnly2.Equals(readOnly1));
        Assert.IsTrue(readOnly1 == readOnly2);
        Assert.IsFalse(readOnly1 != readOnly2);
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void ReadOnly_Equals_DifferentShapes()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 3, Allocator.Temp);

        shape1[0, 0] = true;
        shape1[1, 1] = true;

        shape2[0, 0] = true;
        shape2[2, 2] = true;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsFalse(readOnly1.Equals(readOnly2));
        Assert.IsFalse(readOnly2.Equals(readOnly1));
        Assert.IsFalse(readOnly1 == readOnly2);
        Assert.IsTrue(readOnly1 != readOnly2);
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void ReadOnly_Equals_DifferentDimensions()
    {
        var shape1 = new GridShape(3, 3, Allocator.Temp);
        var shape2 = new GridShape(3, 4, Allocator.Temp);

        shape1[0, 0] = true;
        shape2[0, 0] = true;

        var readOnly1 = shape1.AsReadOnly();
        var readOnly2 = shape2.AsReadOnly();

        Assert.IsFalse(readOnly1.Equals(readOnly2));
        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void ReadOnly_Equals_ObjectOverload()
    {
        var shape = new GridShape(2, 2, Allocator.Temp);
        shape[0, 0] = true;

        Assert.Catch<NotSupportedException>(() => shape.AsReadOnly().Equals(null));
        Assert.Catch<NotSupportedException>(() => shape.AsReadOnly().Equals("not a shape"));
        shape.Dispose();
    }

    [Test]
    public void ReadOnly_ReflectsChangesToOriginalShape()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        var readOnly = shape.AsReadOnly();

        Assert.AreEqual(0, readOnly.OccupiedSpaceCount);
        Assert.IsFalse(readOnly.GetCellValue(1, 1));

        // Modify the original shape
        shape[1, 1] = true;

        // ReadOnly view should reflect the change
        Assert.AreEqual(1, readOnly.OccupiedSpaceCount);
        Assert.IsTrue(readOnly.GetCellValue(1, 1));

        shape[0, 0] = true;
        shape[2, 2] = true;

        Assert.AreEqual(3, readOnly.OccupiedSpaceCount);
        Assert.IsTrue(readOnly.GetCellValue(0, 0));
        Assert.IsTrue(readOnly.GetCellValue(2, 2));
        shape.Dispose();
    }

    [Test]
    public void ReadOnly_ComplexShapes()
    {
        var cross = Shapes.Cross(Allocator.Temp);
        var lShape = Shapes.LShape(Allocator.Temp);
        var tShape = Shapes.TShape(Allocator.Temp);

        var crossReadOnly = cross.AsReadOnly();
        var lShapeReadOnly = lShape.AsReadOnly();
        var tShapeReadOnly = tShape.AsReadOnly();

        Assert.AreEqual(5, crossReadOnly.OccupiedSpaceCount);
        Assert.AreEqual(3, lShapeReadOnly.OccupiedSpaceCount);
        Assert.AreEqual(4, tShapeReadOnly.OccupiedSpaceCount);

        Assert.IsFalse(crossReadOnly.Equals(lShapeReadOnly));
        Assert.IsFalse(lShapeReadOnly.Equals(tShapeReadOnly));
        Assert.IsFalse(tShapeReadOnly.Equals(crossReadOnly));
        cross.Dispose();
        lShape.Dispose();
        tShape.Dispose();
    }
}
