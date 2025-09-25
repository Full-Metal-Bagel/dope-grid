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
        [SerializeField] private Color _placeableColor = new(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color _blockedColor = new(1f, 0f, 0f, 0.35f);

        private Inventory _inventory;
        private SharedInventoryData _sharedInventoryData = null!;

        private readonly Dictionary<int/*item instance*/, Image> _itemViews = new();
        private readonly Dictionary<int/*item instance*/, Image> _draggingViews = new();
        private InventoryViewDragController _dragController = null!;

        private RectTransform Rect => (RectTransform)transform;

        public Inventory.ReadOnly ReadOnlyInventory => _inventory;
        internal Vector2 CellSize => _cellSize;
        internal RectTransform RectTransform => Rect;

        public bool IsInitialized => _inventory.IsCreated;

        protected override void Awake()
        {
#if UNITY_EDITOR
            gameObject.AddComponent<InventoryViewDebugOverlay>();
#endif
        }

        public void Initialize(Inventory inventory, SharedInventoryData sharedInventoryData)
        {
            _inventory = inventory;
            _sharedInventoryData = sharedInventoryData;
            Debug.Assert(_inventory.IsCreated, this);
            Rect.sizeDelta = new Vector2(_inventory.Width * _cellSize.x, _inventory.Height * _cellSize.y);
            _dragController = new InventoryViewDragController(_sharedInventoryData, RectTransform, inventory, CellSize);
        }

        public void Update()
        {
            if (!_inventory.IsCreated) return;
            var seen = HashSetPool<int>.Get();
            var toRemove = ListPool<int>.Get();
            try
            {
                SyncViews(seen, toRemove, _inventory.AsReadOnly());
                UpdateDragPlacementPreview(seen, toRemove, _inventory.AsReadOnly());
            }
            finally
            {
                HashSetPool<int>.Release(seen);
                ListPool<int>.Release(toRemove);
            }
        }

        private void SyncViews(HashSet<int> seen, List<int> toRemove, Inventory.ReadOnly inventory)
        {
            seen.Clear();
            toRemove.Clear();

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

        private void UpdateDragPlacementPreview(HashSet<int> seen, List<int> toRemove, Inventory.ReadOnly inventory)
        {
            seen.Clear();
            toRemove.Clear();

            for (int i = 0; i < _sharedInventoryData.DraggingItems.Count; i++)
            {
                var item = _sharedInventoryData.DraggingItems[i];
                seen.Add(item.InstanceId);

                var sprite = _sharedInventoryData.TryGetSprite(item.DefinitionId);
                if (!sprite) continue;

                var shape = item.Shape;
                var gridPos = item.GetGridPosition(this);

                // Get or create preview view
                if (!_draggingViews.TryGetValue(item.InstanceId, out var preview) || preview == null)
                {
                    preview = _sharedInventoryData.Pool.Get();
#if UNITY_EDITOR
                    preview.name = $"__preview__{item.InstanceId}";
#endif
                    preview.transform.SetParent(transform, false);
                    var prt = (RectTransform)preview.transform;
                    EnsureTopLeftAnchors(prt);
                    _draggingViews[item.InstanceId] = preview;
                }

                // Only show preview if the item is within inventory bounds
                if (gridPos.x < 0 || gridPos.y < 0 ||
                    gridPos.x + shape.Width > inventory.Width ||
                    gridPos.y + shape.Height > inventory.Height)
                {
                    preview.gameObject.SetActive(false);
                    continue;
                }

                var canPlace = inventory.CanMoveItem(item.InstanceId, shape, gridPos);
                item.TargetInventory = _inventory;
                item.TargetPosition = gridPos;
                item.LastFrame = Time.frameCount;

                // Mirror transform logic from SyncViews
                var rotatedSize = new Vector2(shape.Width * _cellSize.x, shape.Height * _cellSize.y);
                var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var anchoredPos = GridToAnchoredPosition(gridPos);
                var angleZ = item.Rotation.GetZRotation();
                var offset = item.Rotation.GetRotationOffset(preRotSize);

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

        private Image GetOrCreateItemView(int instanceId)
        {
            if (_itemViews.TryGetValue(instanceId, out var existing) && existing != null)
                return existing;

            var image = _sharedInventoryData.Pool.Get();
#if UNITY_EDITOR
            image.name = $"Item_{instanceId}";
#endif
            image.transform.SetParent(transform, false);
            image.gameObject.SetActive(true);
            image.color = Color.white;

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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnEndDrag(eventData);
        }
    }
}
