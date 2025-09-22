Dope Inventory (uGUI)

Quick-start

- Add a `Canvas` and `EventSystem` to your scene (GameObject > UI > Canvas).
- Create an empty GameObject under the Canvas, add `InventoryGridView` (Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/UI/InventoryGridView.cs).
- Adjust Width/Height/Cell Size in the inspector.
- Optionally add Initial Items by creating `InventoryItemDefinition` assets (right-click in Project window: Create > Dope Inventory > Item Definition) and assigning them in the Initial Items list.
- Duplicate the GameObject to make a second inventory. You can drag/drop items between them. Press R while dragging to rotate an item.

Notes

- Stacking is supported for stackable 1x1 items (Count merges up to Max Stack when dropped onto the same item).
- Rotation while dragging updates the footprint; placement validates against the grid using Dope Grid's collision checks.
- All grid math and occupancy is powered by `DopeGrid.Native`'s `GridContainer` and `ImmutableGridShape`.

Scripts

- Runtime/Core/InventoryItemDefinition.cs: ScriptableObject describing an item (icon, size, max stack).
- Runtime/Core/InventoryGridModel.cs: Pure model wrapper over `GridContainer` tracking placed items and stacks.
- Runtime/UI/InventoryGridView.cs: uGUI view/controller that renders items and handles placement.
- Runtime/UI/InventoryItemView.cs: Drag/drop/rotate behaviour for a single item instance.

