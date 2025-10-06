using System;
using DopeGrid;
using DopeGrid.Inventory;

public record struct InventoryItem(Guid DefinitionId, ImmutableGridShape DefinitionShape, RotationDegree Rotation) : IInventoryItem
{
    public ImmutableGridShape RotatedShape => DefinitionShape.GetRotatedShape(Rotation);
}

public class InventoryView : InventoryView<InventoryItem>
{
    protected override void Awake()
    {
        gameObject.AddComponent<InventoryDebugView>();
    }
}

public class InventoryDebugView : InventoryViewDebugOverlay<InventoryItem>
{
}
