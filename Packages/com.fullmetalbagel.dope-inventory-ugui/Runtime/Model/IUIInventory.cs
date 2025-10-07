using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

public interface IUIInventory : IGameInventory
{
    RotationDegree GetRotation(Guid itemInstanceId);
    Sprite GetSprite(Guid itemInstanceId);
    ImmutableGridShape GetShape(Guid itemInstanceId);

    IList<DraggingItem> DraggingItems { get; }

    Image GetImage();
    void ReleaseImage(Image item);
}

public static class UIInventoryExtensions
{
    public static ImmutableGridShape GetRotatedShape(this IUIInventory inventory, Guid itemInstanceId)
    {
        var shape = inventory.GetShape(itemInstanceId);
        var rotation = inventory.GetRotation(itemInstanceId);
        return shape.GetRotatedShape(rotation);
    }
}
