using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types")]
public readonly struct IndexedGridBoard : IIndexedGridBoard, IReadOnlyIndexedGridBoard, IDisposable, IEquatable<IndexedGridBoard>
{
    private readonly ValueGridShape<int> _grid; // Stores item index at each cell (-1 for empty)
    private readonly ValueGridShape<int> _initializedGrid;

    private readonly List<ImmutableGridShape> _items;
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
        _items = ListPool<ImmutableGridShape>.Rent();
        _itemPosition = ListPool<(int x, int y)>.Rent();
        _freeIndices = HashSetPool<int>.Rent();
    }

    public static IndexedGridBoard CreateFromShape<TShape>(TShape shape)
        where TShape : struct, IReadOnlyGridShape<bool>
    {
        var board = new IndexedGridBoard(shape.Width, shape.Height);
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

    public BoardItemData GetItemById(int id)
    {
        if (id < 0 || id >= _items.Count) return BoardItemData.Invalid;
        var (positionX, positionY) = _itemPosition[id];
        var item = new BoardItemData(id, _items[id], positionX, positionY);
        return item.IsValid ? item : BoardItemData.Invalid;
    }

    public BoardItemData TryAddItem(ImmutableGridShape shape)
    {
        var (x, y) = _grid.FindFirstFitWithFixedRotation(shape, default(int));
        return x >= 0 ? AddItemAt(shape, x, y) : BoardItemData.Invalid;
    }

    public BoardItemData TryAddItemAt(ImmutableGridShape shape, int x, int y)
    {
        return _grid.CanPlaceItem(shape, x, y, default(int)) ? AddItemAt(shape, x, y) : BoardItemData.Invalid;
    }

    private BoardItemData AddItemAt(ImmutableGridShape itemShape, int x, int y)
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

            _items[itemIndex] = itemShape;
            _itemPosition[itemIndex] = (x, y);
        }
        else
        {
            itemIndex = _items.Count;
            _items.Add(itemShape);
            _itemPosition.Add((x, y));
        }

        _grid.FillShapeWithValue(itemShape, x, y, itemIndex);
        return new  BoardItemData(itemIndex, itemShape, x, y);
    }

    public BoardItemData RemoveItem(int id)
    {
        var item = GetItemById(id);
        if (item.IsInvalid) return BoardItemData.Invalid;

        _grid.FillShapeWithValue(item.Shape, item.X, item.Y, -1);

        _items[id] = ImmutableGridShape.Empty;
        _itemPosition[id] = (-1, -1);
        _freeIndices.Add(id);
        return item;
    }

    public void Reset()
    {
        _initializedGrid.CopyTo(_grid);
        _items.Clear();
        _itemPosition.Clear();
        _freeIndices.Clear();
    }

    public void Dispose()
    {
        _initializedGrid.Dispose();
        _grid.Dispose();
        ListPool<ImmutableGridShape>.Return(_items);
        ListPool<(int x, int y)>.Return(_itemPosition);
        HashSetPool<int>.Return(_freeIndices);
    }

    public Enumerator GetEnumerator() => new(this);

    public static implicit operator ReadOnly(IndexedGridBoard board) => board.AsReadOnly();
    public ReadOnly AsReadOnly() => new(this);

    public bool IsSame(IndexedGridBoard other) => ReferenceEquals(_items, other._items);

    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(IndexedGridBoard left, IndexedGridBoard right) => left.Equals(right);
    public static bool operator !=(IndexedGridBoard left, IndexedGridBoard right) => !(left == right);
    public bool Equals(IndexedGridBoard other) => throw new NotSupportedException();

    public readonly struct ReadOnly : IReadOnlyIndexedGridBoard, IEquatable<ReadOnly>
    {
        private readonly IndexedGridBoard _board;

        public ValueGridShape<int>.ReadOnly Grid => _board.Grid;
        internal HashSet<int> FreeIndices => _board._freeIndices;

        public int Width => Grid.Width;
        public int Height => Grid.Height;

        public int ItemCount => _board.ItemCount;
        public int ItemCapacity => _board.ItemCapacity;
        public int FreeSpace => _board.FreeSpace;

        public bool IsOccupied(int x, int y) => _board.IsOccupied(x, y);
        public int this[int x, int y] => _board[x, y];

        internal ReadOnly(IndexedGridBoard board) => _board = board;
        public Enumerator GetEnumerator() => new(this);
        public BoardItemData GetItemById(int id) => _board.GetItemById(id);
        public bool IsSame(ReadOnly other) => _board.IsSame(other._board);

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

        public readonly BoardItemData Current
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
