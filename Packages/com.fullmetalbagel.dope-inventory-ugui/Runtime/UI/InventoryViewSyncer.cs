using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

internal sealed class InventoryViewSyncer : IDisposable
{
    private readonly SharedInventoryData _sharedInventoryData;
    private readonly Transform _parent;
    private readonly Vector2 _cellSize;
    private readonly Dictionary<InventoryItemInstanceId, Image> _itemViews = new();

    public InventoryViewSyncer(SharedInventoryData sharedInventoryData, Transform parent, Vector2 cellSize)
    {
        _sharedInventoryData = sharedInventoryData;
        _parent = parent;
        _cellSize = cellSize;
    }

    public void Dispose()
    {
        foreach (var kv in _itemViews)
        {
            if (kv.Value != null)
            {
                _sharedInventoryData.Pool.Release(kv.Value);
            }
        }
        _itemViews.Clear();
    }

    public void SyncViews(Inventory.ReadOnly inventory)
    {
        var seen = HashSetPool<InventoryItemInstanceId>.Get();
        var toRemove = ListPool<InventoryItemInstanceId>.Get();

        try
        {
            // Iterate all items from model
            for (int i = 0; i < inventory.ItemCount; i++)
            {
                var item = inventory[i];

                var instanceId = item.InstanceId;
                seen.Add(instanceId);

                // Lookup UI definition by Guid
                if (!_sharedInventoryData.Definitions.TryGetValue(item.DefinitionId, out var itemUI) || itemUI == null)
                    continue; // No UI—skip rendering

                var image = GetOrCreateItemView(instanceId);
                image.sprite = itemUI.Image;
                image.raycastTarget = false;
                image.preserveAspect = false;

                // Compute target AABB (rotated shape) and pre-rotation rect size
                var rotatedSize = new Vector2(item.Shape.Width * _cellSize.x, item.Shape.Height * _cellSize.y);
                var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var pos = item.Position; // int2, top-left origin in model
                var anchoredPos = GridToAnchoredPosition(pos);

                var rt = (RectTransform)image.transform;
                EnsureTopLeftAnchors(rt);
                rt.sizeDelta = preRotSize;
                var angleZ = item.Rotation.GetZRotation();
                var offset = item.Rotation.GetRotationOffset(preRotSize);
                rt.anchoredPosition = anchoredPos + offset;
                rt.localEulerAngles = new Vector3(0f, 0f, angleZ);
            }

            // Remove any views not present anymore
            foreach (var kv in _itemViews)
            {
                if (!seen.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var id in toRemove)
            {
                var image = _itemViews[id];
                if (image != null)
                {
                    _sharedInventoryData.Pool.Release(image);
                }
                _itemViews.Remove(id);
            }
        }
        finally
        {
            HashSetPool<InventoryItemInstanceId>.Release(seen);
            ListPool<InventoryItemInstanceId>.Release(toRemove);
        }
    }

    private Image GetOrCreateItemView(InventoryItemInstanceId id)
    {
        if (_itemViews.TryGetValue(id, out var existing) && existing != null)
            return existing;

        var image = _sharedInventoryData.Pool.Get();
#if UNITY_EDITOR
        image.name = $"Item_{id}";
#endif
        image.transform.SetParent(_parent, false);
        image.gameObject.SetActive(true);
        image.color = Color.white;

        var rt = (RectTransform)image.transform;
        EnsureTopLeftAnchors(rt);

        _itemViews[id] = image;
        return image;
    }

    private static void EnsureTopLeftAnchors(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
    }

    private Vector2 GridToAnchoredPosition(int2 gridPos)
    {
        // Top-left origin: X grows right, Y grows down
        return new Vector2(gridPos.x * _cellSize.x, -(gridPos.y * _cellSize.y));
    }
}
