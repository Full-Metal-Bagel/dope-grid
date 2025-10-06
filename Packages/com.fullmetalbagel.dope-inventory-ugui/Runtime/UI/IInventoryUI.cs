using UnityEngine;

namespace DopeGrid.Inventory;

public interface IInventoryUI
{
    RotationDegree GetItemRotation(int id);
    Sprite GetItemSprite(int id);
    bool HasUI(int id);
}
