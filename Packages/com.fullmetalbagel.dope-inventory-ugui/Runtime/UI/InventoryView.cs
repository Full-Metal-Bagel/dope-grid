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
    public class InventoryView : UIBehaviour
    {
        [SerializeField] private Vector2 _cellSize = new(64f, 64f);

        private Inventory _inventory;
        private IReadOnlyDictionary<Guid, UIItemDefinition> _definitions = null!;
        private IInventoryItemViewPool _pool = null!;

        private readonly Dictionary<int, Image> _itemViews = new(); // instanceId -> Image
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
    }
}
