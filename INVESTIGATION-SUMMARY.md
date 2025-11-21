# Summary: CI Version 3.x Bump Investigation

## Quick Facts

- **Issue**: CI generating 3.0.0-ci instead of 2.2.0-ci
- **Root Cause**: Old prerelease tag (v3.0.0-ci202511172077) created before stable release 2.1.3 was being included in version calculation
- **Impact**: All subsequent CI builds inherited the 3.0.0 version
- **Fix**: Workflow now filters out prerelease tags created before the stable release

## Problem Analysis

### What Happened

1. Commit 300f77c contained a breaking change (`feat!:`)
2. CI created prerelease tag v3.0.0-ci202511172077 (correct at that time)
3. Later, 2.1.3 was released (should have been 3.0.0 per semver)
4. Subsequent CI builds included the old 3.0.0-ci tag in version calculation
5. New builds inherited 3.0.0 even though only feat/fix commits were added

### Why It Happened

The workflow's "existing prerelease" logic didn't check if prerelease tags were created before or after the stable release. It collected ALL prerelease tags merged to HEAD, including historical ones that are part of the stable release.

## Solution Implemented

### Code Changes

1. **Workflow Fix** (.github/workflows/publish.yml)
   - Added chronological filtering using `git merge-base --is-ancestor`
   - Only includes prerelease tags on commits AFTER stable tag
   - Prevents historical tags from affecting version calculation

2. **Cleanup Script** (scripts/Clean-InvalidPrereleases.ps1)
   - Identifies prerelease tags created before stable release
   - Can delete them with `-Delete` flag
   - Found 43 invalid tags (including the problematic v3.0.0-ci202511172077)

3. **Test Script** (scripts/Test-PrereleaseFiltering.ps1)
   - Validates the filtering logic
   - Confirms old tags are excluded, new tags are included

4. **Documentation** (docs/troubleshooting/ci-version-3x-bump-investigation.md)
   - Detailed root cause analysis
   - Resolution steps
   - Lessons learned

### Testing Results

✅ All tests passing:
- Test-PrereleaseFiltering.ps1: 2/2 passed
- Test-WorkflowVersionCalculation.ps1: 8/8 passed
- Version simulation: Correctly calculates 2.2.0

## Resolution Steps

### To Fix Current Issue

Delete the 5 incorrectly-versioned 3.0.0-ci tags:

```bash
git push origin :refs/tags/v3.0.0-ci202511172077 \
                :refs/tags/v3.0.0-ci202511192146 \
                :refs/tags/v3.0.0-ci202511202167 \
                :refs/tags/v3.0.0-ci202511202176 \
                :refs/tags/v3.0.0-ci202511202188
```

Or use the cleanup script:
```bash
# List invalid tags
./scripts/Clean-InvalidPrereleases.ps1

# Delete them
./scripts/Clean-InvalidPrereleases.ps1 -Delete
git push origin :refs/tags/v3.0.0-ci202511172077 # ... etc
```

### After Tag Deletion

The next CI build will:
1. Find no 3.0.0-ci prereleases (deleted)
2. Analyze commits since 2.1.3 (feat + fix commits)
3. Calculate version 2.2.0 (minor bump)
4. Create tag v2.2.0-ci...

## Files Modified

- `.github/workflows/publish.yml` - Workflow fix
- `scripts/Clean-InvalidPrereleases.ps1` - New cleanup tool
- `scripts/Test-PrereleaseFiltering.ps1` - New test
- `docs/troubleshooting/ci-version-3x-bump-investigation.md` - Full documentation

## Key Takeaways

1. **Prerelease tags should be ephemeral** - Clean up after stable releases
2. **Version calculation must be scoped** - Only consider relevant historical context
3. **Breaking changes matter** - The 2.1.3 release should have been 3.0.0
4. **Test thoroughly** - Include tests for edge cases in version logic

## References

- Full documentation: `docs/troubleshooting/ci-version-3x-bump-investigation.md`
- Conventional Commits: https://www.conventionalcommits.org/
- Semantic Versioning: https://semver.org/
