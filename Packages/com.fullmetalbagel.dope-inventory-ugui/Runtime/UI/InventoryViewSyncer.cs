using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

internal sealed class InventoryViewSyncer : IDisposable
{
    private readonly IUIInventory _inventory;
    private readonly Transform _parent;
    private readonly Vector2 _cellSize;
    private readonly Dictionary<int, Image> _itemViews = new();

    public InventoryViewSyncer(IUIInventory inventory, Transform parent, Vector2 cellSize)
    {
        _inventory = inventory;
        _parent = parent;
        _cellSize = cellSize;
    }

    public void Dispose()
    {
        foreach (var kv in _itemViews)
        {
            if (kv.Value != null)
            {
                _inventory.ReleaseImage(kv.Value);
            }
        }
        _itemViews.Clear();
    }

    public void SyncViews()
    {
        var seen = HashSetPool<int>.Get();
        var toRemove = ListPool<int>.Get();

        try
        {
            // Iterate all items from model
            foreach (var itemInstanceId in _inventory)
            {
                var item = _inventory.GetItem(itemInstanceId);
                seen.Add(item.Id);

                var sprite = _inventory.GetSprite(itemInstanceId);
                var rotation = _inventory.GetRotation(itemInstanceId);

                var image = GetOrCreateItemView(item.Id);
                image.sprite = sprite;
                image.raycastTarget = false;
                image.preserveAspect = false;

                // Compute target AABB (rotated shape) and pre-rotation rect size
                var rotatedSize = new Vector2(item.Shape.Width * _cellSize.x, item.Shape.Height * _cellSize.y);
                var preRotSize = rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var pos = new Vector2Int(item.X, item.Y);
                var anchoredPos = GridToAnchoredPosition(pos);

                var rt = (RectTransform)image.transform;
                EnsureTopLeftAnchors(rt);
                rt.sizeDelta = preRotSize;
                var angleZ = rotation.GetZRotation();
                var (offsetX, offsetY) = rotation.GetRotationOffset(preRotSize.x, preRotSize.y);
                var offset = new Vector2(offsetX, offsetY);
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
                    _inventory.ReleaseImage(image);
                }
                _itemViews.Remove(id);
            }
        }
        finally
        {
            HashSetPool<int>.Release(seen);
            ListPool<int>.Release(toRemove);
        }
    }

    private Image GetOrCreateItemView(int id)
    {
        if (_itemViews.TryGetValue(id, out var existing) && existing != null)
            return existing;

        var image = _inventory.GetImage();
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

    private Vector2 GridToAnchoredPosition(Vector2Int gridPos)
    {
        // Top-left origin: X grows right, Y grows down
        return new Vector2(gridPos.x * _cellSize.x, -(gridPos.y * _cellSize.y));
    }
}
