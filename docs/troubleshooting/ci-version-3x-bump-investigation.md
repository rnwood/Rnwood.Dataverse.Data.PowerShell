# CI Version 3.x Bump Investigation and Fix

## Problem Statement

The CI version calculation was generating 3.0.0-ci versions when it should have been generating 2.2.0-ci versions, despite there being no breaking changes since the last stable release (2.1.3).

## Root Cause Analysis

### Timeline of Events

1. **Commit 300f77c** (before 2.1.3): Contains `feat!: exclude content by default in Get-DataverseWebResource, add -IncludeContent (#263)`
   - This is a breaking change commit (indicated by `feat!:`)
   - According to semantic versioning, this should trigger a major version bump (2.x → 3.0.0)

2. **CI Tag v3.0.0-ci202511172077** created on commit 3f7044c
   - This commit is in the history before 2.1.3
   - The CI correctly identified the breaking change and created a 3.0.0 prerelease

3. **Stable Release 2.1.3** tagged on commit daa1a40
   - Released AFTER the breaking change commit
   - Should have been 3.0.0 according to semantic versioning
   - But was released as 2.1.3 instead

4. **Subsequent CI Builds** (commits after 2.1.3):
   - Workflow logic collected ALL prerelease tags merged to HEAD
   - This included v3.0.0-ci202511172077 (created before 2.1.3)
   - New commits only had `feat:` and `fix:` (minor/patch bumps)
   - Expected version: 2.2.0 (minor bump from 2.1.3)
   - Actual version: 3.0.0 (inherited from old prerelease tag)

5. **Version Perpetuation**:
   - v3.0.0-ci202511192146 created with version 3.0.0
   - v3.0.0-ci202511202167 created with version 3.0.0
   - v3.0.0-ci202511202176 created with version 3.0.0
   - v3.0.0-ci202511202188 created with version 3.0.0

### Why This Happened

The workflow's "existing prerelease" logic is designed to prevent version double-bumping when multiple PRs with similar changes are merged in sequence. For example:

- PR1 with `feat!:` merged → creates 2.0.0-ci prerelease
- PR2 with `feat!:` based on old commit → should also use 2.0.0, not 3.0.0

The logic compares newly calculated versions against existing prerelease versions and uses the higher one. This is correct for preventing double-bumping, but it has a flaw:

**The workflow did NOT check if existing prerelease tags were created BEFORE or AFTER the stable release tag.**

This meant that v3.0.0-ci202511172077, which was created before 2.1.3 and is part of the 2.1.3 release history, was incorrectly included in the version calculation for builds after 2.1.3.

## Fix Implementation

### 1. Workflow Change (.github/workflows/publish.yml)

Added logic to filter out prerelease tags that are on commits BEFORE the stable release:

```powershell
# Check if this prerelease tag is on a commit that comes AFTER the stable tag
$tagCommit = git rev-list -n 1 $tag 2>$null
$stableCommit = git rev-list -n 1 $latestStableTag 2>$null

if ($tagCommit -and $stableCommit) {
  # Check if stable tag is an ancestor of this prerelease tag
  $isAfterStable = git merge-base --is-ancestor $stableCommit $tagCommit 2>$null
  
  if ($LASTEXITCODE -ne 0) {
    # Stable tag is NOT an ancestor, so this prerelease is not after it
    continue  # Skip this prerelease tag
  }
  
  # Also verify the prerelease is not the same commit as the stable tag
  if ($tagCommit -eq $stableCommit) {
    continue
  }
}
```

This ensures only prerelease tags created AFTER the stable release are considered.

### 2. Cleanup Script (scripts/Clean-InvalidPrereleases.ps1)

Created a script to identify and optionally delete prerelease tags that are on commits before the stable release:

```bash
# List invalid tags
./scripts/Clean-InvalidPrereleases.ps1

# Delete invalid tags after confirmation
./scripts/Clean-InvalidPrereleases.ps1 -Delete

# Delete without confirmation
./scripts/Clean-InvalidPrereleases.ps1 -Delete -Force
```

The script found 43 invalid prerelease tags, including v3.0.0-ci202511172077.

### 3. Test Script (scripts/Test-PrereleaseFiltering.ps1)

Created a test to validate the filtering logic works correctly:

```bash
./scripts/Test-PrereleaseFiltering.ps1
```

## Resolution Steps

### Immediate Fix (To Stop 3.0.0 Version)

Delete all 3.0.0-ci tags from the repository:

```bash
# Delete from remote
git push origin :refs/tags/v3.0.0-ci202511172077
git push origin :refs/tags/v3.0.0-ci202511192146
git push origin :refs/tags/v3.0.0-ci202511202167
git push origin :refs/tags/v3.0.0-ci202511202176
git push origin :refs/tags/v3.0.0-ci202511202188

# Delete from local (if needed)
git tag -d v3.0.0-ci202511172077
git tag -d v3.0.0-ci202511192146
git tag -d v3.0.0-ci202511202167
git tag -d v3.0.0-ci202511202176
git tag -d v3.0.0-ci202511202188
```

After deleting these tags, the next CI build will correctly calculate version 2.2.0-ci based on the `feat:` commits since 2.1.3.

### Long-term Prevention (Already Implemented)

The workflow fix ensures that future builds will not include prerelease tags created before the stable release, preventing this issue from recurring.

## Verification

After applying the fix and deleting the 3.0.0-ci tags:

1. **Commits since 2.1.3**:
   - `feat(sitemap)`: validate entity names (minor bump)
   - `feat`: add wildcard support (minor bump)
   - `fix(forms)`: subgrid controls XML (patch bump)
   - `fix`: sitemap XML corruption (patch bump)
   - `fix`: auto-determine TableName (patch bump)
   - `build`: automated Sql4Cds build (patch bump)

2. **Expected Version**: 2.2.0 (highest bump is minor)

3. **Verification Command**:
   ```bash
   # Simulate version calculation without 3.0.0-ci tags
   pwsh -Command '
   $commits = git log "2.1.3..HEAD" --format="%s"
   $messages = $commits -split "`n" | Where-Object { $_ -match "\S" }
   & ./scripts/Get-NextVersion.ps1 -BaseVersion "2.1.3" -CommitMessages $messages
   '
   ```
   
   Result: `2.2.0` ✓

## Additional Cleanup (Optional)

The cleanup script identified 43 prerelease tags created before 2.1.3. These don't affect the current issue but could be cleaned up:

```bash
./scripts/Clean-InvalidPrereleases.ps1 -Delete
```

This will remove all prerelease tags that are on commits before the stable release.

## Lessons Learned

1. **Semantic Versioning Correctness**: The breaking change at commit 300f77c should have resulted in a 3.0.0 stable release, not 2.1.3. However, this is a past issue that can't be corrected without breaking existing installations.

2. **Prerelease Tag Lifecycle**: Prerelease tags should be considered ephemeral. Once a stable release is tagged, all prior prerelease tags for that version line should be cleaned up or ignored.

3. **Workflow Logic**: The "existing prerelease" comparison logic needs to be scoped to only tags created AFTER the current stable release to avoid inheritance of old versions.

4. **Version Calculation Idempotency**: The version calculation should be idempotent - running it multiple times on the same commit should produce the same result. The fix ensures this by filtering out irrelevant historical prerelease tags.

## References

- Issue: Investigate why CI version number is generating 3.x bump
- Conventional Commits: https://www.conventionalcommits.org/
- Semantic Versioning: https://semver.org/
- Workflow: `.github/workflows/publish.yml`
- Version Script: `scripts/Get-NextVersion.ps1`
