# Conventional Commits Versioning Implementation

## Overview
This document describes the implementation of conventional commits-based versioning to solve the PowerShell Gallery publication issue where prerelease versions like `1.4.0-ci3` cannot be published when `1.4.0` already exists.

## Problem Statement
The previous versioning strategy always used the latest git tag as the base version for CI builds (e.g., `1.4.0-ci3`). This caused failures when publishing to PowerShell Gallery because:
- Prerelease versions are not considered greater than stable versions
- `1.4.0-ci3` is not greater than `1.4.0` in semantic versioning
- PowerShell Gallery rejects the publication with error: "version must exceed the current version"

## Solution
Implement automatic version determination based on conventional commits in PR descriptions:
1. Parse PR description for conventional commit messages
2. Determine the appropriate version bump (major, minor, or patch)
3. Increment the base version accordingly
4. Create CI builds with the incremented version (e.g., `1.4.1-ci...` or `1.5.0-ci...`)

## Implementation Details

### 1. Version Calculation Script (`scripts/Get-NextVersion.ps1`)
A PowerShell script that:
- Accepts a base version and array of commit messages
- Parses conventional commit syntax
- Handles list markers (`-`, `*`, `+`) in PR descriptions
- Determines the highest version bump needed:
  - **Major**: `feat!:`, `fix!:`, or `BREAKING CHANGE:` → 1.4.0 → 2.0.0
  - **Minor**: `feat:` or `feat(scope):` → 1.4.0 → 1.5.0
  - **Patch**: `fix:`, `docs:`, `chore:`, etc. → 1.4.0 → 1.4.1
- Defaults to patch bump if no conventional commits found

**Example usage:**
```powershell
./scripts/Get-NextVersion.ps1 -BaseVersion "1.4.0" -CommitMessages @(
    "feat: add batch operations",
    "fix: resolve timeout"
)
# Returns: 1.5.0 (minor bump due to feat:)
```

### 2. Workflow Changes (`.github/workflows/publish.yml`)
Updated the Build step to:
- Detect PR events using `$env:GITHUB_EVENT_NAME -eq "pull_request"`
- Read PR description from `$env:GITHUB_EVENT_PATH`
- Parse PR body for conventional commits
- Call `Get-NextVersion.ps1` to determine next version
- Apply the incremented version to CI builds
- Maintain backward compatibility for:
  - Tag-based releases (use tag version)
  - Main branch pushes (analyze commits since last tag)

**Version calculation flow:**
```
1. Get latest tag (e.g., v1.4.0)
2. Parse PR description for conventional commits
3. Determine bump type (major/minor/patch)
4. Calculate next version (e.g., 1.5.0)
5. Add prerelease suffix (e.g., 1.5.0-ci20241102123)
6. Update module manifest
```

### 3. PR Template (`.github/pull_request_template.md`)
Created a template that:
- Includes a dedicated "Conventional Commits" section
- Provides clear examples of each commit type
- Explains version bump rules
- Ensures consistent formatting for parsing

**Template structure:**
```markdown
## Conventional Commits
<!-- This section is REQUIRED for automatic versioning -->

- feat: add batch operations
- fix: resolve connection timeout
- docs: update documentation
```

### 4. Documentation Updates

#### CONTRIBUTING.md
Comprehensive guide including:
- Conventional commit format rules
- Version bump examples
- Breaking change syntax
- Multiple commit scenarios
- PR submission process
- Build and test instructions

#### README.md
Added contributing section with:
- Quick reference to conventional commits
- Links to detailed guidelines
- Version bump examples

#### .github/copilot-instructions.md
Updated with:
- Versioning strategy documentation
- Conventional commit rules
- Version calculation script details
- PR template usage
- Examples of all commit types

#### scripts/README.md
Added version management section documenting:
- Get-NextVersion.ps1 usage
- Test-VersionLogic.ps1 for validation
- Test-WorkflowSimulation.ps1 for workflow testing

### 5. Testing

#### Test-VersionLogic.ps1
Unit tests covering:
- feat → minor bump (1.4.0 → 1.5.0) ✓
- fix → patch bump (1.4.0 → 1.4.1) ✓
- feat! → major bump (1.4.0 → 2.0.0) ✓
- Multiple commits with different types ✓
- No conventional commits (defaults to patch) ✓

#### Test-WorkflowSimulation.ps1
Integration test simulating:
- GitHub Actions PR event
- PR body with conventional commits
- Version calculation workflow
- CI version generation

All tests pass successfully.

## Usage Examples

### Example 1: Feature Addition (Minor Bump)
**PR Description:**
```markdown
## Conventional Commits
- feat: add batch delete operation
- docs: update batch operation examples
```

**Result:** 1.4.0 → 1.5.0-ci20241102123

### Example 2: Bug Fix (Patch Bump)
**PR Description:**
```markdown
## Conventional Commits
- fix: resolve connection timeout issue
- test: add timeout tests
```

**Result:** 1.4.0 → 1.4.1-ci20241102123

### Example 3: Breaking Change (Major Bump)
**PR Description:**
```markdown
## Conventional Commits
- feat!: remove deprecated cmdlet parameters

BREAKING CHANGE: Removed -LegacyBehavior parameter
```

**Result:** 1.4.0 → 2.0.0-ci20241102123

### Example 4: Multiple Changes (Highest Wins)
**PR Description:**
```markdown
## Conventional Commits
- fix: bug fix
- feat: new feature
- docs: documentation
```

**Result:** 1.4.0 → 1.5.0-ci20241102123 (minor wins over patch)

## Benefits

1. **Solves PowerShell Gallery Publication Issue**
   - CI versions now properly increment the base version
   - `1.5.0-ci...` is greater than `1.4.0` and can be published

2. **Automated and Consistent**
   - No manual version management needed
   - Consistent versioning across all PRs
   - Follows semantic versioning principles

3. **Clear Communication**
   - PR descriptions clearly indicate the type of changes
   - Version bumps are predictable and transparent
   - Breaking changes are explicitly marked

4. **Developer Friendly**
   - Simple format to follow
   - Clear documentation and examples
   - PR template guides contributors
   - Validation through tests

5. **Backward Compatible**
   - Existing tag-based releases still work
   - Main branch commits analyzed for version bumps
   - Falls back to patch bump if no conventional commits found

## Migration Notes

### For New PRs
- Use the PR template (auto-populated when creating PR)
- Fill in the "Conventional Commits" section
- Follow the format: `<type>(<scope>): <description>`
- The workflow will automatically determine the version

### For Existing Workflow
- Tag-based releases: No changes needed, continue using git tags
- Main branch pushes: Commits since last tag are analyzed
- PR builds: Uses PR description for version calculation

## Validation

All changes have been validated:
- ✅ Get-NextVersion.ps1 unit tests pass (5/5 scenarios)
- ✅ Workflow simulation test passes
- ✅ dotnet build completes successfully
- ✅ YAML syntax is valid
- ✅ Documentation is comprehensive

## Next Steps

1. Merge this PR to enable conventional commits versioning
2. Update existing open PRs to include conventional commits in description
3. Monitor CI builds to ensure version calculation works correctly
4. Consider adding a GitHub Action to validate PR descriptions contain conventional commits

## References

- [Conventional Commits Specification](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [PowerShell Gallery Publishing](https://learn.microsoft.com/en-us/powershell/gallery/how-to/publishing-packages/publishing-a-package)
