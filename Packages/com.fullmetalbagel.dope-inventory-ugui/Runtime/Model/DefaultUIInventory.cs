using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

public sealed class DefaultUIInventory : IUIInventory
{
    private readonly IGameInventory _inventory;
    private readonly ISharedInventoryData _shared;
    private readonly ISharedUIInventoryData _sharedUI;
    private readonly DefaultInventoryItemViewPool _pool = new();

    public DefaultUIInventory(IGameInventory inventory, ISharedInventoryData shared, ISharedUIInventoryData sharedUI)
    {
        _inventory = inventory;
        _shared = shared;
        _sharedUI = sharedUI;
    }

    public IList<DraggingItem> DraggingItems => _sharedUI.DraggingItems;

    [Pure, MustUseReturnValue]
    public RotationDegree GetRotation(Guid itemInstanceId) => _shared.GetRotation(itemInstanceId);

    [Pure, MustUseReturnValue]
    public Sprite GetSprite(Guid itemInstanceId) => _sharedUI.GetSprite(itemInstanceId);

    [Pure, MustUseReturnValue]
    public ImmutableGridShape GetShape(Guid itemInstanceId) => _shared.GetShape(itemInstanceId);

    [Pure, MustUseReturnValue]
    public BoardItemData GetItemById(int id) => _inventory.GetItemById(id);

    public int Width => _inventory.Width;
    public int Height => _inventory.Height;
    public int this[int x, int y] => _inventory[x, y];

    [Pure, MustUseReturnValue]
    public bool IsOccupied(int x, int y) => _inventory.IsOccupied(x, y);

    [Pure, MustUseReturnValue]
    public GameInventoryEnumerator GetEnumerator() => _inventory.GetEnumerator();

    [Pure, MustUseReturnValue]
    public BoardItemData GetItem(Guid itemInstanceId) => _inventory.GetItem(itemInstanceId);

    [Pure, MustUseReturnValue]
    public Guid GetItemInstanceId(int itemId) => _inventory.GetItemInstanceId(itemId);

    [Pure, MustUseReturnValue]
    public bool CanMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation) => _inventory.CanMoveItem(itemInstanceId, x, y, rotation);

    public bool TryMoveItem(Guid itemInstanceId) => _inventory.TryMoveItem(itemInstanceId);
    public bool TryMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation) => _inventory.TryMoveItem(itemInstanceId, x, y, rotation);
    public bool TryMoveItem(Guid itemInstanceId, RotationDegree rotation) => _inventory.TryMoveItem(itemInstanceId, rotation);
    public bool TryRemoveItem(Guid itemInstanceId) => _inventory.TryRemoveItem(itemInstanceId);

    public Image GetImage() => _pool.Get();
    public void ReleaseImage(Image item) => _pool.Release(item);
}
