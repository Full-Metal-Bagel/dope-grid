using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryView : UIBehaviour
    {
        [SerializeField] private Vector2 _cellSize = new(64f, 64f);

        private Inventory _inventory;
        private IReadOnlyDictionary<Guid, UIItemDefinition> _definitions = null!;

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

        public void Initialize(Inventory inventory, IReadOnlyDictionary<Guid, UIItemDefinition> definitions)
        {
            _inventory = inventory;
            _definitions = definitions;

            Debug.Assert(_inventory.IsCreated, this);
            Rect.sizeDelta = new Vector2(_inventory.Width * _cellSize.x, _inventory.Height * _cellSize.y);
        }

        public void Update()
        {
            if (!_inventory.IsCreated) return;
            SyncViews();
        }

        private void SyncViews()
        {
            var seen = new HashSet<int>();

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

                // Compute rect by shape and position using top-left origin
                var size = new Vector2(item.Shape.Width * _cellSize.x, item.Shape.Height * _cellSize.y);
                var pos = item.Position; // int2, top-left origin in model
                var anchoredPos = GridToAnchoredPosition(pos);

                var rt = (RectTransform)image.transform;
                EnsureTopLeftAnchors(rt);
                rt.sizeDelta = size;
                var angleZ = GetZRotation(item.Rotation);
                var offset = GetRotationOffset(size, item.Rotation);
                rt.anchoredPosition = anchoredPos + offset;
                rt.localEulerAngles = new Vector3(0f, 0f, angleZ);
            }

            // Remove any views not present anymore
            var toRemove = new List<int>();
            foreach (var kv in _itemViews)
            {
                if (!seen.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var id in toRemove)
            {
                var img = _itemViews[id];
                if (img != null)
                    Destroy(img.gameObject);
                _itemViews.Remove(id);
            }
        }

        private Image GetOrCreateItemView(int instanceId)
        {
            if (_itemViews.TryGetValue(instanceId, out var existing) && existing != null)
                return existing;

            // TODO: pool
            var go = new GameObject($"Item_{instanceId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);

            var rt = (RectTransform)go.transform;
            EnsureTopLeftAnchors(rt);

            var img = go.GetComponent<Image>();
            _itemViews[instanceId] = img;
            return img;
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
