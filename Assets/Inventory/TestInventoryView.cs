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
    [SerializeField] private UIItemDefinition[] _uiItems = Array.Empty<UIItemDefinition>();

    private Inventory _inventory;
    private readonly Dictionary<Guid, UIItemDefinition> _definitions = new();

    private void Start()
    {
        // Create an inventory with persistent allocator; dispose on destroy
        _inventory = new Inventory(_width, _height, Allocator.Persistent);

        BuildDefinitionsMap();
        PopulateSampleItems();

        if (_view != null)
        {
            _view.Initialize(_inventory, _definitions);
        }
    }

    private void BuildDefinitionsMap()
    {
        _definitions.Clear();
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;
            _definitions[guid] = ui;
        }
    }

    private void PopulateSampleItems()
    {
        if (_uiItems == null || _uiItems.Length == 0) return;

        var rotations = new[]
        {
            RotationDegree.None,
            RotationDegree.Clockwise90,
            RotationDegree.Clockwise180,
            RotationDegree.Clockwise270
        };

        var instanceId = 1;
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;

            // Build model item definition from UI shape
            var itemDef = new ItemDefinition(guid, ui.Shape.ToImmutableGridShape());
            var rotation = rotations[(instanceId - 1) % rotations.Length];
            var candidate = new InventoryItem(instanceId, itemDef, rotation, new int2(-1, -1));

            if (!_inventory.TryAutoPlaceItem(candidate, out _))
            {
                Debug.LogWarning($"Inventory full; failed to place item instance {instanceId} ({ui.name}).");
            }

            instanceId++;
        }
    }

    private void OnDestroy()
    {
        _inventory.Dispose();
    }
}
