# Dope Grid

Dope Grid is a high-performance, memory-efficient library for creating and managing 2D grids in Unity. It's designed for scenarios where you need to work with many grids or perform frequent grid operations, such as in inventory systems, board games, or procedural generation.

## Overview

Dope Grid provides a set of tools for working with 2D grids of different types. It's built with performance in mind, using structs, `ArrayPool`, and other techniques to minimize garbage collection and maximize speed. The library is highly extensible, allowing you to create custom grid types and operations.

## Features

- **High-performance:** Designed for speed and memory efficiency.
- **Multiple Grid Types:** Includes `GridShape`, `ValueGridShape`, `FixedGridShape`, and `ImmutableGridShape` for different use cases.
- **Rich API:** Provides a wide range of operations for manipulating grids, such as rotation, flipping, and finding the best fit for a shape.
- **Extensible:** Easily create custom grid types and operations.

## Core Concepts

### `GridShape`

`GridShape` is a mutable grid of booleans. It's useful for representing the shape of an item or a container.

### `ImmutableGridShape`

`ImmutableGridShape` is an immutable version of `GridShape`. It's designed for scenarios where you need to share shapes between multiple grids without the risk of accidental modification.

### `ValueGridShape<T>`

`ValueGridShape<T>` is a generic grid that can store any unmanaged type. It's useful for storing data associated with each cell in a grid, such as item IDs or tile types.

### `FixedGridShape<TSize>`

`FixedGridShape<TSize>` is a grid with a fixed size known at compile time. It's useful for small grids where you want to avoid heap allocations.

## Usage

Here's a simple example of how to use Dope Grid to create a grid and add an item to it:

```csharp
using DopeGrid;
using UnityEngine;

public class Example : MonoBehaviour
{
    void Start()
    {
        // Create a 10x10 grid
        var grid = new GridBoard(10, 10);

        // Create a 2x2 square shape
        var itemShape = Shapes.ImmutableSquare(2);

        // Try to add the item to the grid
        if (grid.TryAddItem(itemShape))
        {
            Debug.Log("Item added successfully!");
        }
        else
        {
            Debug.Log("Failed to add item.");
        }

        // Dispose the grid when you're done with it
        grid.Dispose();
    }
}
```

## API Documentation

For a detailed overview of the API, please refer to the source code and the inline documentation.

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue on the [GitHub repository](https://github.com/Full-Metal-Bagel/dope-grid).

## License

Dope Grid is licensed under the [MIT License](LICENSE).
