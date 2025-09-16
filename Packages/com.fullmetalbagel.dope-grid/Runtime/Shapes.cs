using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace DopeGrid;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [Pure, MustUseReturnValue]
    public static GridShape2D Single(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(1, 1, allocator);
        shape.SetCell(new int2(0, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape2D Line(int length, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(length, 1, allocator);
        for (var i = 0; i < length; i++)
            shape.SetCell(new int2(i, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape2D Square(int size, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell(new int2(x, y), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape2D LShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(2, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape2D TShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape2D(3, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 0), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
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

    [Pure, MustUseReturnValue]
    public static GridShape2D Rotate(this GridShape2D shape, RotationDegree degree, Allocator allocator)
    {
        return shape.ToReadOnly().Rotate(degree, allocator);
    }

    [Pure, MustUseReturnValue]
    public static GridShape2D Rotate(this GridShape2D.ReadOnly shape, RotationDegree degree, Allocator allocator)
    {
        var dimensions = shape.GetRotatedDimensions(degree);
        var rotated = new GridShape2D(dimensions.x, dimensions.y, allocator);
        shape.RotateBits(degree, rotated.WritableBits);
        return rotated;
    }

    [Pure, MustUseReturnValue]
    public static int2 GetRotatedDimensions(this GridShape2D.ReadOnly shape, RotationDegree degree)
    {
        return degree switch
        {
            RotationDegree.Rotate90 => shape.Bound.yx,
            RotationDegree.Rotate180 => shape.Bound,
            RotationDegree.Rotate270 => shape.Bound.yx,
            _ => throw new NotImplementedException()
        };
    }

    public static GridShape2D.ReadOnly RotateBits(this GridShape2D.ReadOnly shape, RotationDegree degree, UnsafeBitArray output)
    {
        var width = shape.Width;
        var height = shape.Height;
        var newBound = GetRotatedDimensions(shape, degree);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!shape.GetCell(new int2(x, y))) continue;

                int2 rotatedPos = degree switch
                {
                    RotationDegree.Rotate90 =>
                        // (x,y) -> (height-1-y, x)
                        new int2(height - 1 - y, x),
                    RotationDegree.Rotate180 =>
                        // (x,y) -> (width-1-x, height-1-y)
                        new int2(width - 1 - x, height - 1 - y),
                    RotationDegree.Rotate270 =>
                        // (x,y) -> (y, width-1-x)
                        new int2(y, width - 1 - x),
                    _ => throw new ArgumentException($"Invalid rotation degree: {degree}")
                };

                var index = rotatedPos.y * newBound.x + rotatedPos.x;
                output.Set(index, true);
            }
        }
        return new GridShape2D.ReadOnly(newBound, output.AsReadOnly());
    }
}
