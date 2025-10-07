using System;
using System.Collections.Generic;

namespace DopeGrid.Inventory;

public sealed class DefaultGameInventory : IGameInventory, IDisposable
{
    private readonly IndexedGridBoard _board;
    private readonly List<Guid> _itemInstanceList = new();
    private readonly Dictionary<Guid, int> _itemInstanceIdMap = new();
    private readonly ISharedInventoryData _shared;
    public int Width => _board.Width;
    public int Height => _board.Height;

    public DefaultGameInventory(int width, int height, ISharedInventoryData shared)
    {
        _board = new IndexedGridBoard(width, height);
        _shared = shared;
    }

    public BoardItemData GetItemById(int id) => _board.GetItemById(id);
    public bool IsOccupied(int x, int y) => _board.IsOccupied(x, y);
    public int this[int x, int y] => _board[x, y];
    public GameInventoryEnumerator GetEnumerator() => new(this, _board.GetEnumerator());

    private int GetItemId(Guid itemInstanceId) => _itemInstanceIdMap.GetValueOrDefault(itemInstanceId, -1);
    public BoardItemData GetItem(Guid itemInstanceId) => _board.GetItemById(GetItemId(itemInstanceId));
    public Guid GetItemInstanceId(int itemId) => _itemInstanceList[itemId];

    public bool CanMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation)
    {
        var item = GetItem(itemInstanceId);
        var shape = _shared.GetShape(itemInstanceId);
        shape = shape.GetRotatedShape(rotation);
        return item.IsInvalid ?
            // move item from other inventory
            _board.CanPlaceItem(shape, x, y, default(int)) :
            // move item in this inventory
            _board.CanMoveItem(item.Id, shape, x, y);
    }

    public bool TryMoveItem(Guid itemInstanceId)
    {
        var item = GetItem(itemInstanceId);
        if (item.IsValid) return false;
        var shape = _shared.GetShape(itemInstanceId);
        var (x, y, rotation) = _board.FindFirstFitWithFreeRotation(shape, default(int));
        return ForceMoveItem(itemInstanceId, x, y, rotation);
    }

    public bool TryMoveItem(Guid itemInstanceId, RotationDegree rotation)
    {
        var item = GetItem(itemInstanceId);
        var currentRotation = _shared.GetRotation(itemInstanceId);
        if (item.IsValid && currentRotation == rotation) return false;

        var shape = _shared.GetShape(itemInstanceId);
        shape = shape.GetRotatedShape(rotation);
        var (x, y) = _board.FindFirstFitWithFixedRotation(shape, default(int));
        return ForceMoveItem(itemInstanceId, x, y, rotation);
    }

    public bool TryMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation)
    {
        var item = GetItem(itemInstanceId);
        var currentRotation = _shared.GetRotation(itemInstanceId);
        if (item.IsValid && item.X == x && item.Y == y && currentRotation == rotation) return false;
        if (!CanMoveItem(itemInstanceId, x, y, rotation)) return false;
        return ForceMoveItem(itemInstanceId, x, y, rotation);
    }

    private bool ForceMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation)
    {
        if (itemInstanceId == Guid.Empty || x < 0 || y < 0) return false;

        var shape = _shared.GetShape(itemInstanceId);
        if (shape == ImmutableGridShape.Empty) return false;

        shape = shape.GetRotatedShape(rotation);
        RemoveItemFromAnyOwner(itemInstanceId);
        ForceAddItem(itemInstanceId, shape, x, y, rotation);
        return true;
    }

    public bool TryRemoveItem(Guid itemInstanceId)
    {
        var itemId = GetItemId(itemInstanceId);
        if (itemId == -1) return false;
        ForceRemoveItemFromThis(itemInstanceId);
        return true;
    }

    private void RemoveItemFromAnyOwner(Guid itemInstanceId)
    {
        var item = GetItem(itemInstanceId);
        if (item.IsValid) ForceRemoveItemFromThis(itemInstanceId);
        else _shared.GetOwner(itemInstanceId)?.TryRemoveItem(itemInstanceId);
    }

    private void ForceRemoveItemFromThis(Guid itemInstanceId)
    {
        var itemId = GetItemId(itemInstanceId);
        var removed = _board.RemoveItem(itemId);
        if (removed.IsInvalid) throw new ArgumentException();

        _itemInstanceIdMap[itemInstanceId] = -1;
        _itemInstanceList[itemId] = Guid.Empty;

        if (_shared.GetOwner(itemInstanceId) == this)
            _shared.SetOwner(itemInstanceId, null);
    }

    private void ForceAddItem(Guid itemInstanceId, ImmutableGridShape shape, int x, int y, RotationDegree rotation)
    {
        var added = _board.TryAddItemAt(shape, x, y);
        if (added.IsInvalid) throw new ArgumentException();

        _itemInstanceIdMap[itemInstanceId] = added.Id;
        while (_itemInstanceList.Count <= added.Id)
        {
            _itemInstanceList.Add(Guid.Empty);
        }
        _itemInstanceList[added.Id] = itemInstanceId;

        _shared.SetRotation(itemInstanceId, rotation);
        _shared.SetOwner(itemInstanceId, this);
    }

    public void Dispose()
    {
        _board.Dispose();
    }
}
