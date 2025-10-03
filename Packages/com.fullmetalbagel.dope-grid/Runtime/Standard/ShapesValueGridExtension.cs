using System;

namespace DopeGrid.Standard;

public static class ShapesValueGridExtension
{
    public static void FillShape<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition pos, T value)
        where T : IEquatable<T>
    {
        // Check bounds to prevent out of range access
        var maxX = Math.Min(shape.Width, grid.Width - pos.X);
        var maxY = Math.Min(shape.Height, grid.Height - pos.Y);
        var startX = Math.Max(0, -pos.X);
        var startY = Math.Max(0, -pos.Y);

        // Fill only the cells where the shape has true values
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                if (shape.GetCellValue(new GridPosition(x, y)))
                {
                    var gridPos = new GridPosition(pos.X + x, pos.Y + y);
                    grid.SetValue(gridPos, value);
                }
            }
        }
    }
}
