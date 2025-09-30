using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DopeGrid.Standard;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [Pure, MustUseReturnValue]
    public static GridShape Single()
    {
        var shape = new GridShape(1, 1);
        shape.SetCell((0, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Line(int length)
    {
        var shape = new GridShape(length, 1);
        for (var i = 0; i < length; i++)
            shape.SetCell((i, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Square(int size)
    {
        var shape = new GridShape(size, size);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell((x, y), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape LShape()
    {
        var shape = new GridShape(2, 2);
        shape.SetCell((0, 0), true);
        shape.SetCell((0, 1), true);
        shape.SetCell((1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape TShape()
    {
        var shape = new GridShape(3, 2);
        shape.SetCell((0, 0), true);
        shape.SetCell((1, 0), true);
        shape.SetCell((2, 0), true);
        shape.SetCell((1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Cross()
    {
        var shape = new GridShape(3, 3);
        shape.SetCell((1, 0), true);
        shape.SetCell((0, 1), true);
        shape.SetCell((1, 1), true);
        shape.SetCell((2, 1), true);
        shape.SetCell((1, 2), true);
        return shape;
    }

    // Immutable shape factory methods
    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSingle()
    {
        using var shape = Single();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLine(int length)
    {
        using var shape = Line(length);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSquare(int size)
    {
        using var shape = Square(size);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLShape()
    {
        using var shape = LShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableTShape()
    {
        using var shape = TShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableCross()
    {
        using var shape = Cross();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static GridShape Rotate(this GridShape shape, RotationDegree degree)
    {
        return shape.AsReadOnly().Rotate(degree);
    }

    [Pure, MustUseReturnValue]
    public static GridShape Rotate(this in GridShape.ReadOnly shape, RotationDegree degree)
    {
        var dimensions = shape.GetRotatedDimensions(degree);
        var rotated = new GridShape(dimensions.width, dimensions.height);
        shape.RotateBits(degree, rotated.Bits);
        return rotated;
    }

    [Pure, MustUseReturnValue]
    public static (int width, int height) GetRotatedDimensions(this in GridShape.ReadOnly shape, RotationDegree degree)
    {
        return degree switch
        {
            RotationDegree.Clockwise90 => (shape.Height, shape.Width),
            RotationDegree.Clockwise180 => (shape.Width, shape.Height),
            RotationDegree.Clockwise270 => (shape.Height, shape.Width),
            _ => (shape.Width, shape.Height)
        };
    }

    public static GridShape.ReadOnly RotateBits(this in GridShape.ReadOnly shape, RotationDegree degree, SpanBitArray output)
    {
        var width = shape.Width;
        var height = shape.Height;
        var newBound = GetRotatedDimensions(shape, degree);

        if (degree == RotationDegree.None)
        {
            shape.Bits.CopyTo(output);
            return shape;
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!shape.GetCell((x, y))) continue;

                (int x, int y) rotatedPos = degree switch
                {
                    RotationDegree.Clockwise90 => (height - 1 - y, x),
                    RotationDegree.Clockwise180 => (width - 1 - x, height - 1 - y),
                    RotationDegree.Clockwise270 => (y, width - 1 - x),
                    _ => throw new ArgumentException($"Invalid rotation degree: {degree}")
                };

                var index = rotatedPos.y * newBound.width + rotatedPos.x;
                output.Set(index, true);
            }
        }
        return new GridShape.ReadOnly(newBound, output.AsReadOnly());
    }

    [Pure, MustUseReturnValue]
    public static GridShape Flip(this GridShape shape, FlipAxis axis)
    {
        return shape.AsReadOnly().Flip(axis);
    }

    [Pure, MustUseReturnValue]
    public static GridShape Flip(this in GridShape.ReadOnly shape, FlipAxis axis)
    {
        var flipped = new GridShape(shape.Width, shape.Height);
        shape.FlipBits(axis, flipped.Bits);
        return flipped;
    }

    public static GridShape.ReadOnly FlipBits(this in GridShape.ReadOnly shape, FlipAxis axis, SpanBitArray output)
    {
        var width = shape.Width;
        var height = shape.Height;
        var bits = shape.Bits;

        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;

            for (var x = 0; x < width; x++)
            {
                var sourceIndex = rowStart + x;
                if (!bits.Get(sourceIndex)) continue;

                var flippedPos = axis switch
                {
                    FlipAxis.Horizontal => (width - 1 - x, y),
                    FlipAxis.Vertical => (x, height - 1 - y),
                    _ => throw new ArgumentException($"Invalid flip axis: {axis}")
                };

                var destIndex = flippedPos.Item2 * width + flippedPos.Item1;
                output.Set(destIndex, true);
            }
        }

        return new GridShape.ReadOnly(width, height, output.AsReadOnly());
    }

    [Pure, MustUseReturnValue]
    public static bool IsTrimmed(this GridShape shape)
    {
        return shape.AsReadOnly().IsTrimmed();
    }

    [Pure, MustUseReturnValue]
    public static bool IsTrimmed(this in GridShape.ReadOnly shape)
    {
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        return HasOccupiedCellInRow(shape, 0) &&
               HasOccupiedCellInRow(shape, shape.Height - 1) &&
               HasOccupiedCellInColumn(shape, 0) &&
               HasOccupiedCellInColumn(shape, shape.Width - 1);

        static bool HasOccupiedCellInRow(in GridShape.ReadOnly shape, int row)
        {
            var rowStart = row * shape.Width;
            return shape.Bits.TestAny(rowStart, shape.Width);
        }

        static bool HasOccupiedCellInColumn(in GridShape.ReadOnly shape, int column)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                if (shape.GetCell((column, row)))
                    return true;
            }
            return false;
        }
    }

    [Pure, MustUseReturnValue]
    public static GridShape Trim(this in GridShape.ReadOnly shape)
    {
        if (shape.Width == 0 || shape.Height == 0)
            return new GridShape(0, 0);

        if (shape.IsTrimmed())
            return shape.Clone();

        var bits = shape.Bits;
        var width = shape.Width;
        var height = shape.Height;

        var minY = -1;
        var maxY = -1;

        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;
            if (bits.TestAny(rowStart, width))
            {
                if (minY == -1) minY = y;
                maxY = y;
            }
        }

        if (minY == -1)
            return new GridShape(0, 0);

        var minX = width;
        var maxX = -1;

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (shape.GetCell((x, y)))
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
        }

        var newWidth = maxX - minX + 1;
        var newHeight = maxY - minY + 1;
        var trimmed = new GridShape(newWidth, newHeight);

        for (var row = 0; row < newHeight; row++)
        {
            for (var col = 0; col < newWidth; col++)
            {
                if (shape.GetCell((minX + col, minY + row)))
                {
                    trimmed.SetCell((col, row), true);
                }
            }
        }

        return trimmed;
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape GetRotatedShape(this ImmutableGridShape shape, RotationDegree rotation)
    {
        return rotation switch
        {
            RotationDegree.None => shape,
            RotationDegree.Clockwise90 => shape.Rotate90(),
            RotationDegree.Clockwise180 => shape.Rotate90().Rotate90(),
            RotationDegree.Clockwise270 => shape.Rotate90().Rotate90().Rotate90(),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }
}
