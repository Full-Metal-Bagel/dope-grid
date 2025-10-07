# Repository Guidelines

## Project Structure & Module Organization
- Unity packages live under `Packages/`:
  - `com.fullmetalbagel.dope-grid/Runtime` — core grid/shape/board logic
  - `com.fullmetalbagel.dope-inventory-ugui/Runtime` — uGUI inventory and input
- Demo scene: `Assets/Scenes/Inventory(uGUI).unity`
- Tests:
  - Unity tests: `Assets/Tests/**`
  - .NET mirror for faster CI: `dotnet/DopeGrid.Tests` (links Unity test sources)

## Build, Test, and Development Commands
- .NET restore/build/test (CI path):
  - `dotnet restore dotnet/DopeGrid.Tests/DopeGrid.Tests.csproj`
  - `dotnet build dotnet/DopeGrid.Tests/DopeGrid.Tests.csproj -c Release --no-restore`
  - `dotnet test dotnet/DopeGrid.Tests/DopeGrid.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage"`
- Solution-wide: `dotnet test dotnet/DopeGrid.sln`
- Unity tests: Editor → Window → General → Test Runner → Run All
- Run demo: open `Assets/Scenes/Inventory(uGUI).unity`

## Coding Style & Naming Conventions
- 4-space indent, LF, UTF-8; final newline; trim trailing whitespace (`.editorconfig`).
- C# preferences: file-scoped namespaces; `using` outside namespace; braces required.
- Fields: private/internal `_camelCase`; static `s_camelCase`; constants `PascalCase`.
- Prefer keyword types (e.g., `int`), `var` only when obvious.
- Analyzers on and warnings-as-errors; keep code clean and nullable-enabled.

## Testing Guidelines
- Framework: NUnit (`[TestFixture]`, `[Test]`).
- Naming: place tests alongside feature area; filenames end with `*Tests.cs`.
- Coverage: collected via `--collect:"XPlat Code Coverage"` (uploaded in CI).
- Keep tests deterministic; avoid Unity-specific APIs in `.NET` tests.

## Commit & Pull Request Guidelines
- Use Conventional Commits: `feat:`, `fix:`, `refactor:`, etc. Example: `feat(grid): add IndexedGridBoard (#20)`.
- Keep messages imperative and scoped; reference issues/PRs with `#123`.
- PRs should include: clear description, screenshots for UI, test plan, and linked issues.
- CI must pass (`.github/workflows/dotnet-test.yml`). For releases, NuGet publish is automated on tags.

## Security & Configuration Tips
- Target Unity 2022.3+; avoid unsafe operations in UI unless justified.
- Core library permits `unsafe` for performance—validate bounds and pooling behavior.

## Agent-Specific Notes
- Follow `.editorconfig`; do not restructure packages arbitrarily.
- If API changes, update tests and `README.md` paths accordingly.

