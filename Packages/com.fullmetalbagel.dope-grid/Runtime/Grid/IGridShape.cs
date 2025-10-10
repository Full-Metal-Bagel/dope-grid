using System;

namespace DopeGrid;

public interface IGridShape<T> : IReadOnlyGridShape
{
    T this[int x, int y] { get; set; }
}

public static class GridShapeExtension
{
    public static void SetCellValue<TGrid, TValue>(this TGrid grid, int x, int y, TValue value)
        where TGrid : IGridShape<TValue>
        => grid[x, y] = value;

    public static void WriteTo<TSourceGrid, TTargetGrid, TValue>(this TSourceGrid source, TTargetGrid target)
        where TSourceGrid : IReadOnlyGridShape<TValue>
        where TTargetGrid : IGridShape<TValue>
    {
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            target[x, y] = source[x, y];
        }
    }

    public static void WriteTo<TSourceGrid, TTargetGrid, TValue>(this TSourceGrid source, int width, int height, TTargetGrid target, TValue _ = default!)
        where TSourceGrid : IReadOnlyGridShape<TValue>
        where TTargetGrid : IGridShape<TValue>
    {
        source.WriteTo(0, 0, width, height, target, _);
    }

    public static void WriteTo<TSourceGrid, TTargetGrid, TValue>(this TSourceGrid source, int offsetX, int offsetY, int width, int height, TTargetGrid target, TValue _ = default!)
        where TSourceGrid : IReadOnlyGridShape<TValue>
        where TTargetGrid : IGridShape<TValue>
    {
        if (offsetX < 0 || offsetY < 0 || width < 0 || height < 0)
            throw new ArgumentException("offset/size must be non-negative");

        if (offsetX + width > source.Width || offsetY + height > source.Height)
            throw new ArgumentException("Source sub-rectangle is out of bounds");

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            target[x, y] = source[offsetX + x, offsetY + y];
        }
    }

    public static void FillAll<TGrid, TValue>(this TGrid grid, TValue value)
        where TGrid : IGridShape<TValue>
    {
        for (int y = 0; y < grid.Height; y++)
        for (int x = 0; x < grid.Width; x++)
        {
            grid[x, y] = value;
        }
    }

    public static void FillRect<TGrid, TValue>(this TGrid grid, int x, int y, int width, int height, TValue value)
        where TGrid : IGridShape<TValue>
    {
        for (int dy = 0; dy < height; dy++)
        for (int dx = 0; dx < width; dx++)
        {
            var px = x + dx;
            var py = y + dy;
            if (px >= 0 && px < grid.Width && py >= 0 && py < grid.Height)
            {
                grid[px, py] = value;
            }
        }
    }

    public static void FillShape<TGrid, TShape, TValue>(this TGrid grid, TShape shape, int offsetX, int offsetY, TValue _ = default!)
        where TGrid : IGridShape<TValue>
        where TShape : IReadOnlyGridShape<TValue>
    {
        var maxX = Math.Min(shape.Width, grid.Width - offsetX);
        var maxY = Math.Min(shape.Height, grid.Height - offsetY);
        var startX = Math.Max(0, -offsetX);
        var startY = Math.Max(0, -offsetY);

        for (int y = startY; y < maxY; y++)
        for (int x = startX; x < maxX; x++)
        {
            if (shape.IsOccupied(x, y))
            {
                grid[offsetX + x, offsetY + y] = shape[x, y];
            }
        }
    }

    public static void FillShapeWithValue<TGrid, TShape, TValue>(this TGrid grid, TShape shape, int offsetX, int offsetY, TValue value)
        where TGrid : IGridShape<TValue>
        where TShape : IReadOnlyGridShape
    {
        var maxX = Math.Min(shape.Width, grid.Width - offsetX);
        var maxY = Math.Min(shape.Height, grid.Height - offsetY);
        var startX = Math.Max(0, -offsetX);
        var startY = Math.Max(0, -offsetY);

        for (int y = startY; y < maxY; y++)
        for (int x = startX; x < maxX; x++)
        {
            if (shape.IsOccupied(x, y))
            {
                grid[offsetX + x, offsetY + y] = value;
            }
        }
    }

    public static void RotateShape<TInputShape, TOutputShape, TValue>(this TInputShape input, RotationDegree degree, TOutputShape output, TValue _ = default!)
        where TInputShape : IReadOnlyGridShape<TValue>
        where TOutputShape : IGridShape<TValue>
    {
        var width = input.Width;
        var height = input.Height;
        var (newWidth, newHeight) = degree.CalculateRotatedSize(width, height);
        if (output.Width != newWidth || output.Height != newHeight)
            throw new ArgumentException("output shape is not fit");

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
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

            output[rotatedX, rotatedY] = input[x, y];
        }
    }

    public static void FlipShape<TInputShape, TOutputShape, TValue>(this TInputShape input, FlipAxis axis, TOutputShape output, TValue _ = default!)
        where TInputShape : IReadOnlyGridShape<TValue>
        where TOutputShape : IGridShape<TValue>
    {
        if (!input.HasSameSize(output))
            throw new ArgumentException("output shape is not fit");

        var width = input.Width;
        var height = input.Height;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var (flippedX, flippedY) = axis switch
            {
                FlipAxis.Horizontal => (width - 1 - x, y),
                FlipAxis.Vertical => (x, height - 1 - y),
                _ => throw new ArgumentException($"Invalid flip axis: {axis}")
            };

            output[flippedX, flippedY] = input[x, y];
        }
    }

    public static void Trim<TInputShape, TOutputShape, TValue>(this TInputShape input, TOutputShape output, TValue _ = default!)
        where TInputShape : IReadOnlyGridShape<TValue>
        where TOutputShape : IGridShape<TValue>
        where TValue : IEquatable<TValue>
    {
        var (x, y, width, height) = input.GetTrimmedBound(_);
        if (output.Width != width || output.Height != height)
            throw new ArgumentException("output shape is not fit");
        input.WriteTo<TInputShape, TOutputShape, TValue>(x, y, width, height, output);
    }
}

public static class BoolGridShapeExtension
{
    public static void WriteTo<TSourceGrid, TTargetGrid>(this TSourceGrid source, TTargetGrid target)
        where TSourceGrid : IReadOnlyGridShape<bool>
        where TTargetGrid : IGridShape<bool>
        => source.WriteTo<TSourceGrid, TTargetGrid, bool>(target);

    public static void SetCellValue<TGrid>(this TGrid grid, int x, int y, bool value)
        where TGrid : IGridShape<bool>
        => grid.SetCellValue<TGrid, bool>(x, y, value);

    public static void FillAll<TGrid>(this TGrid grid, bool value)
        where TGrid : IGridShape<bool>
        => grid.FillAll<TGrid, bool>(value);

    public static void FillRect<TGrid>(this TGrid grid, int x, int y, int width, int height, bool value)
        where TGrid : IGridShape<bool>
        => grid.FillRect<TGrid, bool>(x, y, width, height, value);

    public static void FillShapeWithValue<TGrid, TShape>(this TGrid grid, TShape shape, int x, int y, bool value)
        where TGrid : IGridShape<bool>
        where TShape : IReadOnlyGridShape<bool>
        => grid.FillShapeWithValue<TGrid, TShape, bool>(shape, x, y, value);
}
