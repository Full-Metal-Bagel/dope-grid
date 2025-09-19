# Dope Grid

A high-performance, thread-safe grid system

## Features

- **High Performance**: Leverages Unity's Burst compiler and Job System for maximum performance
- **Thread-Safe**: Immutable grid shapes with concurrent caching for safe parallel operations
- **Memory Efficient**: Bit-packed storage with optimized native collections
- **Flexible Transformations**: Support for rotation, flipping, and trimming operations
- **Unity Native**: Seamlessly integrates with native collections

## Core Components

### GridShape
Mutable 2D grid structure representing occupied/empty cells. Optimized for Unity's native collections and Burst compilation.

```csharp
using DopeGrid.Native;
using Unity.Collections;

// Create a 5x5 grid
var shape = new GridShape(5, 5, Allocator.Temp);

// Set cells using indexer syntax
shape[2, 2] = true; // Set center cell
shape[1, 1] = true; // Set another cell

// Perform transformations
var rotated = shape.Rotate(RotationDegree.Clockwise90);
var flipped = shape.Flip(FlipAxis.Horizontal);
var trimmed = shape.Trim(); // Remove empty borders

// Don't forget to dispose
shape.Dispose();
rotated.Dispose();
flipped.Dispose();
trimmed.Dispose();
```

### ImmutableGridShape
Thread-safe, cached immutable version of GridShape. Ensures identical shapes share the same ID across threads.

```csharp
// Create immutable shape from mutable one
var shape = new GridShape(3, 3, Allocator.Temp);
shape[1, 1] = true;

var immutable = ImmutableGridShape.GetOrCreateImmutable(shape);

// Transform operations return new immutable instances
var rotated = immutable.Rotate(RotationDegree.Clockwise90);
var flipped = immutable.Flip(FlipAxis.Vertical);

// Shape IDs are consistent across threads
int shapeId = immutable.ShapeId;

shape.Dispose();
```

### GridContainer
Container for managing multiple grid items with placement, rotation, and collision detection.

```csharp
using DopeGrid.Native;
using Unity.Collections;

// Create a 10x10 container
var container = new GridContainer(10, 10, Allocator.Temp);

// Create an item shape
var itemShape = new GridShape(2, 2, Allocator.Temp);
itemShape.SetAll(true);

// Try to place item at position (3, 3)
if (container.TryAddItem(3, 3, itemShape, out int itemId))
{
    // Item placed successfully

    // Check if position is occupied
    bool occupied = container.IsOccupied(3, 3);

    // Remove item
    container.RemoveItem(itemId);
}

container.Dispose();
itemShape.Dispose();
```

### SpanBitArray / ReadOnlySpanBitArray
Low-level bit manipulation structures for efficient storage of grid cell states.

```csharp
using DopeGrid.Core;

// Create bit array with 64 bits
var bitArray = new SpanBitArray(stackalloc ulong[1]);

// Set and get bits
bitArray[5] = true;
bool isSet = bitArray[5];

// Count set bits
int count = bitArray.CountSetBits();

// Create read-only view
var readOnly = new ReadOnlySpanBitArray(bitArray.Data);
```

## Job System Integration

The library is designed for efficient parallel processing with Unity Jobs:

```csharp
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct GridProcessingJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<ImmutableGridShape> shapes;
    public NativeArray<int> results;

    public void Execute(int index)
    {
        var shape = shapes[index];
        // Process shape - immutable shapes are thread-safe
        results[index] = shape.Width * shape.Height;
    }
}
```

## Performance Considerations

- **Always Dispose**: Native collections must be properly disposed to avoid memory leaks
- **Use Appropriate Allocators**:
  - `Allocator.Temp` for short-lived data (1 frame)
  - `Allocator.TempJob` for job data (4 frames)
  - `Allocator.Persistent` for long-lived data
- **Trim Before Caching**: Always trim shapes before creating immutable versions for optimal deduplication
- **Batch Operations**: Use job system for processing multiple shapes in parallel

## Testing

Tests are included in `Assets/Tests/` and can be run through Unity's Test Runner:

1. Open Unity Editor
2. Navigate to Window > General > Test Runner
3. Run all tests or filter by namespace

Test coverage includes:
- Core bit array operations
- Grid shape transformations
- Immutable shape caching and thread safety
- Concurrent operations via Job System
- Burst-compiled job tests

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please ensure:
- All tests pass
- Code follows existing conventions
- Native collections are properly disposed
- Thread safety is maintained for immutable types

## Support

For issues, questions, or feature requests, please visit the [GitHub repository](https://github.com/Full-Metal-Bagel/dope-grid).
