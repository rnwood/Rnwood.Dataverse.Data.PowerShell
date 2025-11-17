# CI/CD Workflows

## pr-title-validation.yml - PR Title Validation

This workflow validates that PR titles follow conventional commit format, which is required for automatic versioning.

### Triggers

- **Pull Request**: Runs when a PR is opened, reopened, synchronized, or **edited** (title changed)

### Validation Rules

The workflow uses `scripts/Test-ConventionalCommits.ps1` to validate that PR titles use conventional commit format:

- **Format**: `<type>(<scope>): <description>`
- **Valid types**: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`
- **Breaking changes**: Add `!` after type (works with ANY type: `feat!:`, `fix!:`, `docs!:`, `chore!:`, etc.)

### Examples

Valid PR titles:
- `feat: add batch delete operation`
- `fix: resolve connection timeout issue`
- `feat!: remove deprecated parameters` (breaking feature)
- `fix(auth): handle expired tokens correctly`
- `docs!: restructure entire documentation` (breaking docs change)
- `chore!: drop support for PowerShell 5.1` (breaking maintenance change)

Invalid PR titles:
- `Update code` (missing type)
- `feat add feature` (missing colon)
- `random changes` (no conventional commit format)

### Why This Workflow?

The project uses conventional commits for automatic version determination. By validating PR titles in a separate workflow that runs on the `edited` event, we ensure:

1. PR titles are validated immediately when created or edited
2. Version calculation in the build workflow can rely on valid PR titles
3. Feedback is fast and focused (doesn't require full build to run)

## publish.yml - Build and Test Workflow

This workflow handles building, testing, and publishing the PowerShell module.

### Triggers

- **Push**: Runs on every push to any branch
- **Pull Request**: Runs when a PR is opened, reopened, or synchronized
- **Release**: Publishes to PowerShell Gallery when a release is published

### Matrix Strategy

The workflow runs tests across multiple environments:
- Ubuntu + PowerShell 7.4.11
- Windows + PowerShell 7.4.11
- Ubuntu + PowerShell latest
- Windows + PowerShell latest
- Windows + PowerShell 5.1 (Desktop)

### Test Failure Feedback

When tests fail in a pull request, the workflow automatically:

1. **Captures failure details** - Saves test failure information including:
   - Test names and paths
   - Error messages
   - Test statistics (total, passed, failed, skipped)

2. **Uploads artifacts** - Failure reports are uploaded as workflow artifacts for historical analysis

3. **Comments on PR** - Posts a comment to the PR that:
   - @mentions `@copilot` to alert the AI agent
   - Includes formatted failure details
   - Links to the workflow run and commit
   - Shows OS and PowerShell version for each failure

This automation ensures test failures are immediately visible and actionable, enabling faster debugging and resolution.

### Example Failure Comment

```markdown
@copilot please investigate these test failures:

## ❌ Test Failures - ubuntu-latest / PowerShell 7.4.11

**Total Tests:** 396  
**Passed:** 350  
**Failed:** 1  
**Skipped:** 45

### Failed Tests:

- **Remove-DataverseRecord - IfExists Flag > Should handle non-existent record with IfExists**
  ```
  Expected no exception, but got: OrganizationServiceFault 0: contact with Id abc123 Does Not Exist
  ```

**Workflow Run:** https://github.com/owner/repo/actions/runs/12345
**Commit:** abc123def456
```

### Permissions

The workflow requires:
- `contents: read` - To checkout code
- `pull-requests: write` - To post comments on PRs
