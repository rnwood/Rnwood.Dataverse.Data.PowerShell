# Conventional Commits Versioning Implementation

## Overview
This document describes the implementation of conventional commits-based versioning to solve the PowerShell Gallery publication issue where prerelease versions like `1.4.0-ci3` cannot be published when `1.4.0` already exists.

## Problem Statement
The previous versioning strategy always used the latest git tag as the base version for CI builds (e.g., `1.4.0-ci3`). This caused failures when publishing to PowerShell Gallery because:
- Prerelease versions are not considered greater than stable versions
- `1.4.0-ci3` is not greater than `1.4.0` in semantic versioning
- PowerShell Gallery rejects the publication with error: "version must exceed the current version"

## Solution
Implement automatic version determination based on conventional commits in PR titles:
1. Parse PR title for conventional commit message
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

### 2. Release Notes Generation (`scripts/Get-ReleaseNotes.ps1`)
A PowerShell script that:
- Analyzes commit messages between two git references
- Groups changes by type (Features, Bug Fixes, Breaking Changes, Other)
- Generates formatted release notes in markdown or text format
- Supports emoji icons for better readability in GitHub releases

**Change Categories:**
- **⚠️ BREAKING CHANGES**: Commits with `!` or `BREAKING CHANGE:` footer
- **✨ Features**: Commits starting with `feat:`
- **🐛 Bug Fixes**: Commits starting with `fix:`
- **📝 Other Changes**: Documentation, chore, refactor, style, performance, test, build, CI commits

**Release Notes Strategy:**
- **CI Builds (Prereleases)**: Compare to last prerelease version
- **Stable Releases**: Compare to last stable release version
- If no previous version found, uses all commits or shows "Initial release"

**Example usage:**
```powershell
./scripts/Get-ReleaseNotes.ps1 -FromRef "v1.4.0" -ToRef "HEAD" -Format markdown
# Generates markdown release notes for commits between v1.4.0 and HEAD
```

### 3. Workflow Changes (`.github/workflows/publish.yml`)
Updated the Build step to:
- Detect PR events using `$env:GITHUB_EVENT_NAME -eq "pull_request"`
- Read PR title from `$env:GITHUB_EVENT_PATH`
- Validate PR title contains a conventional commit
- Call `Get-NextVersion.ps1` to determine next version
- **Generate release notes** using `Get-ReleaseNotes.ps1`
- Apply the incremented version to CI builds
- Save release notes for GitHub releases and PowerShell Gallery
- Maintain backward compatibility for:
  - Tag-based releases (use tag version)
  - Main branch pushes (analyze commits since last tag)

**Version calculation flow:**
```
1. Get latest tag (e.g., v1.4.0)
2. Parse PR title for conventional commit
3. Determine bump type (major/minor/patch)
4. Calculate next version (e.g., 1.5.0)
5. Generate release notes comparing to appropriate previous version
6. Add prerelease suffix (e.g., 1.5.0-ci20241102123)
7. Update module manifest
8. Save release notes to files (markdown and text formats)
```

**Release Notes Integration:**
- **GitHub Releases**: Markdown release notes included in release body
- **PowerShell Gallery**: Text release notes added to module manifest ReleaseNotes field
- **CI Builds**: Compare to last prerelease, include in GitHub prerelease
- **Stable Releases**: Compare to last stable release, include in both GitHub and Gallery

**PR Validation:**
- **Build fails** if PR title does not contain a valid conventional commit message
- Clear error message guides contributors to fix the issue
- Only validates new PRs (not historic commits in the repository)
- Enforces consistent versioning for all new contributions

### 4. PR Template (`.github/pull_request_template.md`)
Created a template that:
- Includes clear instructions at the top for PR title format
- Provides clear examples of each commit type
- Explains version bump rules
- Ensures consistent formatting for parsing
- **REQUIRED** for successful build

