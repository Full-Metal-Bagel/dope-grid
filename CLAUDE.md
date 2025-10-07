# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Dope Inventory** is a Unity-based 2D grid inventory system with drag-and-drop support, item rotation, and stacking. The project consists of two main Unity packages:

1. **DopeGrid** (`com.fullmetalbagel.dope-grid`): Low-level grid shape library providing efficient 2D grid operations using span-based bit arrays and unsafe pointers
2. **DopeInventory** (`com.fullmetalbagel.dope-inventory-ugui`): uGUI-based inventory system built on top of DopeGrid

Additionally, there's a standalone .NET project under `dotnet/DopeGrid/` that shares the same source code with the Unity package for cross-platform testing.

## Development Commands

### Unity Testing
```bash
# Run tests in Unity Editor (from Unity Editor's Test Runner window)
# Window > General > Test Runner
# Click "Run All" for EditMode or PlayMode tests
```

### .NET Testing
```bash
# Run standalone .NET tests (faster, no Unity dependency)
cd dotnet
dotnet test DopeGrid.sln

# Run tests with coverage
dotnet test DopeGrid.sln --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"
```

### Unity Project
```bash
# Open in Unity (version 2022.3+)
# Open the project root directory in Unity Hub

# Build Unity project
# File > Build Settings > Build
```

## Architecture

### DopeGrid Core Architecture

**Grid Shape Abstraction:**
- `IReadOnlyGridShape`: Base interface for immutable grid shapes with width/height and occupancy checking
- `IGridShape<T>`: Mutable grid shapes storing typed values per cell
- `ImmutableGridShape`: Readonly struct-based shape using span bit arrays for memory efficiency
- `GridShape`: Mutable grid shape with pooling support
- `SpanBitArray`/`ReadOnlySpanBitArray`: Efficient bit-level storage for grid cell occupancy

**Grid Board (Container):**
- `IGridBoard<T>`: Interface for grid containers that manage multiple items
- `IndexedGridBoard`: Concrete implementation storing item IDs per cell (int-based)
- `BoardItemData`: Value type containing item position, shape reference, and ID
- Board handles collision detection when adding/moving items using shape overlap checks

**Transformations:**
- `RotationDegree`: Enum for 90/180/270 degree rotations
- `FlipAxis`: Horizontal/Vertical flipping
- All transformations are performed through extension methods in `GridShapeExtension`

### DopeInventory Architecture

**Model Layer:**
- `IGameInventory`: Core inventory interface extending `IReadOnlyIndexedGridBoard` with GUID-based item tracking
- `IUIInventory`: UI-specific inventory interface adding sprite and image pooling
- `DefaultGameInventory`/`DefaultUIInventory`: Default implementations with rotation state management

**View Layer:**
- `InventoryView`: Main MonoBehaviour orchestrating the inventory UI
- `InventoryViewSyncer`: Synchronizes model changes to view (item placement/removal)
- `InventoryViewDragController`: Handles drag-and-drop interactions
- `InventoryViewDragPreviewController`: Manages visual feedback during dragging
- `DraggingItem`: Value type representing an item being dragged

**Input Abstraction:**
- `InventoryViewInput`: Interface for input handling (mouse/touch)
- `InventoryViewLegacyInput`: Legacy Unity Input implementation

**Object Pooling:**
- `DefaultInventoryItemViewPool`: Pools UI Image components for items
- `SharedData`: Holds references to pooled/shared resources across inventories

### Key Design Patterns

1. **Composition Over Inheritance:** Grid shapes use interfaces and extension methods rather than deep hierarchies
2. **Struct-Based Immutability:** `ImmutableGridShape` and `BoardItemData` are readonly structs to minimize allocations
3. **Object Pooling:** Both grid shapes and UI images use pooling to reduce GC pressure
4. **Interface Segregation:** Clean separation between readonly (`IReadOnly*`) and mutable (`I*`) interfaces

### Critical Files

- **DopeGrid Core:**
  - `Packages/com.fullmetalbagel.dope-grid/Runtime/IGridShape.cs` - Shape interface and transformations
  - `Packages/com.fullmetalbagel.dope-grid/Runtime/IGridBoard.cs` - Board container interface
  - `Packages/com.fullmetalbagel.dope-grid/Runtime/IndexedGridBoard.cs` - Board implementation
  - `Packages/com.fullmetalbagel.dope-grid/Runtime/ImmutableGridShape.cs` - Immutable shape implementation

- **DopeInventory:**
  - `Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/Model/IGameInventory.cs` - Core inventory model
  - `Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/Model/IUIInventory.cs` - UI inventory interface
  - `Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/UI/InventoryView.cs` - Main view component

### Code Style Notes

- Uses C# 10+ features (`record`, pattern matching, range operators)
- Heavy use of `unsafe` code and pointers for performance-critical grid operations
- Pure functions marked with `[Pure, MustUseReturnValue]` (JetBrains.Annotations)
- Strict null safety (`#nullable enable`)
- Project uses `.editorconfig` with strict analyzer rules (`TreatWarningsAsErrors`)

### Dual-Build Strategy

The DopeGrid library maintains two build configurations:
1. **Unity package** (`Packages/com.fullmetalbagel.dope-grid/`) - Uses Unity's test framework, includes Unity-specific features
2. **.NET project** (`dotnet/DopeGrid/`) - References the same source files via wildcard includes, uses NUnit, faster test iteration

When modifying core grid code, ensure changes work in both Unity and standalone .NET contexts.
