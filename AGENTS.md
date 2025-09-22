# Repository Guidelines

## Project Structure & Module Organization
- Core grid runtime: `Packages/com.fullmetalbagel.dope-grid/Runtime/{Core,Native,Standard}`.
- Inventory UI + glue: `Packages/com.fullmetalbagel.dope-inventory-ugui/Runtime/{Core,UI}`.
- Unity content: `Assets/Scenes` (editor-facing), tests in `Assets/Tests/{Core,Native}`.
- Standalone .NET: `dotnet/` (use `DopeGrid.sln` + `global.json`).
- Generated folders (`Library/`, `Temp/`, `Logs/`) are disposable.

## Build, Test, and Development Commands
- `dotnet restore dotnet/DopeGrid.sln` — restore against .NET 8 SDK.
- `dotnet test dotnet/DopeGrid.sln --framework net8.0` — fast NUnit tests without Unity.
- `open -a Unity Hub .` (or Unity 2022.3.62f1) — open the editor for play-mode checks.
- `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -quit` — CI EditMode coverage.

## Coding Style & Naming Conventions
- Follow `.editorconfig`: 4 spaces, LF, UTF-8; file-scoped namespaces; prefer expression-bodied members.
- Fields: private/internal `_camelCase`, static `s_`; constants PascalCase.
- Nullable reference types enabled; warnings are errors. Add explicit null checks and resolve analyzers before pushing.

## Testing Guidelines
- Framework: NUnit; name files `*Tests.cs` and place under `Assets/Tests/Core` (algorithms) or `Assets/Tests/Native` (Unity containers).
- Dispose all native collections; mirror allocator usage (`Allocator.Temp`, `Allocator.TempJob`, `Allocator.Persistent`).
- Cover changes in both the .NET suite and Unity Test Runner.

## Commit & Pull Request Guidelines
- Subject: concise, imperative (e.g., "Add immutable grid cache"), optional `(#123)` reference.
- Body: note performance and allocator implications; list test results (`dotnet test --framework net8.0`, Unity runner screenshot/GIF for UI).
- Link issues and include short clips for UX changes.

## Unity & Native Collections Tips
- Keep allocators scoped; prefer `Allocator.TempJob` for jobs and trim shapes before caching to improve dedup rates.
- For Burst/Jobs debugging, enable `ENABLE_UNITY_COLLECTIONS_CHECKS` and gate editor-only utilities with `#if UNITY_EDITOR` under `Packages/com.fullmetalbagel.dope-grid/Runtime/Native`.
