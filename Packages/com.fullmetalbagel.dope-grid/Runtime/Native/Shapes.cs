using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid.Native;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [Pure, MustUseReturnValue]
    public static GridShape Single(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(1, 1, allocator);
        shape.SetCell(new int2(0, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Line(int length, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(length, 1, allocator);
        for (var i = 0; i < length; i++)
            shape.SetCell(new int2(i, 0), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Square(int size, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell(new int2(x, y), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape LShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(2, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape TShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 2, allocator);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 0), true);
        shape.SetCell(new int2(1, 1), true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Cross(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 3, allocator);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);
        shape.SetCell(new int2(2, 1), true);
        shape.SetCell(new int2(1, 2), true);
        return shape;
    }

    // Immutable shape factory methods
    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSingle()
    {
        using var shape = Single(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLine(int length)
    {
        using var shape = Line(length, Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSquare(int size)
    {
        using var shape = Square(size, Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLShape()
    {
        using var shape = LShape(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableTShape()
    {
        using var shape = TShape(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableCross()
    {
        using var shape = Cross(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static GridShape Rotate(this GridShape shape, RotationDegree degree, Allocator allocator)
    {
        return shape.AsReadOnly().Rotate(degree, allocator);
    }

    [Pure, MustUseReturnValue]
    public static GridShape Rotate(this in GridShape.ReadOnly shape, RotationDegree degree, Allocator allocator)
    {
        var dimensions = shape.GetRotatedDimensions(degree);
        var rotated = new GridShape(dimensions.x, dimensions.y, allocator);
        shape.RotateBits(degree, rotated.Bits);
        return rotated;
    }

    [Pure, MustUseReturnValue]
    public static int2 GetRotatedDimensions(this in GridShape.ReadOnly shape, RotationDegree degree)
    {
        return degree switch
        {
            RotationDegree.Rotate90 => shape.Bound.yx,
            RotationDegree.Rotate180 => shape.Bound,
            RotationDegree.Rotate270 => shape.Bound.yx,
            _ => throw new NotImplementedException()
        };
    }

    public static GridShape.ReadOnly RotateBits(this in GridShape.ReadOnly shape, RotationDegree degree, SpanBitArray output)
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
        return new GridShape.ReadOnly(newBound, output.AsReadOnly());
    }

    [Pure, MustUseReturnValue]
    public static GridShape Flip(this GridShape shape, FlipAxis axis, Allocator allocator)
    {
        return shape.AsReadOnly().Flip(axis, allocator);
    }

    [Pure, MustUseReturnValue]
    public static GridShape Flip(this in GridShape.ReadOnly shape, FlipAxis axis, Allocator allocator)
    {
        var flipped = new GridShape(shape.Width, shape.Height, allocator);
        shape.FlipBits(axis, flipped.Bits);
        return flipped;
    }

    public static GridShape.ReadOnly FlipBits(this in GridShape.ReadOnly shape, FlipAxis axis, SpanBitArray output)
    {
        var width = shape.Width;
        var height = shape.Height;
        var bits = shape.Bits;

        // Use direct bit operations for better performance
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;

            for (var x = 0; x < width; x++)
            {
                var sourceIndex = rowStart + x;
                if (!bits.Get(sourceIndex)) continue;

                var flippedPos = axis switch
                {
                    FlipAxis.Horizontal => new int2(width - 1 - x, y),
                    FlipAxis.Vertical => new int2(x, height - 1 - y),
                    _ => throw new ArgumentException($"Invalid flip axis: {axis}")
                };

                var destIndex = flippedPos.y * width + flippedPos.x;
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
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1);     // Right column

        static bool HasOccupiedCellInRow(in GridShape.ReadOnly shape, int row)
        {
            var rowStart = row * shape.Width;
            return shape.Bits.TestAny(rowStart, shape.Width);
        }

        static bool HasOccupiedCellInColumn(in GridShape.ReadOnly shape, int column)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                if (shape.GetCell(column, row))
                    return true;
            }
            return false;
        }
    }

    [Pure, MustUseReturnValue]
    public static GridShape Trim(this in GridShape.ReadOnly shape, Allocator allocator = Allocator.Temp)
    {
        // Handle empty shapes
        if (shape.Width == 0 || shape.Height == 0)
            return new GridShape(0, 0, allocator);

        // If already trimmed, create a copy with the same cells
        if (shape.IsTrimmed())
            return shape.Clone(allocator);

        var bits = shape.Bits;
        var width = shape.Width;
        var height = shape.Height;

        // Find bounds using direct bit operations
        var minY = -1;
        var maxY = -1;

        // Find first and last occupied rows
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;
            if (bits.TestAny(rowStart, width))
            {
                if (minY == -1) minY = y;
                maxY = y;
            }
        }

        // No occupied cells found
        if (minY == -1)
            return new GridShape(0, 0, allocator);

        // Find left and right bounds
        var minX = width;
        var maxX = -1;

        for (var y = minY; y <= maxY; y++)
        {
            var rowStart = y * width;
            var remaining = width;
            var offset = 0;

            while (remaining > 0)
            {
                var chunkSize = math.min(remaining, 64);
                var chunk = bits.GetBits(rowStart + offset, chunkSize);

                if (chunk != 0)
                {
                    var trailingZeros = math.tzcnt(chunk);
                    if (trailingZeros < chunkSize)
                        minX = math.min(minX, offset + trailingZeros);

                    var leadingZeros = math.lzcnt(chunk);
                    var highestBit = 63 - leadingZeros;
                    maxX = math.max(maxX, offset + highestBit);

                    if (minX == 0 && maxX == width - 1)
                        break;
                }

                remaining -= chunkSize;
                offset += chunkSize;
            }

            if (minX == 0 && maxX == width - 1)
                break;
        }

        // Calculate new dimensions
        var newWidth = maxX - minX + 1;
        var newHeight = maxY - minY + 1;
        var trimmed = new GridShape(newWidth, newHeight, allocator);

        // Copy each occupied row segment into the output without per-cell writes
        var destBits = trimmed.Bits;

        for (var row = 0; row < newHeight; row++)
        {
            var srcIndex = (minY + row) * width + minX;
            var destIndex = row * newWidth;

            var remaining = newWidth;
            var offset = 0;

            while (remaining > 0)
            {
                var chunkSize = math.min(remaining, 64);
                var chunk = bits.GetBits(srcIndex + offset, chunkSize);

                if (chunk != 0)
                {
                    destBits.SetBits(destIndex + offset, chunk, chunkSize);
                }

                remaining -= chunkSize;
                offset += chunkSize;
            }
        }

        return trimmed;
    }
}
