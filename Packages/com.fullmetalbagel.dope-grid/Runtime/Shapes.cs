using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

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

    public static GridShape2D Rotate(this GridShape2D shape, RotationDegree degree, Allocator allocator)
    {
        var width = shape.Width;
        var height = shape.Height;
        
        // Always rotate around center for consistent behavior
        var pivot = new int2(width / 2, height / 2);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        using var rotatedPositions = new NativeList<int2>(width * height, Allocator.Temp);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!shape.GetCell(new int2(x, y))) continue;

                var relativePos = new int2(x - pivot.x, y - pivot.y);
                int2 rotatedPos;

                switch (degree)
                {
                    case RotationDegree.Rotate90:
                        rotatedPos = new int2(-relativePos.y, relativePos.x);
                        break;
                    case RotationDegree.Rotate180:
                        rotatedPos = new int2(-relativePos.x, -relativePos.y);
                        break;
                    case RotationDegree.Rotate270:
                        rotatedPos = new int2(relativePos.y, -relativePos.x);
                        break;
                    default:
                        rotatedPos = relativePos;
                        break;
                }

                var finalPos = rotatedPos + pivot;
                rotatedPositions.Add(finalPos);

                minX = finalPos.x < minX ? finalPos.x : minX;
                minY = finalPos.y < minY ? finalPos.y : minY;
                maxX = finalPos.x > maxX ? finalPos.x : maxX;
                maxY = finalPos.y > maxY ? finalPos.y : maxY;
            }
        }

        var newWidth = maxX - minX + 1;
        var newHeight = maxY - minY + 1;
        var rotatedShape = new GridShape2D(newWidth, newHeight, allocator);

        for (var i = 0; i < rotatedPositions.Length; i++)
        {
            var pos = rotatedPositions[i];
            var translatedPos = new int2(pos.x - minX, pos.y - minY);
            rotatedShape.SetCell(translatedPos, true);
        }

        return rotatedShape;
    }
}
