using System;
using System.Collections.Generic;
using UnityEngine;

namespace DopeGrid.Inventory;

public class SharedInventoryData
{
    public Dictionary<Guid, UIImageGridDefinitionData> Definitions { get; } = new();
    public IInventoryItemViewPool Pool { get; }
    // public List<DraggingItem> DraggingItems { get; } = new();

    public SharedInventoryData() : this(new DefaultInventoryItemViewPool())
    {
    }

    public SharedInventoryData(IInventoryItemViewPool pool)
    {
        Pool = pool;
    }

    public Sprite? TryGetSprite(Guid definitionId)
    {
        return Definitions.TryGetValue(definitionId, out var ui) && ui != null && ui.Image != null ? ui.Image : null;
    }
}
