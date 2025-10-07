using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DopeGrid.Inventory;

public interface ISharedUIInventoryData
{
    IList<DraggingItem> DraggingItems { get; }

    [Pure, MustUseReturnValue]
    Sprite GetSprite(Guid itemInstanceId);
}

public class SharedUIInventoryData : ISharedUIInventoryData
{
    public Dictionary<Guid, Sprite> ItemSpriteMap { get; } = new();
    public IList<DraggingItem> DraggingItems { get; } = new List<DraggingItem>();
    public Sprite GetSprite(Guid itemInstanceId) => ItemSpriteMap[itemInstanceId];
}
