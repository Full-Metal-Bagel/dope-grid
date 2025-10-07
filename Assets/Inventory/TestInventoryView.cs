using System;
using DopeGrid;
using DopeGrid.Inventory;
using UnityEngine;

public class TestInventoryView : MonoBehaviour
{
    [SerializeField] private InventoryView _view = null!;
    [SerializeField] private int _width = 6;
    [SerializeField] private int _height = 6;
    [SerializeField] private UIImageGridDefinition[] _uiItems = Array.Empty<UIImageGridDefinition>();

    private DefaultGameInventory _inventory;

    private void Start()
    {
        var shared = GetComponentInParent<Shared>();
        _inventory = new DefaultGameInventory(_width, _height, shared.SharedData);
        var inventoryUI = PopulateSampleItems(_inventory, shared);
        _view.Initialize(inventoryUI);
    }

    private IUIInventory PopulateSampleItems(IGameInventory inventory, Shared shared)
    {
        var inventoryUI = new DefaultUIInventory(inventory, shared.SharedData, shared.SharedUIData);
        if (_uiItems == null || _uiItems.Length == 0) return inventoryUI;

        var rotations = new[]
        {
            RotationDegree.None,
            RotationDegree.Clockwise90,
            RotationDegree.Clockwise180,
            RotationDegree.Clockwise270
        };

        var rotationIndex = 0;
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;

            // Build model item from UI shape
            var shape = ui.Shape.ToImmutableGridShape();
            var rotation = rotations[rotationIndex];
            rotationIndex = (rotationIndex + 1) % rotations.Length;
            var id = Guid.NewGuid();
            shared.SharedData.ItemRotationMap[id] = rotation;
            shared.SharedData.ItemShapeMap[id] = shape;
            shared.SharedUIData.ItemSpriteMap[id] = ui.Image;

            var added = _inventory.TryMoveItem(id, rotation);
            if (!added)
            {
                Debug.LogWarning($"Inventory full; failed to place item ({ui.name}).");
            }
        }
        return inventoryUI;
    }

    private void OnDestroy()
    {
        _inventory.Dispose();
    }
}
