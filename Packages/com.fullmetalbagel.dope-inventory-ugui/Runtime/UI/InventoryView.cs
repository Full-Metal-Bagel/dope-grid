using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryView : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Color _placeableColor = new(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color _blockedColor = new(1f, 0f, 0f, 0.35f);

        private static readonly Vector2 s_defaultCellSize = new(64f, 64f);

        public IUIInventory? Inventory { get; private set; }

        private Vector2 _cellSize = s_defaultCellSize;

        private InventoryViewSyncer _viewSyncer = null!;
        private InventoryViewDragPreviewController _viewDragPreviewController = null!;
        private InventoryViewDragController _dragController = null!;

        public Vector2 CellSize => _cellSize;
        public bool IsInitialized => Inventory != null;

#if UNITY_EDITOR
        protected override void Awake()
        {
            gameObject.AddComponent<InventoryViewDebugOverlay>();
        }
#endif

        public void Initialize(IUIInventory inventory)
        {
            if (inventory.IsZeroSize())
            {
                Debug.LogError($"Inventory must be initialized with a size greater than zero. The provided inventory is {inventory.Width}x{inventory.Height}. Halting initialization.", this);
                return;
            }

            Inventory = inventory;

            var rectTransform = (RectTransform)transform;
            _cellSize = ResolveCellSize(rectTransform, Inventory.Width, Inventory.Height);
            rectTransform.sizeDelta = new Vector2(Inventory.Width * _cellSize.x, Inventory.Height * _cellSize.y);

            _viewSyncer = new InventoryViewSyncer(Inventory, transform, _cellSize);
            _viewDragPreviewController = new InventoryViewDragPreviewController(Inventory, rectTransform, _cellSize, _placeableColor, _blockedColor);
            _dragController = new InventoryViewDragController(Inventory, rectTransform, CellSize);
        }

        private static Vector2 ResolveCellSize(RectTransform rectTransform, int gridWidth, int gridHeight)
        {
            gridWidth = Mathf.Max(1, gridWidth);
            gridHeight = Mathf.Max(1, gridHeight);

            var rect = rectTransform.rect;
            var width = rect.width;
            var height = rect.height;

            if (width <= 0f) width = rectTransform.sizeDelta.x;
            if (height <= 0f) height = rectTransform.sizeDelta.y;

            if (width <= 0f) width = gridWidth * s_defaultCellSize.x;
            if (height <= 0f) height = gridHeight * s_defaultCellSize.y;

            width = Mathf.Abs(width);
            height = Mathf.Abs(height);

            return new Vector2(width / gridWidth, height / gridHeight);
        }

        public void SetDraggingItemRotation(RotationDegree rotation)
        {
            _dragController.SetDraggingItemRotation(rotation);
        }

        public RotationDegree GetDraggingItemRotation()
        {
            return _dragController.GetDraggingItemRotation();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            _viewSyncer.SyncViews();
            _viewDragPreviewController.UpdateDragPlacementPreview();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _viewSyncer?.Dispose();
            _viewDragPreviewController?.Dispose();
            _dragController?.Dispose();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnBeginDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!IsInitialized) return;
            _dragController.OnEndDrag(eventData);
        }
    }
}
