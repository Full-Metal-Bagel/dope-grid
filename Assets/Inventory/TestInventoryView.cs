using System;
using System.Collections.Generic;
using DopeGrid;
using DopeGrid.Inventory;
using UnityEngine;

class InventoryUI : IInventoryUI
{
    public Dictionary<int, (Sprite image, RotationDegree rotation)> Items { get; } = new();

    public RotationDegree GetItemRotation(int id) => Items[id].rotation;
    public Sprite GetItemSprite(int id) => Items[id].image;
    public bool HasUI(int id) => Items.ContainsKey(id);
}

public class TestInventoryView : MonoBehaviour
{
    [SerializeField] private InventoryView _view = null!;
    [SerializeField] private int _width = 6;
    [SerializeField] private int _height = 6;
    [SerializeField] private UIImageGridDefinition[] _uiItems = Array.Empty<UIImageGridDefinition>();

    private IndexedGridBoard _inventory;
    private Shared _shared;

    private void Start()
    {
        _shared = GetComponentInParent<Shared>();
        // Create an inventory; dispose on destroy
        _inventory = new IndexedGridBoard(_width, _height);

        BuildDefinitionsMap();
        var inventoryUI = PopulateSampleItems();

        _view.Initialize(_inventory, _shared.SharedInventoryData, inventoryUI);
    }

    private void BuildDefinitionsMap()
    {
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;
            _shared.SharedInventoryData.Definitions[guid] = ui.ToData();
        }
    }

    private IInventoryUI PopulateSampleItems()
    {
        var inventoryUI = new InventoryUI();
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
            if (!Guid.TryParse(ui.Id, out var guid)) continue;

            // Build model item from UI shape
            var shape = ui.Shape.ToImmutableGridShape();
            var rotation = rotations[rotationIndex];
            rotationIndex = (rotationIndex + 1) % rotations.Length;

            var (item, rot) = _inventory.TryAddItem(shape.GetRotatedShape(rotation));
            if (item.IsInvalid)
            {
                Debug.LogWarning($"Inventory full; failed to place item ({ui.name}).");
            }
            else
            {
                inventoryUI.Items.Add(item.Id, (ui.Image, rotation.Rotate(rot)));
            }
        }
        return inventoryUI;
    }

    private void OnDestroy()
    {
        _inventory.Dispose();
    }
}
