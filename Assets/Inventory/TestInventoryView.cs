using System;
using System.Collections.Generic;
using DopeGrid;
using DopeGrid.Inventory;
using DopeGrid.Native;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TestInventoryView : MonoBehaviour
{
    [SerializeField] private InventoryView _view = null!;
    [SerializeField] private int _width = 6;
    [SerializeField] private int _height = 6;
    [SerializeField] private UIImageGridDefinition[] _uiItems = Array.Empty<UIImageGridDefinition>();

    private Inventory _inventory;
    private Items _items;

    private void Start()
    {
        _items = GetComponentInParent<Items>();
        // Create an inventory with persistent allocator; dispose on destroy
        _inventory = new Inventory(_width, _height, Allocator.Persistent);

        BuildDefinitionsMap();
        PopulateSampleItems();

        if (_view != null)
        {
            _view.Initialize(_inventory, _items.SharedInventoryData);
        }
    }

    private void BuildDefinitionsMap()
    {
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;
            _items.SharedInventoryData.Definitions[guid] = ui.ToData();
        }
    }

    private void PopulateSampleItems()
    {
        if (_uiItems == null || _uiItems.Length == 0) return;

        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;

            // Build model item definition from UI shape
            var itemDef = new ItemDefinition(guid, ui.Shape.ToImmutableGridShape());

            var placedItem = _inventory.TryAutoPlaceItem(itemDef);
            if (!placedItem.IsValid)
            {
                Debug.LogWarning($"Inventory full; failed to place item ({ui.name}).");
            }
        }
    }

    private void OnDestroy()
    {
        _inventory.Dispose();
    }
}
