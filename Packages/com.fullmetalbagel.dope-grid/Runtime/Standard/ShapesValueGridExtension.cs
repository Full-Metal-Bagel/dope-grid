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
}