**Template structure:**
```markdown
## PR Title Instructions
<!-- Your PR title MUST use conventional commit format -->

Format: <type>(<scope>): <description>
Examples:
- feat: add support for batch operations
- fix: resolve connection timeout issue
```

### 5. PR Validation (`scripts/Test-ConventionalCommits.ps1`)
A PowerShell script that:
- Validates text (PR title, description, or commit message) contains at least one conventional commit message
- Provides clear error messages when validation fails
- Lists all valid commit types and examples
- Integrated into CI/CD workflow to fail builds for non-compliant PRs

**Validation triggers:**
- Empty or missing text → Build fails
- No conventional commit message found → Build fails
- At least one valid conventional commit → Build passes

**Error message example:**
```
ERROR: No conventional commit message found

Text MUST contain at least one conventional commit message.

Required format: <type>(<scope>): <description>

Valid types:
  - feat:     A new feature (minor version bump)
  - fix:      A bug fix (patch version bump)
  ...

Examples:
  - feat: add batch delete operation
  - fix: resolve connection timeout
```

### 6. Documentation Updates

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
**PR Title:**
```
feat: add batch delete operation
```

**Result:** 1.4.0 → 1.5.0-ci20241102123

### Example 2: Bug Fix (Patch Bump)
**PR Title:**
```
fix: resolve connection timeout issue
```

**Result:** 1.4.0 → 1.4.1-ci20241102123

### Example 3: Breaking Change (Major Bump)
**PR Title:**
```
feat!: remove deprecated cmdlet parameters
```

**Result:** 1.4.0 → 2.0.0-ci20241102123

### Example 4: With Scope
**PR Title:**
```
fix(auth): handle expired tokens correctly
```

**Result:** 1.4.0 → 1.4.1-ci20241102123

## Benefits

1. **Solves PowerShell Gallery Publication Issue**
   - CI versions now properly increment the base version
   - `1.5.0-ci...` is greater than `1.4.0` and can be published

2. **Automated and Consistent**
   - No manual version management needed
   - Consistent versioning across all PRs
   - Follows semantic versioning principles
   - **Automatic release notes generation** from commit history
   - Single source of truth (PR title) for versioning

3. **Clear Communication**
   - PR titles clearly indicate the type of changes
   - Version bumps are predictable and transparent
   - Breaking changes are explicitly marked
   - **Release notes** automatically generated and included in:
     - GitHub releases (markdown format with emoji icons)
     - PowerShell Gallery (text format in module manifest)
   - Easy to scan PR list for types of changes

4. **Developer Friendly**
   - Simple format to follow
   - Clear documentation and examples
   - PR template guides contributors
   - Validation through tests
   - Release notes organized by change type

5. **Backward Compatible**
   - Existing tag-based releases still work
   - Main branch commits analyzed for version bumps
   - Falls back to patch bump if no conventional commits found
   - Release notes compare to appropriate previous version automatically

## Migration Notes

### For New PRs
- Use conventional commit format in your PR title
- Follow the format: `<type>(<scope>): <description>`
- Examples: `feat: add new feature`, `fix: resolve bug`
- The workflow will automatically determine the version
- PR template provides instructions and examples

### For Existing Workflow
- Tag-based releases: No changes needed, continue using git tags
- Main branch pushes: Commits since last tag are analyzed
- PR builds: Uses PR title for version calculation

## Validation

All changes have been validated:
- ✅ Get-NextVersion.ps1 unit tests pass (5/5 scenarios)
- ✅ Workflow simulation test passes
- ✅ dotnet build completes successfully
- ✅ YAML syntax is valid
- ✅ Documentation is comprehensive

## Next Steps

1. Merge this PR to enable conventional commits versioning via PR titles
2. Update existing open PRs to use conventional commit format in their titles
3. Monitor CI builds to ensure version calculation works correctly
4. Consider adding PR title validation as a separate check for better visibility

## References

- [Conventional Commits Specification](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [PowerShell Gallery Publishing](https://learn.microsoft.com/en-us/powershell/gallery/how-to/publishing-packages/publishing-a-package)
