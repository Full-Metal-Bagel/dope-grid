using System;
using JetBrains.Annotations;

namespace DopeGrid.Inventory;

public interface IGameInventory : IReadOnlyIndexedGridBoard
{
    [Pure, MustUseReturnValue]
    GameInventoryEnumerator GetEnumerator();

    [Pure, MustUseReturnValue]
    Guid GetItemInstanceId(int itemId);

    [Pure, MustUseReturnValue]
    BoardItemData GetItem(Guid itemInstanceId);

    [Pure, MustUseReturnValue]
    bool CanMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation);

    bool TryMoveItem(Guid itemInstanceId);
    bool TryMoveItem(Guid itemInstanceId, RotationDegree rotation);
    bool TryMoveItem(Guid itemInstanceId, int x, int y, RotationDegree rotation);
    bool TryRemoveItem(Guid itemInstanceId);
}

public ref struct GameInventoryEnumerator
{
    private IndexedGridBoard.Enumerator _enumerator;
    private readonly IGameInventory _inventory;
    public GameInventoryEnumerator(IGameInventory inventory, IndexedGridBoard.Enumerator enumerator)
    {
        _inventory = inventory;
        _enumerator = enumerator;
    }

    public bool MoveNext() => _enumerator.MoveNext();
    public void Reset() => _enumerator.Reset();
    public Guid Current => _inventory.GetItemInstanceId(_enumerator.Current.Id);
}
