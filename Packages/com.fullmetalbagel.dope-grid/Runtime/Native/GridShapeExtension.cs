

namespace DopeGrid.Native;

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
    public static bool IsTrimmed(this ref GridShape shape, bool freeValue = default)
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
    public static bool CanPlaceItem(this ref GridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
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
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this ref GridShape container, ImmutableGridShape item)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(ref container, rotatedItem);
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
    public static GridPosition FindFirstFitWithFixedRotation(this ref GridShape grid, ImmutableGridShape item, bool freeValue = default)
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
    public static bool IsTrimmed(this in GridShape.ReadOnly shape, bool freeValue = default)
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
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
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
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in GridShape.ReadOnly container, ImmutableGridShape item)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem);
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
    public static GridPosition FindFirstFitWithFixedRotation(this in GridShape.ReadOnly grid, ImmutableGridShape item, bool freeValue = default)
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
    public static int GetIndex<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => GetIndex(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this ref ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this ref ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => GetCellValue(ref shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this ref ValueGridShape<T> shape) where T : unmanaged, System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this ref ValueGridShape<T> shape) where T : unmanaged, System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this ref ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => Contains(ref shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this ref ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this ref ValueGridShape<T> shape, T target) where T : unmanaged, System.IEquatable<T>
    {
        return Count(ref shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Count(ref shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool Any<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Any(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool All<T>(this ref ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return All(ref shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this ref ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool IsTrimmed<T>(this ref ValueGridShape<T> shape, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static bool CanPlaceItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this ref ValueGridShape<T> container, ImmutableGridShape item) where T : unmanaged, System.IEquatable<T>
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(ref container, rotatedItem);
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
    public static bool CheckShapeCells<T, TData>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : unmanaged, System.IEquatable<T>
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
    public static GridPosition FindFirstFitWithFixedRotation<T>(this ref ValueGridShape<T> grid, ImmutableGridShape item, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static bool IsWithinBounds<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position) where T : unmanaged, System.IEquatable<T>
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, int x, int y) where T : unmanaged, System.IEquatable<T>
    {
        return IsWithinBounds(ref grid, shape, new GridPosition(x, y));
    }

#endregion
    
#region in ValueGridShape<T>.ReadOnly

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => GetIndex(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => shape[shape.GetIndex(pos)];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => GetCellValue(in shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this in ValueGridShape<T>.ReadOnly shape) where T : unmanaged, System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this in ValueGridShape<T>.ReadOnly shape) where T : unmanaged, System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => Contains(in shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this in ValueGridShape<T>.ReadOnly shape, T target) where T : unmanaged, System.IEquatable<T>
    {
        return Count(in shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Count(in shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool Any<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Any(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool All<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return All(in shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
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
    public static bool IsTrimmed<T>(this in ValueGridShape<T>.ReadOnly shape, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static bool CanPlaceItem<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item, GridPosition pos, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item) where T : unmanaged, System.IEquatable<T>
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem);
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
    public static bool CheckShapeCells<T, TData>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : unmanaged, System.IEquatable<T>
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
    public static GridPosition FindFirstFitWithFixedRotation<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape item, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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
    public static bool IsWithinBounds<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position) where T : unmanaged, System.IEquatable<T>
    {
        return position is { X: >= 0, Y: >= 0 } &&
               position.X + shape.Width <= grid.Width &&
               position.Y + shape.Height <= grid.Height;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsWithinBounds<T>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, int x, int y) where T : unmanaged, System.IEquatable<T>
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
    public static bool IsTrimmed(this in ImmutableGridShape shape, bool freeValue = default)
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
    public static bool CanPlaceItem(this in ImmutableGridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
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
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in ImmutableGridShape container, ImmutableGridShape item)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(in container, rotatedItem);
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
    public static GridPosition FindFirstFitWithFixedRotation(this in ImmutableGridShape grid, ImmutableGridShape item, bool freeValue = default)
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

    public static void Clear(this ref GridShape shape)
    {
        FillAll(ref shape, default);
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

    public static void RemoveItem(this ref GridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
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

    public static void SetCellValue<T>(this ref ValueGridShape<T> shape, GridPosition pos, T value) where T : unmanaged, System.IEquatable<T> => shape.SetCellValue(pos.X, pos.Y, value);
    public static void SetCellValue<T>(this ref ValueGridShape<T> shape, int x, int y, T value) where T : unmanaged, System.IEquatable<T> => shape[shape.GetIndex(x, y)] = value;

    public static void FillAll<T>(this ref ValueGridShape<T> shape, T value) where T : unmanaged, System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
            shape[i] = value;
    }

    public static void FillRect<T>(this ref ValueGridShape<T> shape, int x, int y, int width, int height, T value) where T : unmanaged, System.IEquatable<T>
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

    public static void FillRect<T>(this ref ValueGridShape<T> shape, GridPosition pos, int width, int height, T value) where T : unmanaged, System.IEquatable<T>
    {
        FillRect(ref shape, pos.X, pos.Y, width, height, value);
    }

    public static void FillShape<T>(this ref ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition pos, T value) where T : unmanaged, System.IEquatable<T>
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

    public static void Clear<T>(this ref ValueGridShape<T> shape) where T : unmanaged, System.IEquatable<T>
    {
        FillAll(ref shape, default);
    }

    public static void PlaceItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T value) where T : unmanaged, System.IEquatable<T>
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

    public static void RemoveItem<T>(this ref ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T freeValue = default) where T : unmanaged, System.IEquatable<T>
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

