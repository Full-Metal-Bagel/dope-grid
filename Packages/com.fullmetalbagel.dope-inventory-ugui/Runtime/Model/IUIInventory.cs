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
