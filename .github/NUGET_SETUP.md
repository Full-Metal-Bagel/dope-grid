# NuGet Publishing Setup

This guide explains how to set up and use the NuGet publishing workflow for DopeGrid.

## Prerequisites

1. **NuGet Account**: Create an account at https://www.nuget.org
2. **API Key**: Generate an API key from your NuGet account settings
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create" and give it a name (e.g., "DopeGrid GitHub Actions")
   - Set the key scope to "Push" and select package glob pattern or specific package
   - Copy the generated API key (you'll only see it once!)

## GitHub Setup

### Add NuGet API Key as Secret

1. Go to your GitHub repository: https://github.com/Full-Metal-Bagel/dope-grid
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `NUGET_API_KEY`
5. Value: Paste your NuGet API key
6. Click **Add secret**

## Publishing Methods

### Method 1: Automatic Publishing on Release (Recommended)

This is the recommended approach for production releases.

1. Create and push a git tag with version:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. Go to GitHub Releases: https://github.com/Full-Metal-Bagel/dope-grid/releases
3. Click **Draft a new release**
4. Choose the tag you just created (v1.0.0)
5. Fill in release title and description
6. Click **Publish release**
7. The workflow will automatically:
   - Extract version from tag (removes 'v' prefix)
   - Run tests
   - Build and pack the NuGet package
   - Publish to NuGet.org

### Method 2: Manual Publishing via Workflow Dispatch

Use this for testing or manual releases.

1. Go to **Actions** tab: https://github.com/Full-Metal-Bagel/dope-grid/actions
2. Select **Publish to NuGet** workflow
3. Click **Run workflow**
4. Enter version number (e.g., `1.0.0-beta`)
5. Click **Run workflow**

## Version Management

### Semantic Versioning

Follow [SemVer](https://semver.org/) for version numbers:
- **MAJOR.MINOR.PATCH** (e.g., 1.0.0)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Pre-release Versions

For beta/alpha releases, append a suffix:
- `1.0.0-alpha.1`
- `1.0.0-beta.1`
- `1.0.0-rc.1`

## Workflow Details

The workflow performs these steps:

1. **Checkout**: Clones the repository
2. **Setup .NET**: Installs .NET 8.0 SDK
3. **Extract Version**: Gets version from tag or manual input
4. **Update csproj**: Injects version into DopeGrid.csproj
5. **Restore**: Restores NuGet dependencies
6. **Build**: Builds in Release configuration
7. **Test**: Runs all tests to ensure quality
8. **Pack**: Creates .nupkg and .snupkg (symbols) files
9. **Publish**: Pushes packages to NuGet.org
10. **Upload Artifacts**: Saves packages as GitHub artifacts (for debugging)

## Package Contents

The published NuGet package includes:

- **DopeGrid.dll**: Main library (netstandard2.1)
- **DopeGrid.pdb**: Debug symbols (in separate .snupkg)
- **README.md**: Project documentation
- **Dependencies**: JetBrains.Annotations 2025.2.2

## Testing the Package Locally

Before publishing, you can test the package locally:

```bash
# Build and pack locally
cd dotnet/DopeGrid
dotnet pack --configuration Release --output ./local-nupkg

# Test in another project
cd /path/to/test/project
dotnet add package DopeGrid --source /path/to/dope-grid/dotnet/DopeGrid/local-nupkg
```

## Troubleshooting

### "Package already exists" Error

If you see `409 Conflict - The package already exists`, either:
- Increment the version number
- The workflow uses `--skip-duplicate` to ignore this error

### Tests Failing

The workflow will abort if tests fail. Fix tests before publishing:
```bash
cd dotnet
dotnet test DopeGrid.Tests/DopeGrid.Tests.csproj
```

### Version Not Updating

Ensure your tag follows the format `vX.Y.Z` or manually specify version in workflow dispatch.

## Package URL

Once published, your package will be available at:
https://www.nuget.org/packages/DopeGrid

## Updating Package Metadata

To update package description, tags, or other metadata:

1. Edit `dotnet/DopeGrid/DopeGrid.csproj`
2. Update properties in the `<!-- NuGet Package Metadata -->` section
3. Commit and push changes
4. Create a new release with incremented version

## Best Practices

1. **Always test before releasing**: Run full test suite locally
2. **Use descriptive release notes**: Help users understand what changed
3. **Follow SemVer**: Make version numbers meaningful
4. **Tag releases**: Use git tags for version tracking
5. **Keep README updated**: Package includes README.md
6. **Test major changes in pre-release**: Use `-beta` or `-alpha` suffixes
