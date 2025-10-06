    using System;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Pool;
    using UnityEngine.UI;

    namespace DopeGrid.Inventory;

    internal sealed class InventoryViewDragPreviewController : IDisposable
    {
        private readonly SharedInventoryData _sharedInventoryData;
        private readonly RectTransform _view;
        private readonly Vector2 _cellSize;
        private readonly Color _placeableColor;
        private readonly Color _blockedColor;
        private readonly Dictionary<DraggingItem, Image> _draggingViews = new();

        public InventoryViewDragPreviewController(SharedInventoryData sharedInventoryData, RectTransform view, Vector2 cellSize, Color placeableColor, Color blockedColor)
        {
            _sharedInventoryData = sharedInventoryData;
            _view = view;
            _cellSize = cellSize;
            _placeableColor = placeableColor;
            _blockedColor = blockedColor;
        }

        public void Dispose()
        {
            foreach (var kv in _draggingViews)
            {
                if (kv.Value != null)
                {
                    _sharedInventoryData.Pool.Release(kv.Value);
                }
            }
            _draggingViews.Clear();
        }

        public void UpdateDragPlacementPreview(IndexedGridBoard inventory)
        {
            var seen = HashSetPool<DraggingItem>.Get();
            var toRemove = ListPool<DraggingItem>.Get();

            try
            {
                for (int i = 0; i < _sharedInventoryData.DraggingItems.Count; i++)
                {
                    var item = _sharedInventoryData.DraggingItems[i];
                    // var itemData = item.Data;
                    seen.Add(item);

                    var sprite = item.Image;
                    if (!sprite) continue;

                    var shape = item.Shape;
                    var gridPos = item.GetGridPosition(_view, _cellSize);

                    // Get or create preview view
                    if (!_draggingViews.TryGetValue(item, out var preview) || preview == null)
                    {
                        preview = _sharedInventoryData.Pool.Get();
    #if UNITY_EDITOR
                        preview.name = "__placement_preview__";
    #endif
                        preview.transform.SetParent(_view, false);
                        var prt = (RectTransform)preview.transform;
                        EnsureTopLeftAnchors(prt);
                        _draggingViews[item] = preview;
                    }

                    // Only show preview if the item is within inventory bounds
                    if (gridPos.x < 0 || gridPos.y < 0 ||
                        gridPos.x + shape.Width > inventory.Width ||
                        gridPos.y + shape.Height > inventory.Height)
                    {
                        preview.gameObject.SetActive(false);
                        continue;
                    }

                    // TODO: can move
                    var canPlace = inventory.CanPlaceItem(shape, gridPos.x, gridPos.y, default(int));
                    // item.TargetInventory = inventory;
                    // item.TargetPosition = gridPos;
                    // item.LastFrame = Time.frameCount;

                    // Mirror transform logic from SyncViews
                    var rotatedSize = new Vector2(shape.Width * _cellSize.x, shape.Height * _cellSize.y);
                    var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                        ? new Vector2(rotatedSize.y, rotatedSize.x)
                        : rotatedSize;
                    var anchoredPos = GridToAnchoredPosition(gridPos);
                    var angleZ = item.Rotation.GetZRotation();
                    var (offsetX, offsetY) = item.Rotation.GetRotationOffset(preRotSize.x, preRotSize.y);
                    var offset = new Vector2(offsetX, offsetY);

                    var rt = (RectTransform)preview.transform;
                    rt.sizeDelta = preRotSize;
                    rt.anchoredPosition = anchoredPos + offset;
                    rt.localEulerAngles = new Vector3(0f, 0f, angleZ);
                    rt.SetAsLastSibling();

                    preview.sprite = sprite;
                    preview.preserveAspect = false;
                    preview.raycastTarget = false;
                    preview.color = canPlace ? _placeableColor : _blockedColor;
                    preview.gameObject.SetActive(true);
                }

                // Release previews that are no longer dragged
                foreach (var kv in _draggingViews)
                {
                    if (!seen.Contains(kv.Key))
                        toRemove.Add(kv.Key);
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    var key = toRemove[i];
                    var img = _draggingViews[key];
                    if (img != null) _sharedInventoryData.Pool.Release(img);
                    _draggingViews.Remove(key);
                }
            }
            finally
            {
                HashSetPool<DraggingItem>.Release(seen);
                ListPool<DraggingItem>.Release(toRemove);
            }
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
