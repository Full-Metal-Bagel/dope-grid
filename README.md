# Dope Inventory

A high-performance Unity 2D grid inventory system with drag-and-drop support, item rotation, and stacking.

## Features

- **2D Grid-Based Inventory:** Efficient grid system using span-based bit arrays and unsafe pointers
- **Drag & Drop:** Intuitive item dragging between multiple inventories with visual feedback
- **Item Rotation:** Press R while dragging to rotate items (90°/180°/270°)
- **Collision Detection:** Real-time placement validation using shape overlap checks
- **Object Pooling:** Minimized GC allocations through pooling of shapes and UI elements
- **uGUI Integration:** Ready-to-use Unity UI components

## Quick Start

### Requirements

- Unity 2022.3 or later

### Demo Scene

Open `Assets/Scenes/Inventory(uGUI).unity` to see a working example.

## Architecture

### Packages

This repository contains two Unity packages:

#### DopeGrid (`com.fullmetalbagel.dope-grid`)
Low-level grid shape library providing:
- `IGridShape<T>`: Interface for mutable grid shapes with typed cell values
- `ImmutableGridShape`: Memory-efficient readonly grid shapes using bit arrays
- `IndexedGridBoard`: Grid container managing multiple items with collision detection
- Grid transformations (rotation, flipping, trimming)

#### DopeInventory (`com.fullmetalbagel.dope-inventory-ugui`)
uGUI-based inventory system built on DopeGrid:
- `IGameInventory`/`IUIInventory`: Inventory model interfaces with GUID-based item tracking
- `InventoryView`: Main MonoBehaviour for inventory UI
- `InventoryViewDragController`: Drag-and-drop interaction handling
- `InventoryItemViewPool`: Object pooling for UI elements

### Key Components

**InventoryView** (`Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/UI/InventoryView.cs`)
Main component orchestrating inventory UI, handling user input, and synchronizing model-view state.

**IGameInventory** (`Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/Model/IGameInventory.cs`)
Core inventory model interface managing item placement, movement, and removal with collision detection.

**IndexedGridBoard** (`Packages/com.fullmetalbagel.dope-grid/Runtime/IndexedGridBoard.cs`)
Grid container storing item IDs per cell, providing efficient spatial queries and collision checking.

## Development

### Running Tests

**Unity Tests:**
```bash
# Open Test Runner in Unity Editor
# Window > General > Test Runner
# Click "Run All" for EditMode or PlayMode tests
```

**.NET Tests (faster, no Unity dependency):**
```bash
cd dotnet
dotnet test DopeGrid.sln

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific test
dotnet test --filter "FullyQualifiedName~TestName"
```

### Code Style

- C# 10+ with nullable reference types enabled
- Unsafe code and pointers for performance-critical grid operations
- Pure functions marked with `[Pure, MustUseReturnValue]`
- Strict analyzer rules with `TreatWarningsAsErrors`

### Project Structure

```
Packages/
├── com.fullmetalbagel.dope-grid/          # Low-level grid library
│   ├── Runtime/                            # Grid shapes, boards, transformations
│   └── Editor/                             # Editor utilities
├── com.fullmetalbagel.dope-inventory-ugui/ # Inventory UI system
│   ├── Runtime/
│   │   ├── Model/                          # Inventory model layer
│   │   ├── UI/                             # UI components
│   │   └── Input/                          # Input abstraction
Assets/
├── Scenes/                                 # Demo scenes
└── Tests/                                  # Unity test cases
dotnet/
└── DopeGrid/                               # Standalone .NET project (same source)
```

## License

MIT License - see [LICENSE](LICENSE) for details.
