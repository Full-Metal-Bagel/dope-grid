using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeInventory;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    public static GridShape2D Single(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(1, 1, allocator);
        shape.SetCell(new int2(0, 0), true);
        return shape;
    }

    public static GridShape2D Line(int length, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(length, 1, allocator);
        for (var i = 0; i < length; i++)
            shape.SetCell(new int2(i, 0), true);
        return shape;
    }

    public static GridShape2D Square(int size, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell(new int2(x, y), true);
        return shape;
    }

    public static GridShape2D LShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(2, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    public static GridShape2D TShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(3, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 0), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    public static GridShape2D Cross(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(3, 3, allocator);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);
        shape.SetCell(new int2(2, 1), true);
        shape.SetCell(new int2(1, 2), true);
        return shape;
    }
}
