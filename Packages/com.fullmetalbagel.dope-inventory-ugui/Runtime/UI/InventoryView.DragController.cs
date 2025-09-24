using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DopeGrid.Inventory
{
    public partial class InventoryView : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData) => _dragController.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => _dragController.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => _dragController.OnEndDrag(eventData);

        private sealed class DragController : IDisposable
        {
            private readonly InventoryView _view;
            private readonly Canvas _canvas;
            private Image _ghost;
            private DraggingItem? _draggingItem;

            public DragController(InventoryView view)
            {
                _view = view;
                _canvas = view.GetComponentInParent<Canvas>();
                if (_canvas == null) throw new InvalidOperationException("InventoryView must be under a Canvas");
                _ghost = view._sharedInventoryData.Pool.Get();
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
                    _view._sharedInventoryData.Pool.Release(_ghost);
                    _ghost = null!;
                }
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (!_view.IsInitialized) return;
                var inventory = _view.ReadOnlyInventory;

                if (!TryGetPointerLocalTopLeft(eventData, out var fromTopLeft)) return;

                var gx = Mathf.FloorToInt(fromTopLeft.x / _view.CellSize.x);
                var gy = Mathf.FloorToInt(fromTopLeft.y / _view.CellSize.y);
                if (gx < 0 || gy < 0 || gx >= inventory.Width || gy >= inventory.Height) return;

                var item = inventory.GetItemAt(new int2(gx, gy));
                if (item.IsInvalid) return;
                if (!_view.TryGetSprite(item.DefinitionId, out var sprite)) return;

                _draggingItem = new DraggingItem(item.InstanceId, item.Definition, _ghost.rectTransform, item.Rotation);
                _view._sharedInventoryData.DraggingItems.Add(_draggingItem);

                var rotatedSize = new Vector2(item.Shape.Width * _view.CellSize.x, item.Shape.Height * _view.CellSize.y);
                var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var angleZ = InventoryView.GetZRotation(item.Rotation);

                if (_ghost == null) return;
                var grt = (RectTransform)_ghost.transform;
                grt.sizeDelta = preRotSize;
                grt.localEulerAngles = new Vector3(0f, 0f, angleZ);
                if (TryGetPointerLocalInCanvas(eventData, out var canvasLocal))
                {
                    grt.anchoredPosition = canvasLocal;
                }
                _ghost.sprite = sprite;
                _ghost.raycastTarget = false;
                _ghost.gameObject.SetActive(true);
                grt.SetAsLastSibling();
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (_draggingItem == null) return;
                if (_ghost == null) return;
                if (!TryGetPointerLocalInCanvas(eventData, out var canvasLocal)) return;
                var grt = (RectTransform)_ghost.transform;
                grt.anchoredPosition = canvasLocal;
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (_draggingItem != null)
                {
                    _view._sharedInventoryData.DraggingItems.Remove(_draggingItem);
                    _draggingItem = null;
                }

                if (_ghost != null)
                {
                    _ghost.gameObject.SetActive(false);
                }
            }

            private bool TryGetPointerLocalTopLeft(PointerEventData eventData, out Vector2 fromTopLeft)
            {
                var rectT = _view.RectTransform;
                var cam = eventData.pressEventCamera;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectT, eventData.position, cam, out var local))
                {
                    fromTopLeft = default;
                    return false;
                }

                var rect = rectT.rect;
                var size = rect.size;
                var left = -rectT.pivot.x * size.x;
                var top = (1 - rectT.pivot.y) * size.y;
                fromTopLeft = new Vector2(local.x - left, top - local.y);
                return true;
            }

            private bool TryGetPointerLocalInCanvas(PointerEventData eventData, out Vector2 canvasLocal)
            {
                var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
                return RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, eventData.position, cam, out canvasLocal);
            }
        }
    }
}
