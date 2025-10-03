

namespace DopeGrid.Standard;

internal static class math
{
    public static int min(int a, int b) => System.Math.Min(a, b);
    public static int max(int a, int b) => System.Math.Max(a, b);
    public static int tzcnt(ulong value)
    {
        if (value == 0) return 64;
        int count = 0;
        while ((value & 1) == 0)
        {
            value >>= 1;
            count++;
        }
        return count;
    }
    public static int lzcnt(ulong value)
    {
        if (value == 0) return 64;
        int count = 0;
        while ((value & (1UL << 63)) == 0)
        {
            value <<= 1;
            count++;
        }
        return count;
    }
}

public static partial class ReadOnlyGridShapeExtension
{
    
#region ref GridShape

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this ref GridShape shape, GridPosition pos) => GetIndex(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this ref GridShape shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this ref GridShape shape, GridPosition pos) => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this ref GridShape shape, int x, int y) => GetCellValue(ref shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this ref GridShape shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this ref GridShape shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this ref GridShape shape, GridPosition pos) => Contains(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this ref GridShape shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this ref GridShape shape, bool target)
    {
        return Count(ref shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this ref GridShape shape, System.Func<bool, bool> predicate)
    {
        return Count(ref shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this ref GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this ref GridShape shape, System.Func<bool, bool> predicate)
    {
        return Any(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this ref GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this ref GridShape shape, System.Func<bool, bool> predicate)
    {
        return All(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this ref GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this ref GridShape shape, bool freeValue)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(ref shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(ref shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(ref shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(ref shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(ref GridShape shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = GetCellValue(ref shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(ref GridShape shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = GetCellValue(ref shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this ref GridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (GetCellValue(ref container, gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this ref GridShape container, ImmutableGridShape item, bool freeValue)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(ref container, rotatedItem, freeValue);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells(this ref GridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, bool> cellPredicate)
    {
        return CheckShapeCells(ref grid, shape, position, static (pos, value, pred) => pred(pos, value), cellPredicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this ref GridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!Contains(ref grid, gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this ref GridShape grid, ImmutableGridShape item, bool freeValue)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(ref grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this ref GridShape grid, ImmutableGridShape shape, GridPosition position)
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this ref GridShape grid, ImmutableGridShape shape, int x, int y)
    {
        return IsWithinBounds(ref grid, shape, new GridPosition(x, y));
    }

#endregion
    
#region in GridShape.ReadOnly

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape.ReadOnly shape, GridPosition pos) => GetIndex(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape.ReadOnly shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape.ReadOnly shape, GridPosition pos) => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape.ReadOnly shape, int x, int y) => GetCellValue(in shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this in GridShape.ReadOnly shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this in GridShape.ReadOnly shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape.ReadOnly shape, GridPosition pos) => Contains(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape.ReadOnly shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this in GridShape.ReadOnly shape, bool target)
    {
        return Count(in shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return Count(in shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return Any(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return All(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this in GridShape.ReadOnly shape, bool freeValue)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(in shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(in shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(in shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(in shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in GridShape.ReadOnly shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in GridShape.ReadOnly shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, ImmutableGridShape item, GridPosition pos, bool freeValue)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (GetCellValue(in container, gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in GridShape.ReadOnly container, ImmutableGridShape item, bool freeValue)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem, freeValue);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells(this in GridShape.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, bool> cellPredicate)
    {
        return CheckShapeCells(in grid, shape, position, static (pos, value, pred) => pred(pos, value), cellPredicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this in GridShape.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!Contains(in grid, gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this in GridShape.ReadOnly grid, ImmutableGridShape item, bool freeValue)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(in grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this in GridShape.ReadOnly grid, ImmutableGridShape shape, GridPosition position)
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this in GridShape.ReadOnly grid, ImmutableGridShape shape, int x, int y)
    {
        return IsWithinBounds(in grid, shape, new GridPosition(x, y));
    }

#endregion
    
#region ref ValueGridShape<T>

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : System.IEquatable<T> => GetIndex(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this ref ValueGridShape<T> shape, int x, int y) where T : System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : System.IEquatable<T> => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this ref ValueGridShape<T> shape, int x, int y) where T : System.IEquatable<T> => GetCellValue(ref shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this ref ValueGridShape<T> shape) where T : System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this ref ValueGridShape<T> shape) where T : System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : System.IEquatable<T> => Contains(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this ref ValueGridShape<T> shape, int x, int y) where T : System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this ref ValueGridShape<T> shape, T target) where T : System.IEquatable<T>
    {
        return Count(ref shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return Count(ref shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return Any(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return All(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed<T>(this ref ValueGridShape<T> shape, T freeValue) where T : System.IEquatable<T>
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(ref shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(ref shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(ref shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(ref shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(ref ValueGridShape<T> shape, int row, T freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = GetCellValue(ref shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(ref ValueGridShape<T> shape, int column, T freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = GetCellValue(ref shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T freeValue) where T : System.IEquatable<T>
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (GetCellValue(ref container, gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, T freeValue) where T : System.IEquatable<T>
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(ref container, rotatedItem, freeValue);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, bool> cellPredicate) where T : System.IEquatable<T>
    {
        return CheckShapeCells(ref grid, shape, position, static (pos, value, pred) => pred(pos, value), cellPredicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T, TData>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : System.IEquatable<T>
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!Contains(ref grid, gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation<T>(this ref ValueGridShape<T> grid, ImmutableGridShape item, T freeValue) where T : System.IEquatable<T>
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(ref grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position) where T : System.IEquatable<T>
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, int x, int y) where T : System.IEquatable<T>
    {
        return IsWithinBounds(ref grid, shape, new GridPosition(x, y));
    }

#endregion
    
#region in ValueGridShape<T>.ReadOnly

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : System.IEquatable<T> => GetIndex(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : System.IEquatable<T> => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : System.IEquatable<T> => GetCellValue(in shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this in ValueGridShape<T>.ReadOnly shape) where T : System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this in ValueGridShape<T>.ReadOnly shape) where T : System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : System.IEquatable<T> => Contains(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this in ValueGridShape<T>.ReadOnly shape, T target) where T : System.IEquatable<T>
    {
        return Count(in shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return Count(in shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return Any(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : System.IEquatable<T>
    {
        return All(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed<T>(this in ValueGridShape<T>.ReadOnly shape, T freeValue) where T : System.IEquatable<T>
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(in shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(in shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(in shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(in shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in ValueGridShape<T>.ReadOnly shape, int row, T freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in ValueGridShape<T>.ReadOnly shape, int column, T freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item, GridPosition pos, T freeValue) where T : System.IEquatable<T>
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (GetCellValue(in container, gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item, T freeValue) where T : System.IEquatable<T>
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem, freeValue);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, bool> cellPredicate) where T : System.IEquatable<T>
    {
        return CheckShapeCells(in grid, shape, position, static (pos, value, pred) => pred(pos, value), cellPredicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T, TData>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : System.IEquatable<T>
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!Contains(in grid, gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape item, T freeValue) where T : System.IEquatable<T>
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(in grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position) where T : System.IEquatable<T>
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, int x, int y) where T : System.IEquatable<T>
    {
        return IsWithinBounds(in grid, shape, new GridPosition(x, y));
    }

#endregion
    
#region in ImmutableGridShape

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in ImmutableGridShape shape, GridPosition pos) => GetIndex(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in ImmutableGridShape shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in ImmutableGridShape shape, GridPosition pos) => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in ImmutableGridShape shape, int x, int y) => GetCellValue(in shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this in ImmutableGridShape shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this in ImmutableGridShape shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in ImmutableGridShape shape, GridPosition pos) => Contains(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in ImmutableGridShape shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this in ImmutableGridShape shape, bool target)
    {
        return Count(in shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return Count(in shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return Any(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return All(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this in ImmutableGridShape shape, bool freeValue)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(in shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(in shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(in shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(in shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in ImmutableGridShape shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in ImmutableGridShape shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = GetCellValue(in shape, column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this in ImmutableGridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (GetCellValue(in container, gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in ImmutableGridShape container, ImmutableGridShape item, bool freeValue)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem, freeValue);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells(this in ImmutableGridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, bool> cellPredicate)
    {
        return CheckShapeCells(in grid, shape, position, static (pos, value, pred) => pred(pos, value), cellPredicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this in ImmutableGridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!Contains(in grid, gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this in ImmutableGridShape grid, ImmutableGridShape item, bool freeValue)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(in grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this in ImmutableGridShape grid, ImmutableGridShape shape, GridPosition position)
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds(this in ImmutableGridShape grid, ImmutableGridShape shape, int x, int y)
    {
        return IsWithinBounds(in grid, shape, new GridPosition(x, y));
    }

#endregion
    }

public static partial class WritableGridShapeExtension
{
    
#region ref GridShape

    public static void SetCellValue(this ref GridShape shape, GridPosition pos, bool value) => shape.SetCellValue(pos.X, pos.Y, value);
    public static void SetCellValue(this ref GridShape shape, int x, int y, bool value) => shape[shape.GetIndex(x, y)] = value;

    public static void FillAll(this ref GridShape shape, bool value)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
            shape[i] = value;
    }

    public static void FillRect(this ref GridShape shape, int x, int y, int width, int height, bool value)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var px = x + dx;
                var py = y + dy;
                if (px >= 0 && px < shape.Width && py >= 0 && py < shape.Height)
                {
                    shape.SetCellValue(px, py, value);
                }
            }
        }
    }

    public static void FillRect(this ref GridShape shape, GridPosition pos, int width, int height, bool value)
    {
        FillRect(ref shape, pos.X, pos.Y, width, height, value);
    }

    public static void FillShape(this ref GridShape grid, ImmutableGridShape shape, GridPosition pos, bool value)
    {
        // Check bounds to prevent out of range access
        var maxX = System.Math.Min(shape.Width, grid.Width - pos.X);
        var maxY = System.Math.Min(shape.Height, grid.Height - pos.Y);
        var startX = System.Math.Max(0, -pos.X);
        var startY = System.Math.Max(0, -pos.Y);

        // Fill only the cells where the shape has true values
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                if (shape[x, y])
                {
                    SetCellValue(ref grid, pos.X + x, pos.Y + y, value);
                }
            }
        }
    }

    public static void PlaceItem(this ref GridShape container, ImmutableGridShape item, GridPosition pos, bool value)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCellValue(gridPos, value);
            }
        }
    }

    public static void RemoveItem(this ref GridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCellValue(gridPos, freeValue);
            }
        }
    }

#endregion
    
#region ref ValueGridShape<T>

    public static void SetCellValue<T>(this ref ValueGridShape<T> shape, GridPosition pos, T value) where T : System.IEquatable<T> => shape.SetCellValue(pos.X, pos.Y, value);
    public static void SetCellValue<T>(this ref ValueGridShape<T> shape, int x, int y, T value) where T : System.IEquatable<T> => shape[shape.GetIndex(x, y)] = value;

    public static void FillAll<T>(this ref ValueGridShape<T> shape, T value) where T : System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
            shape[i] = value;
    }

    public static void FillRect<T>(this ref ValueGridShape<T> shape, int x, int y, int width, int height, T value) where T : System.IEquatable<T>
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var px = x + dx;
                var py = y + dy;
                if (px >= 0 && px < shape.Width && py >= 0 && py < shape.Height)
                {
                    shape.SetCellValue(px, py, value);
                }
            }
        }
    }

    public static void FillRect<T>(this ref ValueGridShape<T> shape, GridPosition pos, int width, int height, T value) where T : System.IEquatable<T>
    {
        FillRect(ref shape, pos.X, pos.Y, width, height, value);
    }

    public static void FillShape<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition pos, T value) where T : System.IEquatable<T>
    {
        // Check bounds to prevent out of range access
        var maxX = System.Math.Min(shape.Width, grid.Width - pos.X);
        var maxY = System.Math.Min(shape.Height, grid.Height - pos.Y);
        var startX = System.Math.Max(0, -pos.X);
        var startY = System.Math.Max(0, -pos.Y);

        // Fill only the cells where the shape has true values
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                if (shape[x, y])
                {
                    SetCellValue(ref grid, pos.X + x, pos.Y + y, value);
                }
            }
        }
    }

    public static void PlaceItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T value) where T : System.IEquatable<T>
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCellValue(gridPos, value);
            }
        }
    }

    public static void RemoveItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T freeValue) where T : System.IEquatable<T>
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCellValue(gridPos, freeValue);
            }
        }
    }

#endregion
    }

public static partial class Shapes
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Single(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(1, 1, allocator);
        shape.SetCellValue(0, 0, true);
        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Line(int length, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(length, 1, allocator);
        for (var i = 0; i < length; i++)
            shape.SetCellValue(i, 0, true);
        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Square(int size, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCellValue(x, y, true);
        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape LShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(2, 2, allocator);
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(0, 1, true);
        shape.SetCellValue(1, 1, true);
        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape TShape(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 2, allocator);
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(1, 0, true);
        shape.SetCellValue(2, 0, true);
        shape.SetCellValue(1, 1, true);
        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Cross(Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(3, 3, allocator);
        shape.SetCellValue(1, 0, true);
        shape.SetCellValue(0, 1, true);
        shape.SetCellValue(1, 1, true);
        shape.SetCellValue(2, 1, true);
        shape.SetCellValue(1, 2, true);
        return shape;
    }

    // Immutable shape factory methods
    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableSingle()
    {
        using var shape = Single(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableLine(int length)
    {
        using var shape = Line(length, Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableSquare(int size)
    {
        using var shape = Square(size, Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableLShape()
    {
        using var shape = LShape(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableTShape()
    {
        using var shape = TShape(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ImmutableCross()
    {
        using var shape = Cross(Allocator.Temp);
        return shape.GetOrCreateImmutable();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Rotate(this GridShape shape, RotationDegree degree, Allocator allocator = Allocator.Temp)
    {
        return shape.AsReadOnly().Rotate(degree, allocator);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Rotate(this in GridShape.ReadOnly shape, RotationDegree degree, Allocator allocator = Allocator.Temp)
    {
        var dimensions = shape.GetRotatedDimensions(degree);
        var rotated = new GridShape(dimensions.width, dimensions.height, allocator);
        shape.RotateBits(degree, rotated.Bits);
        return rotated;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (int width, int height) GetRotatedDimensions(this in GridShape.ReadOnly shape, RotationDegree degree)
    {
        return degree switch
        {
            RotationDegree.None => (shape.Width, shape.Height),
            RotationDegree.Clockwise90 => (shape.Height, shape.Width),
            RotationDegree.Clockwise180 => (shape.Width, shape.Height),
            RotationDegree.Clockwise270 => (shape.Height, shape.Width),
            _ => throw new System.NotImplementedException()
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
                    _ => throw new System.ArgumentException($"Invalid rotation degree: {degree}")
                };

                var index = rotatedY * newBound.width + rotatedX;
                output.Set(index, true);
            }
        }
        return new GridShape.ReadOnly(newBound.width, newBound.height, output.AsReadOnly());
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Flip(this GridShape shape, FlipAxis axis, Allocator allocator = Allocator.Temp)
    {
        return shape.AsReadOnly().Flip(axis, allocator);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Flip(this in GridShape.ReadOnly shape, FlipAxis axis, Allocator allocator = Allocator.Temp)
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
                    _ => throw new System.ArgumentException($"Invalid flip axis: {axis}")
                };

                var destIndex = flippedY * width + flippedX;
                output.Set(destIndex, true);
            }
        }

        return new GridShape.ReadOnly(width, height, output.AsReadOnly());
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape Trim(this in GridShape.ReadOnly shape, Allocator allocator = Allocator.Temp)
    {
        // Handle empty shapes
        if (shape.Width == 0 || shape.Height == 0)
            return new GridShape(0, 0, allocator);

        // If already trimmed, create a copy with the same cells
        if (shape.IsTrimmed(freeValue: false))
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

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape GetRotatedShape(this ImmutableGridShape shape, RotationDegree rotation)
    {
        return rotation switch
        {
            RotationDegree.None => shape,
            RotationDegree.Clockwise90 => shape.Rotate90(),
            RotationDegree.Clockwise180 => shape.Rotate90().Rotate90(),
            RotationDegree.Clockwise270 => shape.Rotate90().Rotate90().Rotate90(),
            _ => throw new System.ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape ToGridShape(this EditorGridShape editorShape, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(editorShape.Width, editorShape.Height, allocator);

        for (int y = 0; y < editorShape.Height; y++)
        {
            for (int x = 0; x < editorShape.Width; x++)
            {
                var index = y * editorShape.Width + x;
                if (index < editorShape.Shape.Length)
                {
                    shape[x, y] = editorShape.Shape[index];
                }
            }
        }

        return shape;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridShape ToTrimmedGridShape(this EditorGridShape editorShape, Allocator allocator = Allocator.Temp)
    {
        var shape = editorShape.ToGridShape(Allocator.Temp);
        return shape.AsReadOnly().Trim();
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static ImmutableGridShape ToImmutableGridShape(this EditorGridShape editorShape)
    {
        return editorShape.ToTrimmedGridShape(Allocator.Temp).GetOrCreateImmutable();
    }
}
