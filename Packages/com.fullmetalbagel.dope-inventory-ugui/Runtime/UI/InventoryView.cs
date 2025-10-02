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

        private Inventory _inventory;
        private SharedInventoryData _sharedInventoryData = null!;
        private Vector2 _cellSize = s_defaultCellSize;

        private InventoryViewSyncer _viewSyncer = null!;
        private InventoryViewDragPreviewController _viewDragPreviewController = null!;
        private InventoryViewDragController _dragController = null!;

        public Inventory.ReadOnly ReadOnlyInventory => _inventory;
        public Vector2 CellSize => _cellSize;
        public bool IsInitialized => _inventory.IsCreated;

        protected override void Awake()
        {
#if UNITY_EDITOR
            gameObject.AddComponent<InventoryViewDebugOverlay>();
#endif
        }

        public void Initialize(Inventory inventory, SharedInventoryData sharedInventoryData)
        {
            Debug.Assert(inventory.IsCreated, this);
            if (inventory.Width == 0 || inventory.Height == 0)
            {
                Debug.LogError($"Inventory must be initialized with a size greater than zero. The provided inventory is {_inventory.Width}x{_inventory.Height}. Halting initialization.", this);
                return;
            }

            _inventory = inventory;
            _sharedInventoryData = sharedInventoryData;

            var rectTransform = (RectTransform)transform;
            _cellSize = ResolveCellSize(rectTransform, _inventory.Width, _inventory.Height);
            rectTransform.sizeDelta = new Vector2(_inventory.Width * _cellSize.x, _inventory.Height * _cellSize.y);

            _viewSyncer = new InventoryViewSyncer(_sharedInventoryData, transform, _cellSize);
            _viewDragPreviewController = new InventoryViewDragPreviewController(_sharedInventoryData, rectTransform, _cellSize, _placeableColor, _blockedColor);
            _dragController = new InventoryViewDragController(_sharedInventoryData, rectTransform, inventory, CellSize);
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
            _dragController.SetRotation(rotation);
        }

        public RotationDegree GetDraggingItemRotation()
        {
            return _dragController.GetRotation();
        }

        private void Update()
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
