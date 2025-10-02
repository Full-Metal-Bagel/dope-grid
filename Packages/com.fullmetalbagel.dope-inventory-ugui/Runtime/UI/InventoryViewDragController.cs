using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

internal sealed class InventoryViewDragController : IDisposable
{
    private readonly SharedInventoryData _sharedInventoryData;
    private readonly Canvas _canvas;
    private readonly RectTransform _inventoryTransform;
    private readonly Inventory _inventory;
    private readonly Vector2 _cellSize;
    private Image _ghost;
    private DraggingItem? _draggingItem;

    public InventoryViewDragController(SharedInventoryData sharedInventoryData, RectTransform inventoryTransform, Inventory inventory, Vector2 cellSize)
    {
        _sharedInventoryData = sharedInventoryData;
        _inventoryTransform = inventoryTransform;
        _inventory = inventory;
        _cellSize = cellSize;
        _canvas = inventoryTransform.GetComponentInParent<Canvas>();
        if (_canvas == null) throw new InvalidOperationException("InventoryView must be under a Canvas");
        _ghost = _sharedInventoryData.Pool.Get();
#if UNITY_EDITOR
        _ghost.name = "__ghost__";
#endif
        _ghost.transform.SetParent(_canvas.transform, false);
        var rt = (RectTransform)_ghost.transform;
        // Center pivot/anchors so ghost stays under the pointer
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        _ghost.preserveAspect = false;
        _ghost.raycastTarget = false;
        _ghost.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        if (_ghost != null)
        {
            _sharedInventoryData.Pool.Release(_ghost);
            _ghost = null!;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_inventoryTransform.TryGetPointerLocalTopLeft(eventData, out var fromTopLeft)) return;

        var gx = Mathf.FloorToInt(fromTopLeft.x / _cellSize.x);
        var gy = Mathf.FloorToInt(fromTopLeft.y / _cellSize.y);
        if (gx < 0 || gy < 0 || gx >= _inventory.Width || gy >= _inventory.Height) return;

        var item = _inventory.GetItemAt(new int2(gx, gy));
        if (item.IsInvalid) return;
        var sprite = _sharedInventoryData.TryGetSprite(item.DefinitionId);
        if (!sprite) return;

        _draggingItem = new DraggingItem(item.InstanceId, item.Definition, _ghost.rectTransform, item.Rotation);
        _draggingItem.SourceInventory = _inventory;
        _sharedInventoryData.DraggingItems.Add(_draggingItem);

        if (_ghost == null) return;
        UpdateDraggingItemRotation();
        if (_canvas.TryGetPointerLocalInCanvas(eventData, out var canvasLocal))
        {
            ((RectTransform)_ghost.transform).anchoredPosition = canvasLocal;
        }
        _ghost.sprite = sprite;
        _ghost.raycastTarget = false;
        _ghost.gameObject.SetActive(true);
        _ghost.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingItem == null) return;
        if (_ghost == null) return;
        if (!_canvas.TryGetPointerLocalInCanvas(eventData, out var canvasLocal)) return;

        var grt = (RectTransform)_ghost.transform;
        grt.anchoredPosition = canvasLocal;
    }

    public void OnEndDrag(PointerEventData _)
    {
        if (_draggingItem != null)
        {
            // Check if we have a valid target position from the most recent frame
            if (Time.frameCount - _draggingItem.LastFrame <= 1 && _draggingItem.TargetInventory.IsCreated)
            {
                var targetPos = _draggingItem.TargetPosition;
                var sourceInventory = _draggingItem.SourceInventory;
                var targetInventory = _draggingItem.TargetInventory;

                // Check if it's the same inventory by comparing the underlying native containers
                var isSameInventory = sourceInventory.IsSame(targetInventory);

                if (isSameInventory)
                {
                    // Same inventory - move the item with updated rotation using atomic operation
                    sourceInventory.TryMoveItem(_draggingItem.InstanceId, targetPos, _draggingItem.Rotation);
                }
                else
                {
                    // Cross-inventory move - get item, remove from source, add to target
                    // var item = sourceInventory.GetItemByInstanceId(_draggingItem.InstanceId);
                    // if (item.IsValid)
                    // {
                    //     var newItem = new InventoryItem(_draggingItem.InstanceId, item.Definition, _draggingItem.Rotation, targetPos);
                    //     if (targetInventory.TryPlaceItem(newItem))
                    //     {
                    //         sourceInventory.RemoveItem(_draggingItem.InstanceId);
                    //     }
                    // }
                }
            }

            _sharedInventoryData.DraggingItems.Remove(_draggingItem);
            _draggingItem = null;
        }

        if (_ghost != null)
        {
            _ghost.gameObject.SetActive(false);
        }
    }

    public RotationDegree GetRotation()
    {
        return _draggingItem?.Rotation ?? RotationDegree.None;
    }

    public void SetRotation(RotationDegree rotation)
    {
        if (_draggingItem == null) return;

        _draggingItem.Rotation = rotation;
        UpdateDraggingItemRotation();
    }

    private void UpdateDraggingItemRotation()
    {
        if (_draggingItem == null) return;
        var (width, height) = _draggingItem.Shape.Bound;
        var size = new Vector2(_cellSize.x * width, _cellSize.y * height) ;
        _draggingItem.Rotation.ApplyToRectTransform(_draggingItem.View, size);
    }
}
