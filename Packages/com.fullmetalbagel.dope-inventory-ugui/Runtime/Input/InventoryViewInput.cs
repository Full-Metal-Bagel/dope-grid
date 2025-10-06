// #if UNITY_INPUT_SYSTEM
//
// using UnityEngine;
// using UnityEngine.InputSystem;
//
// namespace DopeGrid.Inventory
// {
//     [RequireComponent(typeof(InventoryView))]
//     public class InventoryViewInput : MonoBehaviour
//     {
//         [Header("Input Actions")]
//         [SerializeField] private InputActionReference _rotateAction = null!;
//         [SerializeField] private InputActionReference _scrollAction = null!;
//
//         private InventoryView _inventoryView = null!;
//         private bool _rotateWasPressed;
//
//         private void Awake()
//         {
//             _inventoryView = GetComponent<InventoryView>();
//         }
//
//         private void OnEnable()
//         {
//             if (_rotateAction != null)
//             {
//                 _rotateAction.action.Enable();
//             }
//
//             if (_scrollAction != null)
//             {
//                 _scrollAction.action.Enable();
//             }
//         }
//
//         private void OnDisable()
//         {
//             if (_rotateAction != null)
//             {
//                 _rotateAction.action.Disable();
//             }
//
//             if (_scrollAction != null)
//             {
//                 _scrollAction.action.Disable();
//             }
//         }
//
//         private void Update()
//         {
//             HandleRotationInput();
//         }
//
//         private void HandleRotationInput()
//         {
//             // Handle rotate action with state tracking to avoid multiple rotations per press
//             if (_rotateAction != null)
//             {
//                 var rotatePressed = _rotateAction.action.IsPressed();
//                 if (rotatePressed && !_rotateWasPressed)
//                 {
//                     var currentRotation = _inventoryView.GetDraggingItemRotation();
//                     var newRotation = currentRotation.GetNextClockwiseRotation();
//                     _inventoryView.SetDraggingItemRotation(newRotation);
//                 }
//                 _rotateWasPressed = rotatePressed;
//             }
//
//             // Handle scroll action
//             if (_scrollAction != null)
//             {
//                 var scrollValue = _scrollAction.action.ReadValue<Vector2>();
//
//                 if (scrollValue.y > 0.1f) // Scroll up - clockwise
//                 {
//                     var currentRotation = _inventoryView.GetDraggingItemRotation();
//                     var newRotation = currentRotation.GetNextClockwiseRotation();
//                     _inventoryView.SetDraggingItemRotation(newRotation);
//                 }
//                 else if (scrollValue.y < -0.1f) // Scroll down - counter-clockwise
//                 {
//                     var currentRotation = _inventoryView.GetDraggingItemRotation();
//                     var newRotation = currentRotation.GetPreviousClockwiseRotation();
//                     _inventoryView.SetDraggingItemRotation(newRotation);
//                 }
//             }
//         }
//     }
// }
//
// #endif
