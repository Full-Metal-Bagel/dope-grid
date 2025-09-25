using UnityEngine;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(InventoryView))]
    public class InventoryViewLegacyInput : MonoBehaviour
    {
        private InventoryView _inventoryView = null!;
        private bool _rKeyWasPressed;

        private void Awake()
        {
            _inventoryView = GetComponent<InventoryView>();
        }

        private void Update()
        {
            HandleRotationInput();
        }

        private void HandleRotationInput()
        {
            // Handle R key with state tracking to avoid multiple rotations per press
            var rKeyPressed = Input.GetKey(KeyCode.R);
            if (rKeyPressed && !_rKeyWasPressed)
            {
                var currentRotation = _inventoryView.GetDraggingItemRotation();
                var newRotation = currentRotation.GetNextClockwiseRotation();
                _inventoryView.SetDraggingItemRotation(newRotation);
            }
            _rKeyWasPressed = rKeyPressed;

            // Handle scroll wheel input
            if (Input.mouseScrollDelta.y > 0.1f) // Add threshold to avoid micro-scrolls
            {
                var currentRotation = _inventoryView.GetDraggingItemRotation();
                var newRotation = currentRotation.GetNextClockwiseRotation();
                _inventoryView.SetDraggingItemRotation(newRotation);
            }
            else if (Input.mouseScrollDelta.y < -0.1f)
            {
                var currentRotation = _inventoryView.GetDraggingItemRotation();
                var newRotation = currentRotation.GetPreviousClockwiseRotation();
                _inventoryView.SetDraggingItemRotation(newRotation);
            }
        }
    }
}
