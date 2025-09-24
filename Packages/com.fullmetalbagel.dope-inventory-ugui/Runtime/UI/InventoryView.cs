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
    public partial class InventoryView : UIBehaviour
    {
        [SerializeField] private Vector2 _cellSize = new(64f, 64f);
        [SerializeField] private Color _placeableColor = new(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color _blockedColor = new(1f, 0f, 0f, 0.35f);

        private Inventory _inventory;
        private IReadOnlyDictionary<Guid, UIItemDefinition> _definitions = null!;
        private IInventoryItemViewPool _pool = null!;

        private readonly Dictionary<int/*item instance*/, Image> _itemViews = new();
        private readonly Dictionary<int/*item instance*/, Image> _draggingViews = new();
        private DragController _dragController = null!;

        private RectTransform Rect => (RectTransform)transform;
        private List<DraggingItem> _draggingItems = null!;

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

        public void Initialize(Inventory inventory, IReadOnlyDictionary<Guid, UIItemDefinition> definitions, List<DraggingItem> draggingItems, IInventoryItemViewPool? pool = null)
        {
            _inventory = inventory;
            _definitions = definitions;
            _draggingItems = draggingItems;
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

        private void UpdateDragPlacementPreview(HashSet<int> seen, List<int> toRemove, Inventory.ReadOnly inventory)
        {
            seen.Clear();
            toRemove.Clear();

            for (int i = 0; i < _draggingItems.Count; i++)
            {
                var item = _draggingItems[i];
                seen.Add(item.InstanceId);

                if (!TryGetSprite(item.DefinitionId, out var sprite))
                    continue;

                var shape = item.Shape;
                var gridPos = item.GetGridPosition(this);

                // Get or create preview view
                if (!_draggingViews.TryGetValue(item.InstanceId, out var preview) || preview == null)
                {
                    preview = _pool.Get();
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

                // Mirror transform logic from SyncViews
                var rotatedSize = new Vector2(shape.Width * _cellSize.x, shape.Height * _cellSize.y);
                var preRotSize = item.Rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
                    ? new Vector2(rotatedSize.y, rotatedSize.x)
                    : rotatedSize;
                var anchoredPos = GridToAnchoredPosition(gridPos);
                var angleZ = GetZRotation(item.Rotation);
                var offset = GetRotationOffset(preRotSize, item.Rotation);

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
                if (img != null) _pool.Release(img);
                _draggingViews.Remove(key);
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
    }
}
