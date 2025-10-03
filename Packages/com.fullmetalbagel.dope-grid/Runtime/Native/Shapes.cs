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
        shape.SetCell(0, 0, true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Line(int length, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(length, 1, allocator);
        for (var i = 0; i < length; i++)
            shape.SetCell(i, 0, true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Square(int size, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell(x, y, true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape LShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(2, 2, allocator);
        shape.SetCell(0, 0, true);
        shape.SetCell(0, 1, true);
        shape.SetCell(1, 1, true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape TShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 2, allocator);
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 0, true);
        shape.SetCell(2, 0, true);
        shape.SetCell(1, 1, true);
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Cross(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 3, allocator);
        shape.SetCell(1, 0, true);
        shape.SetCell(0, 1, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(2, 1, true);
        shape.SetCell(1, 2, true);
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
        var rotated = new GridShape(dimensions.width, dimensions.height, allocator);
        shape.RotateBits(degree, rotated.Bits);
        return rotated;
    }

    [Pure, MustUseReturnValue]
    public static (int width, int height) GetRotatedDimensions(this in GridShape.ReadOnly shape, RotationDegree degree)
    {
        return degree switch
        {
            RotationDegree.None => (shape.Width, shape.Height),
            RotationDegree.Clockwise90 => (shape.Height, shape.Width),
            RotationDegree.Clockwise180 => (shape.Width, shape.Height),
            RotationDegree.Clockwise270 => (shape.Height, shape.Width),
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
                if (!shape[x, y]) continue;

                (int rotatedX, int rotatedY) = degree switch
                {
                    RotationDegree.Clockwise90 =>
                        // (x,y) -> (height-1-y, x)
                        (height - 1 - y, x),
                    RotationDegree.Clockwise180 =>
                        // (x,y) -> (width-1-x, height-1-y)
                        (width - 1 - x, height - 1 - y),
                    RotationDegree.Clockwise270 =>
                        // (x,y) -> (y, width-1-x)
                        (y, width - 1 - x),
                    _ => throw new ArgumentException($"Invalid rotation degree: {degree}")
                };

                var index = rotatedY * newBound.width + rotatedX;
                output.Set(index, true);
            }
        }
        return new GridShape.ReadOnly(newBound.width, newBound.height, output.AsReadOnly());
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

                var (flippedX, flippedY) = axis switch
                {
                    FlipAxis.Horizontal => (width - 1 - x, y),
                    FlipAxis.Vertical => (x, height - 1 - y),
                    _ => throw new ArgumentException($"Invalid flip axis: {axis}")
                };

                var destIndex = flippedY * width + flippedX;
                output.Set(destIndex, true);
            }
        }

        return new GridShape.ReadOnly(width, height, output.AsReadOnly());
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

    public static void FillShape<T>(this ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition pos, T value)
        where T : unmanaged, IEquatable<T>
    {
        // Check bounds to prevent out of range access
        var maxX = math.min(shape.Width, grid.Width - pos.X);
        var maxY = math.min(shape.Height, grid.Height - pos.Y);
        var startX = math.max(0, -pos.X);
        var startY = math.max(0, -pos.Y);

        // Fill only the cells where the shape has true values
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                if (shape[x, y])
                {
                    grid.SetValue(pos.X + x, pos.Y + y, value);
                }
            }
        }
    }
}
