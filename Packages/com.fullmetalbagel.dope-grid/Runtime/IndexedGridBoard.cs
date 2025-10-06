using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types")]
public readonly struct IndexedGridBoard<TItemData> : IReadOnlyGridShape<int>, IDisposable, IEquatable<IndexedGridBoard<TItemData>>
{
    public readonly record struct ItemData(int Id, TItemData Data, ImmutableGridShape Shape, int X, int Y)
    {
        public int Id { get; } = Id;
        public TItemData Data { get; } = Data;
        public ImmutableGridShape Shape { get; } = Shape;
        public int X { get; } = X;
        public int Y { get; } = Y;

        public static ItemData Invalid => new(-1, default!, ImmutableGridShape.Empty, -1, -1);
        public bool IsValid => !IsInvalid;
        public bool IsInvalid => Id < 0 || X < 0 || Y < 0 || Shape.IsZeroSize();
    }

    private readonly ValueGridShape<int> _grid; // Stores item index at each cell (-1 for empty)
    private readonly ValueGridShape<int> _initializedGrid;

    private readonly List<TItemData> _items;
    private readonly List<ImmutableGridShape> _itemShapes;
    private readonly List<(int x, int y)> _itemPosition;
    private readonly HashSet<int> _freeIndices;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public ValueGridShape<int>.ReadOnly Grid => _grid;
    public ValueGridShape<int>.ReadOnly InitializedGrid => _initializedGrid;

    public int ItemCount => _items.Count - _freeIndices.Count;
    public int ItemCapacity => _items.Count;
    public int FreeSpace => Grid.CountValue(-1);

    public IndexedGridBoard(int width, int height)
    {
        _grid = new ValueGridShape<int>(width, height, -1);
        _initializedGrid = _grid.Clone();
        _items = ListPool<TItemData>.Rent();
        _itemShapes = ListPool<ImmutableGridShape>.Rent();
        _itemPosition = ListPool<(int x, int y)>.Rent();
        _freeIndices = HashSetPool<int>.Rent();
    }

    public static IndexedGridBoard<TItemData> CreateFromShape<TShape>(TShape shape)
        where TShape : struct, IReadOnlyGridShape<bool>
    {
        var board = new IndexedGridBoard<TItemData>(shape.Width, shape.Height);
        for (var y = 0; y < shape.Height; y++)
        for (var x = 0; x < shape.Width; x++)
        {
            if (!shape.IsOccupied(x, y))
            {
                board._grid[x, y] = int.MinValue;
                board._initializedGrid[x, y] = int.MinValue;
            }
        }
        return board;
    }

    public bool IsOccupied(int x, int y) => _grid.IsOccupied(x, y);
    public int this[int x, int y] => _grid[x, y];

    public ItemData GetItemOnPosition(int x, int y)
    {
        return GetItemById(this[x, y]);
    }

    public ItemData GetItemById(int id)
    {
        if (id < 0 || id >= _items.Count) return ItemData.Invalid;
        var (positionX, positionY) = _itemPosition[id];
        var item = new ItemData(id, _items[id], _itemShapes[id], positionX, positionY);
        return item.IsValid ? item : ItemData.Invalid;
    }

    public (int id, RotationDegree rotation) TryAddItem(TItemData data, ImmutableGridShape item)
    {
        var (x, y, rotation) = _grid.FindFirstFitWithFreeRotation(item, default(int));
        if (x >= 0)
        {
            var index = AddItemAt(data, item.GetRotatedShape(rotation), x, y);
            return (index, rotation);
        }

        return (-1, RotationDegree.None);
    }

    public int TryAddItemAt(TItemData item, ImmutableGridShape shape, int x, int y)
    {
        if (_grid.CanPlaceItem(shape, x, y, default(int)))
        {
            return AddItemAt(item, shape, x, y);
        }
        return -1;
    }

    private int AddItemAt(TItemData itemData, ImmutableGridShape itemShape, int x, int y)
    {
        int itemIndex;
        // Reuse any free index
        if (_freeIndices.Count > 0)
        {
            // Take any index from the set (enumeration order is undefined but fast)
            using var enumerator = _freeIndices.GetEnumerator();
            enumerator.MoveNext();
            itemIndex = enumerator.Current;
            _freeIndices.Remove(itemIndex);

            _items[itemIndex] = itemData;
            _itemShapes[itemIndex] = itemShape;
            _itemPosition[itemIndex] = (x, y);
        }
        else
        {
            itemIndex = _items.Count;
            _items.Add(itemData);
            _itemShapes.Add(itemShape);
            _itemPosition.Add((x, y));
        }

        _grid.FillShapeWithValue(itemShape, x, y, itemIndex);
        return itemIndex;
    }

    public void RemoveItem(int id)
    {
        if (id >= 0 && id < _items.Count)
        {
            var shape = _itemShapes[id];
            var (x, y) = _itemPosition[id];
            _grid.FillShapeWithValue(shape, x, y, -1);

            _items[id] = default!;
            _itemShapes[id] = ImmutableGridShape.Empty;
            _itemPosition[id] = (-1, -1);
            _freeIndices.Add(id);
        }
    }

    public void Clear()
    {
        _initializedGrid.CopyTo(_grid);
        _items.Clear();
        _itemShapes.Clear();
        _itemPosition.Clear();
        _freeIndices.Clear();
    }

    public void Dispose()
    {
        _initializedGrid.Dispose();
        _grid.Dispose();
        ListPool<TItemData>.Return(_items);
        ListPool<ImmutableGridShape>.Return(_itemShapes);
        ListPool<(int x, int y)>.Return(_itemPosition);
        HashSetPool<int>.Return(_freeIndices);
    }

    public Enumerator GetEnumerator() => new(this);

    public static implicit operator ReadOnly(IndexedGridBoard<TItemData> board) => board.AsReadOnly();
    public ReadOnly AsReadOnly() => new(this);

    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(IndexedGridBoard<TItemData> left, IndexedGridBoard<TItemData> right) => left.Equals(right);
    public static bool operator !=(IndexedGridBoard<TItemData> left, IndexedGridBoard<TItemData> right) => !(left == right);
    public bool Equals(IndexedGridBoard<TItemData> other) => throw new NotSupportedException();

    public readonly struct ReadOnly : IReadOnlyGridShape<int>, IEquatable<ReadOnly>
    {
        private readonly IndexedGridBoard<TItemData> _board;

        public ValueGridShape<int>.ReadOnly Grid => _board.Grid;
        internal HashSet<int> FreeIndices => _board._freeIndices;

        public int Width => Grid.Width;
        public int Height => Grid.Height;

        public int ItemCount => _board.ItemCount;
        public int ItemCapacity => _board.ItemCapacity;
        public int FreeSpace => _board.FreeSpace;

        public bool IsOccupied(int x, int y) => _board.IsOccupied(x, y);
        public int this[int x, int y] => _board[x, y];

        internal ReadOnly(IndexedGridBoard<TItemData> board)
        {
            _board = board;
        }

        public Enumerator GetEnumerator() => new(this);

        public ItemData GetItemOnPosition(int x, int y)
        {
            return _board.GetItemOnPosition(x, y);
        }

        public ItemData GetItemById(int id)
        {
            return _board.GetItemById(id);
        }

        public override bool Equals(object? obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !(left == right);
        public bool Equals(ReadOnly other) => throw new NotSupportedException();
    }

    public ref struct Enumerator
    {
        private readonly ReadOnly _board;
        private int _currentIndex;

        internal Enumerator(ReadOnly board)
        {
            _board = board;
            _currentIndex = -1;
        }

        public readonly ItemData Current
        {
            get
            {
                if (_currentIndex < 0 || _currentIndex >= _board.ItemCapacity)
                    throw new InvalidOperationException("Enumeration has not started or has already finished");
                return _board.GetItemById(_currentIndex);
            }
        }

        public bool MoveNext()
        {
            while (++_currentIndex < _board.ItemCapacity)
            {
                if (!_board.FreeIndices.Contains(_currentIndex))
                    return true;
            }
            return false;
        }

        public void Reset() => _currentIndex = -1;
    }
}
