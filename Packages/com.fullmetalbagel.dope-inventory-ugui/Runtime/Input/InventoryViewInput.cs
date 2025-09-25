#if UNITY_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

namespace DopeGrid.Inventory
{
    [RequireComponent(typeof(InventoryView))]
    public class InventoryViewInput : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _rotateAction = null!;
        [SerializeField] private InputActionReference _scrollAction = null!;

        private InventoryView _inventoryView = null!;

        private void Awake()
        {
            _inventoryView = GetComponent<InventoryView>();
        }

        private void OnEnable()
        {
            if (_rotateAction != null)
            {
                _rotateAction.action.Enable();
            }

            if (_scrollAction != null)
            {
                _scrollAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (_rotateAction != null)
            {
                _rotateAction.action.Disable();
            }

            if (_scrollAction != null)
            {
                _scrollAction.action.Disable();
            }
        }

        private void Update()
        {
            HandleRotationInput();
        }

        private void HandleRotationInput()
        {
            var currentRotation = _inventoryView.GetDraggingItemRotation();
            RotationDegree newRotation = currentRotation;

            // Prioritize rotate action, then scroll action. Only one rotation per frame.
            if (_rotateAction != null && _rotateAction.action.WasPressedThisFrame())
            {
                newRotation = currentRotation.GetNextClockwiseRotation();
            }
            else if (_scrollAction != null)
            {
                var scrollValue = _scrollAction.action.ReadValue<Vector2>();
                if (scrollValue.y > 0.1f) // Scroll up - clockwise
                {
                    newRotation = currentRotation.GetNextClockwiseRotation();
                }
                else if (scrollValue.y < -0.1f) // Scroll down - counter-clockwise
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

#endif
