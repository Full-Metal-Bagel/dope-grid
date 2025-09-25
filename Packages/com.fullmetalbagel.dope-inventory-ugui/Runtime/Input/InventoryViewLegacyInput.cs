using UnityEngine;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(InventoryView))]
    public class InventoryViewLegacyInput : MonoBehaviour
    {
        private InventoryView _inventoryView = null!;

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
            var currentRotation = _inventoryView.GetDraggingItemRotation();
            RotationDegree newRotation = currentRotation;

            // Prioritize R key, then scroll wheel. Only one rotation per frame.
            if (Input.GetKeyDown(KeyCode.R))
            {
                newRotation = currentRotation.GetNextClockwiseRotation();
            }
            else
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > 0.1f) // Add threshold to avoid micro-scrolls
                {
                    newRotation = currentRotation.GetNextClockwiseRotation();
                }
                else if (scrollDelta < -0.1f)
                {
                    newRotation = currentRotation.GetPreviousClockwiseRotation();
                }
            }

            if (newRotation != currentRotation)
            {
                _inventoryView.SetDraggingItemRotation(newRotation);
            }
        }
    }
}
