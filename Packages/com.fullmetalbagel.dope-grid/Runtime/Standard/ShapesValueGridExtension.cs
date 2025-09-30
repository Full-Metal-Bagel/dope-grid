using System;

namespace DopeGrid.Standard;

public static class ShapesValueGridExtension
{
    public static void FillShape<T>(this ValueGridShape<T> grid, ImmutableGridShape shape, (int x, int y) pos, T value)
        where T : IEquatable<T>
    {
        // Check bounds to prevent out of range access
        var maxX = Math.Min(shape.Width, grid.Width - pos.x);
        var maxY = Math.Min(shape.Height, grid.Height - pos.y);
        var startX = Math.Max(0, -pos.x);
        var startY = Math.Max(0, -pos.y);

        // Fill only the cells where the shape has true values
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                if (shape.GetCell((x, y)))
                {
                    var gridPos = (pos.x + x, pos.y + y);
                    grid.SetValue(gridPos, value);
                }
            }
        }
    }

    public static bool IsWithinBounds<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, (int x, int y) position)
        where T : IEquatable<T>
    {
        return position.x >= 0 && position.y >= 0 &&
               position.x + shape.Width <= grid.Width &&
               position.y + shape.Height <= grid.Height;
    }

    public static bool CheckShapeCells<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, (int x, int y) position, Func<(int x, int y), T, bool> cellPredicate)
        where T : IEquatable<T>
    {
        return grid.CheckShapeCells(shape, position, (pos, value, predicate) => predicate(pos, value), cellPredicate);
    }

    public static bool CheckShapeCells<T, TData>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, (int x, int y) position, Func<(int x, int y), T, TData, bool> cellPredicate, TData data)
        where T : IEquatable<T>
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape.GetCell((x, y)))
                {
                    var gridPos = (position.x + x, position.y + y);
                    var cellValue = grid[gridPos];
                    if (!cellPredicate(gridPos, cellValue, data))
                        return false;
                }
            }
        }
        return true;
    }
}