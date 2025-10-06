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

    private IndexedGridBoard<InventoryItem> _inventory;
    private Shared _shared;

    private void Start()
    {
        _shared = GetComponentInParent<Shared>();
        // Create an inventory; dispose on destroy
        _inventory = new IndexedGridBoard<InventoryItem>(_width, _height);

        BuildDefinitionsMap();
        PopulateSampleItems();

        if (_view != null)
        {
            _view.Initialize(_inventory, _shared.SharedInventoryData);
        }
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

        var rotationIndex = 0;
        foreach (var ui in _uiItems)
        {
            if (ui == null) continue;
            if (!Guid.TryParse(ui.Id, out var guid)) continue;

            // Build model item from UI shape
            var shape = ui.Shape.ToImmutableGridShape();
            var rotation = rotations[rotationIndex];
            rotationIndex = (rotationIndex + 1) % rotations.Length;
            var item = new InventoryItem(guid, shape, rotation);

            var (id, rot) = _inventory.TryAddItem(item, item.RotatedShape);
            if (rot != RotationDegree.None)
            {
                _inventory.UpdateItem(id, item with { Rotation = rotation.Rotate(rot) });
            }

            if (id < 0)
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
