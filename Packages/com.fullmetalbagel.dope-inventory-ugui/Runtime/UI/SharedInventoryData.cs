using System;
using System.Collections.Generic;

namespace DopeGrid.Inventory;

public class SharedInventoryData
{
    public Dictionary<Guid, UIItemDefinition> Definitions { get; } = new();
    public IInventoryItemViewPool Pool { get; }
    public List<DraggingItem> DraggingItems { get; } = new();

    public SharedInventoryData() : this(new DefaultInventoryItemViewPool())
    {
    }

    public SharedInventoryData(IInventoryItemViewPool pool)
    {
        Pool = pool;
    }
}
