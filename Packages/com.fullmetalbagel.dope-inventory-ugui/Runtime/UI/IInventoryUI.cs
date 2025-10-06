namespace DopeGrid.Inventory;

public interface IInventoryUI
{
    bool HasUI(int id);
    RotationDegree GetItemRotation(int id);
    UIImageGridDefinitionData GetItemDefinition(int id);
}
