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

        private InventoryViewSyncer _viewSyncer = null!;
        private InventoryViewDragPreviewController _viewDragPreviewController = null!;
        private InventoryViewDragController _dragController = null!;

        public Inventory.ReadOnly ReadOnlyInventory => _inventory;
        internal Vector2 CellSize => _cellSize;

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
            var rectTransform = (RectTransform)transform;
            rectTransform.sizeDelta = new Vector2(_inventory.Width * _cellSize.x, _inventory.Height * _cellSize.y);

            _viewSyncer = new InventoryViewSyncer(_sharedInventoryData, transform, _cellSize);
            _viewDragPreviewController = new InventoryViewDragPreviewController(_sharedInventoryData, rectTransform, _cellSize, _placeableColor, _blockedColor);
            _dragController = new InventoryViewDragController(_sharedInventoryData, rectTransform, inventory, CellSize);
        }

        public void Update()
        {
            if (!_inventory.IsCreated) return;

            var readOnlyInventory = _inventory.AsReadOnly();
            _viewSyncer.SyncViews(readOnlyInventory);
            _viewDragPreviewController.UpdateDragPlacementPreview(_inventory);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _viewSyncer?.Dispose();
            _viewDragPreviewController?.Dispose();
            _dragController?.Dispose();
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
