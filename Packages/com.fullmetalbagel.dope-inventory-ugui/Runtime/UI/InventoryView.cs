using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryView : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Vector2 _cellSize = new(64f, 64f);

        private Inventory _inventory;
        private IReadOnlyDictionary<Guid, UIItemDefinition> _definitions = null!;
        private IInventoryItemViewPool _pool = null!;

        private readonly Dictionary<int, Image> _itemViews = new(); // instanceId -> Image
        private DragController _dragController = null!;
        private RectTransform Rect => (RectTransform)transform;

        internal Inventory Inventory => _inventory;
        internal Vector2 CellSize => _cellSize;
        internal RectTransform RectTransform => Rect;

        protected override void Awake()
        {
#if UNITY_EDITOR
            gameObject.AddComponent<InventoryViewDebugOverlay>();
#endif
        }

        public void Initialize(Inventory inventory, IReadOnlyDictionary<Guid, UIItemDefinition> definitions, IInventoryItemViewPool? pool = null)
        {
            _inventory = inventory;
            _definitions = definitions;
            _pool = pool ?? new DefaultInventoryItemViewPool();
            Debug.Assert(_inventory.IsCreated, this);
            Rect.sizeDelta = new Vector2(_inventory.Width * _cellSize.x, _inventory.Height * _cellSize.y);

            _dragController = new DragController(this);
        }

        public void Update()
        {
            if (!_inventory.IsCreated) return;
            var seen = HashSetPool<int>.Get();
            var toRemove = ListPool<int>.Get();
            try
            {
                SyncViews(seen, toRemove);
            }
            finally
            {
                HashSetPool<int>.Release(seen);
                ListPool<int>.Release(toRemove);
            }
        }

        private void SyncViews(HashSet<int> seen, List<int> toRemove)
        {
            // Iterate all items from model
            for (int i = 0; i < _inventory.ItemCount; i++)
            {
                if (!_inventory.TryGetItem(i, out var item))
                    continue;

                var instanceId = item.InstanceId;
                seen.Add(instanceId);

                // Lookup UI definition by Guid
                if (!_definitions.TryGetValue(item.DefinitionId, out var itemUI) || itemUI == null)
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
                var angleZ = GetZRotation(item.Rotation);
                var offset = GetRotationOffset(preRotSize, item.Rotation);
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
                    _pool.Release(image);
                }
                _itemViews.Remove(id);
            }
        }

        private Image GetOrCreateItemView(int instanceId)
        {
            if (_itemViews.TryGetValue(instanceId, out var existing) && existing != null)
                return existing;

            var image = _pool.Get();
#if UNITY_EDITOR
            image.name = $"Item_{instanceId}";
#endif
            image.transform.SetParent(transform, false);

            var rt = (RectTransform)image.transform;
            EnsureTopLeftAnchors(rt);

            _itemViews[instanceId] = image;
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

        private static float GetZRotation(RotationDegree rotation) => rotation switch
        {
            RotationDegree.None => 0f,
            RotationDegree.Clockwise90 => -90f,
            RotationDegree.Clockwise180 => -180f,
            RotationDegree.Clockwise270 => -270f,
            _ => 0f
        };

        private static Vector2 GetRotationOffset(Vector2 size, RotationDegree rotation)
        {
            var w = size.x;
            var h = size.y;
            return rotation switch
            {
                RotationDegree.None => Vector2.zero,
                RotationDegree.Clockwise90 => new Vector2(h, 0f),
                RotationDegree.Clockwise180 => new Vector2(w, -h),
                RotationDegree.Clockwise270 => new Vector2(0f, -w),
                _ => Vector2.zero
            };
        }

        private bool TryGetSprite(Guid definitionId, out Sprite? sprite)
        {
            if (_definitions.TryGetValue(definitionId, out var ui) && ui != null && ui.Image != null)
            {
                sprite = ui.Image;
                return true;
            }
            sprite = null;
            return false;
        }

        public void OnBeginDrag(PointerEventData eventData) => _dragController.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => _dragController.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => _dragController.OnEndDrag(eventData);

        private sealed class DragController : IDisposable
        {
            private readonly InventoryView _view;
            private Image _ghost;
            private int _dragItemIndex = -1;

            public DragController(InventoryView view)
            {
                _view = view;
                _ghost = view._pool.Get();
#if UNITY_EDITOR
                _ghost.name = "__ghost__";
#endif
                _ghost.transform.SetParent(_view.GetComponentInParent<Canvas>().transform, false);
                var rt = (RectTransform)_ghost.transform;
                EnsureTopLeftAnchors(rt);
                _ghost.preserveAspect = false;
                _ghost.raycastTarget = false;
                _ghost.gameObject.SetActive(false);
            }

            public void Dispose()
            {
                if (_ghost != null)
                {
                    _view._pool.Release(_ghost);
                    _ghost = null!;
                }
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                var inventory = _view.Inventory;
                if (!inventory.IsCreated) return;

                if (!TryGetPointerLocalTopLeft(eventData, out var fromTopLeft)) return;

                var gx = Mathf.FloorToInt(fromTopLeft.x / _view.CellSize.x);
                var gy = Mathf.FloorToInt(fromTopLeft.y / _view.CellSize.y);
                if (gx < 0 || gy < 0 || gx >= inventory.Width || gy >= inventory.Height) return;

                var idx = inventory.GetItemAt(new int2(gx, gy));
                if (idx < 0) return;
                if (!inventory.TryGetItem(idx, out var item)) return;
                if (!_view.TryGetSprite(item.DefinitionId, out var sprite)) return;

                _dragItemIndex = idx;

                var rotatedSize = new Vector2(item.Shape.Width * _view.CellSize.x, item.Shape.Height * _view.CellSize.y);
                var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var angleZ = InventoryView.GetZRotation(item.Rotation);

                if (_ghost == null) return;
                var grt = (RectTransform)_ghost.transform;
                grt.sizeDelta = preRotSize;
                grt.localEulerAngles = new Vector3(0f, 0f, angleZ);
                grt.anchoredPosition = new Vector2(fromTopLeft.x, -fromTopLeft.y);
                _ghost.sprite = sprite;
                _ghost.raycastTarget = false;
                _ghost.gameObject.SetActive(true);
                grt.SetAsLastSibling();
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (_dragItemIndex < 0) return;
                if (_ghost == null) return;
                if (!TryGetPointerLocalTopLeft(eventData, out var fromTopLeft)) return;
                var grt = (RectTransform)_ghost.transform;
                grt.anchoredPosition = new Vector2(fromTopLeft.x, -fromTopLeft.y);
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                _dragItemIndex = -1;
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
        }
    }
}
