using System;
using System.Collections.Generic;

namespace DopeGrid;

public readonly struct IndexedGridBoard<TItemData> : IReadOnlyGridShape<TItemData>, IDisposable, IEquatable<IndexedGridBoard<TItemData>>
{
    private readonly ValueGridShape<int> _grid; // Stores item index at each cell (-1 for empty)
    public ValueGridShape<int>.ReadOnly Grid => _grid;

    private readonly ValueGridShape<int> _initializedGrid;
    public ValueGridShape<int>.ReadOnly InitializedGrid => _initializedGrid;

    private readonly List<TItemData> _items;
    public IReadOnlyList<TItemData> Items => _items;

    private readonly List<ImmutableGridShape> _itemShapes;
    public IReadOnlyList<ImmutableGridShape> ItemShapes => _itemShapes;

    private readonly List<(int x, int y)> _itemPosition;
    public IReadOnlyList<(int x, int y)> ItemPositions => _itemPosition;

    private readonly Queue<int> _freeIndices;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public int ItemCount => _items.Count;
    public int FreeSpace => Grid.CountValue(-1);

    public IndexedGridBoard(int width, int height)
    {
        _grid = new ValueGridShape<int>(width, height, -1);
        _initializedGrid = _grid.Clone();
        _items = ListPool<TItemData>.Rent();
        _itemShapes = ListPool<ImmutableGridShape>.Rent();
        _itemPosition = ListPool<(int x, int y)>.Rent();
        _freeIndices = QueuePool<int>.Rent();
    }

    public IndexedGridBoard(ValueGridShape<int> containerShape)
    {
        if (containerShape.IsEmpty()) throw new ArgumentException("invalid initial shape", nameof(containerShape));
        _grid = containerShape.Clone();
        _initializedGrid = _grid.Clone();
        _items = ListPool<TItemData>.Rent();
        _itemShapes = ListPool<ImmutableGridShape>.Rent();
        _itemPosition = ListPool<(int x, int y)>.Rent();
        _freeIndices = QueuePool<int>.Rent();
    }

    public bool IsOccupied(int x, int y) => _grid.IsOccupied(x, y);
    public TItemData this[int x, int y] => _items[_grid[x, y]];

    public (int index, RotationDegree rotation) TryAddItem(TItemData data, ImmutableGridShape item)
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
        if (_freeIndices.TryDequeue(out var itemIndex))
        {
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

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            var shape = _itemShapes[index];
            var (x, y) = _itemPosition[index];
            _grid.FillShapeWithValue(shape, x, y, -1);

            _items[index] = default!;
            _itemShapes[index] = ImmutableGridShape.Empty;
            _itemPosition[index] = (-1, -1);
            _freeIndices.Enqueue(index);
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
        QueuePool<int>.Return(_freeIndices);
    }

    public static implicit operator ReadOnly(IndexedGridBoard<TItemData> board) => board.AsReadOnly();
    public ReadOnly AsReadOnly() => new(_grid.AsReadOnly(), _items);

    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(IndexedGridBoard<TItemData> left, IndexedGridBoard<TItemData> right) => left.Equals(right);
    public static bool operator !=(IndexedGridBoard<TItemData> left, IndexedGridBoard<TItemData> right) => !(left == right);
    public bool Equals(IndexedGridBoard<TItemData> other) => throw new NotSupportedException();

    public readonly struct ReadOnly : IReadOnlyGridShape<TItemData>, IEquatable<ReadOnly>
    {
        private readonly ValueGridShape<int>.ReadOnly _grid;
        private readonly IReadOnlyList<TItemData> _items;

        public ValueGridShape<int>.ReadOnly Grid => _grid;
        public int Width => _grid.Width;
        public int Height => _grid.Height;

        public int ItemCount => _items.Count;
        public int FreeSpace => _grid.CountValue(-1);

        public bool IsOccupied(int x, int y) => _grid.IsOccupied(x, y);
        public TItemData this[int x, int y] => _items[_grid[x, y]];

        internal ReadOnly(ValueGridShape<int>.ReadOnly grid, IReadOnlyList<TItemData> items)
        {
            _grid = grid;
            _items = items;
        }

        public override bool Equals(object obj) => throw new NotSupportedException();
        public override int GetHashCode() => throw new NotSupportedException();
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !(left == right);
        public bool Equals(ReadOnly other) => throw new NotSupportedException();
    }
}
