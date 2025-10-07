using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

internal sealed class InventoryViewDragController : IDisposable
{
    private readonly Canvas _canvas;
    private readonly RectTransform _inventoryTransform;
    private readonly IUIInventory _inventory;
    private readonly Vector2 _cellSize;
    private Image _ghost;
    private DraggingItem? _draggingItem;

    public InventoryViewDragController(IUIInventory inventory, RectTransform inventoryTransform, Vector2 cellSize)
    {
        _inventoryTransform = inventoryTransform;
        _inventory = inventory;
        _cellSize = cellSize;
        _canvas = inventoryTransform.GetComponentInParent<Canvas>();
        if (_canvas == null) throw new InvalidOperationException("InventoryView must be under a Canvas");
        _ghost = _inventory.GetImage();
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
            _inventory.ReleaseImage(_ghost);
            _ghost = null!;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_inventoryTransform.TryGetPointerLocalTopLeft(eventData, out var fromTopLeft)) return;

        var gx = Mathf.FloorToInt(fromTopLeft.x / _cellSize.x);
        var gy = Mathf.FloorToInt(fromTopLeft.y / _cellSize.y);
        if (gx < 0 || gy < 0 || gx >= _inventory.Width || gy >= _inventory.Height) return;

        var item = _inventory.GetItemOnPosition(gx, gy);
        if (item.IsInvalid) return;
        var itemInstanceId = _inventory.GetItemInstanceId(item.Id);
        var rotation = _inventory.GetRotation(itemInstanceId);
        _draggingItem = new DraggingItem(itemInstanceId, _ghost.rectTransform, rotation);
        _inventory.DraggingItems.Add(_draggingItem);

        if (_ghost == null) return;
        UpdateDraggingItemRotation();
        if (_canvas.TryGetPointerLocalInCanvas(eventData, out var canvasLocal))
        {
            ((RectTransform)_ghost.transform).anchoredPosition = canvasLocal;
        }
        _ghost.sprite = _inventory.GetSprite(itemInstanceId);
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
            if (Time.frameCount - _draggingItem.LastFrame <= 1 && _draggingItem.TargetInventory != null)
            {
                _draggingItem.TargetInventory.TryMoveItem(
                    _draggingItem.ItemInstanceId,
                    _draggingItem.TargetPosition.x,
                    _draggingItem.TargetPosition.y,
                    _draggingItem.Rotation
                );
            }

            _inventory.DraggingItems.Remove(_draggingItem);
            _draggingItem = null;
        }

        if (_ghost != null)
        {
            _ghost.gameObject.SetActive(false);
        }
    }

    public RotationDegree GetDraggingItemRotation()
    {
        return _draggingItem?.Rotation ?? RotationDegree.None;
    }

    public void SetDraggingItemRotation(RotationDegree rotation)
    {
        if (_draggingItem == null) return;

        _draggingItem.Rotation = rotation;
        UpdateDraggingItemRotation();
    }

    private void UpdateDraggingItemRotation()
    {
        if (_draggingItem == null) return;
        var shape = _inventory.GetShape(_draggingItem.ItemInstanceId);
        shape = shape.GetRotatedShape(_draggingItem.Rotation);
        var (width, height) = shape.Bound;
        var size = new Vector2(_cellSize.x * width, _cellSize.y * height) ;
        ApplyToRectTransform(_draggingItem.Rotation, _draggingItem.View, size);
    }

    private static void ApplyToRectTransform(RotationDegree rotation, RectTransform transform, Vector2 size)
    {
        var angleZ = rotation.GetZRotation();
        var (x, y) = rotation.CalculateRotatedSize(size.x, size.y);
        transform.sizeDelta = new Vector2(x, y);
        transform.localEulerAngles = new Vector3(0f, 0f, angleZ);
    }
}
