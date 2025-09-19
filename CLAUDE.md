# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity project for the Dope Grid inventory system, a high-performance grid-based data structure library with native Unity support, Burst compilation, and Job System integration.

## Development Commands

### Unity Testing
Tests are run through Unity's Test Runner. Since this is a Unity project:
- Open the project in Unity Editor (2022.3 or later)
- Open Test Runner: Window > General > Test Runner
- Run all tests or filter specific test assemblies/namespaces
- For command-line testing: Use Unity's batch mode with `-runTests` flag

### Building
This is a Unity package project. Building happens through Unity:
- Unity automatically compiles C# scripts when files change
- Package is defined in `Packages/com.fullmetalbagel.dope-grid/package.json`
- Assembly definitions control compilation boundaries (`.asmdef` files)

## Architecture

### Package Structure
The main package `com.fullmetalbagel.dope-grid` is organized into three assemblies:

1. **DopeGrid.Core** (`Runtime/Core/`)
   - Foundation types like `SpanBitArray`, `ReadOnlySpanBitArray`
   - Utility classes and enums (`FlipAxis`, `RotationDegree`)
   - No Unity dependencies, pure C# implementation

2. **DopeGrid.Native** (`Runtime/Native/`)
   - Unity-native implementations using Burst and Job System
   - Key types: `GridShape`, `ImmutableGridShape`, `GridContainer`
   - Thread-safe immutable shape caching system
   - Optimized for parallel processing with Unity Jobs

3. **DopeGrid.Standard** (`Runtime/Standard/`)
   - Standard C# implementations without Unity dependencies

### Key Concepts

**GridShape**: Mutable 2D grid structure representing occupied/empty cells. Uses native collections for high performance.

**ImmutableGridShape**: Thread-safe, cached immutable version of GridShape. Uses a static repository pattern to ensure identical shapes share the same ID across threads. Critical for concurrent operations in Unity Job System.

**SpanBitArray/ReadOnlySpanBitArray**: Low-level bit manipulation structures for efficient storage of grid cell states.

**GridContainer**: Container for managing multiple grid items with placement, rotation, and collision detection.

### Testing Architecture

Tests are in `Assets/Tests/` and cover:
- Core bit array operations
- Grid shape transformations (rotation, flip, trim)
- Immutable shape caching and thread safety
- Concurrent operations via Unity Job System
- Burst-compiled job tests

### Thread Safety

The `ImmutableGridShape` system is designed for thread-safe operations:
- Static shape repository with concurrent dictionary
- GetOrCreateImmutable() ensures shape deduplication
- All transformations return new immutable instances
- Safe for use in parallel jobs and multi-threaded contexts

## Dependencies

- Unity 2022.3+
- Unity Collections 2.1.4
- Unity Mathematics 1.2.6

## Important Notes

- Always trim shapes before creating immutable versions to ensure proper deduplication
- Use `Allocator.TempJob` for temporary native collections in jobs
- Dispose native collections properly to avoid memory leaks
- The project uses Unity's native collections and job system extensively - be familiar with their constraints