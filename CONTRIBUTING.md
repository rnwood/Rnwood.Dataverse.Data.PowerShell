# Contributing to Rnwood.Dataverse.Data.PowerShell

Thank you for your interest in contributing! This document provides guidelines for contributing to this project.

## Pull Request Process

1. **Fork and Clone**: Fork the repository and clone it locally
2. **Create a Branch**: Create a feature branch from `main`
3. **Make Changes**: Make your changes following the coding conventions
4. **Write Tests**: Add tests for your changes
5. **Test Locally**: Run tests to ensure they pass
6. **Use Conventional Commits**: Format your PR title using conventional commits (see below)
7. **Submit PR**: Submit your pull request with a clear description

## Conventional Commits for Versioning

This project uses **Conventional Commits** in PR titles to automatically determine version numbers for CI builds.

### Format

Your PR title should follow this format:

```
<type>(<scope>): <description>
```

Examples:
- `feat: add batch delete operation`
- `fix(auth): resolve connection timeout`
- `feat!: remove deprecated parameters`

### Types and Version Bumps

| Type | Version Bump | Example |
|------|--------------|---------|
| `feat:` | Minor (1.4.0 → 1.5.0) | `feat: add batch delete operation` |
| `fix:` | Patch (1.4.0 → 1.4.1) | `fix: resolve connection timeout` |
| **Any type with `!`** or `BREAKING CHANGE:` | **Major (1.4.0 → 2.0.0)** | `feat!: remove deprecated parameters`<br/>`fix!: change parameter types`<br/>`docs!: restructure all documentation` |
| `docs:` | Patch (1.4.0 → 1.4.1) | `docs: update installation guide` |
| `chore:`, `style:`, `refactor:`, `perf:`, `test:`, `build:`, `ci:` | Patch | `chore: update dependencies` |

### Scope (Optional)

The scope provides additional context about what part of the codebase is affected:

```
feat(auth): add device code flow
fix(metadata): handle null attribute values
docs(readme): update quick start examples
```

### Breaking Changes

Breaking changes trigger a **major version bump**. Indicate breaking changes in one of two ways:

1. **Add `!` after the type** (works with ANY type): `feat!:`, `fix!:`, `docs!:`, `chore!:`, etc.
2. **Use `BREAKING CHANGE:` footer**:

```
feat: change parameter names

BREAKING CHANGE: renamed -TableName to -Table for consistency
```

**Note:** According to the Conventional Commits specification, the `!` suffix can be used with any commit type to indicate a breaking change, not just `feat` and `fix`. For example:
- `docs!: restructure entire documentation` (breaking documentation changes)
- `build!: upgrade to .NET 9` (breaking build changes)
- `chore!: drop support for PowerShell 5.1` (breaking maintenance changes)

### Examples

**Adding a new feature:**
```
PR Title: feat: add support for many-to-many relationships

PR Description:
- Implement Set-DataverseRelationshipMetadata cmdlet
- Add relationship creation/update logic
- Include tests for OneToMany and ManyToMany scenarios
```

**Fixing a bug:**
```
PR Title: fix: handle expired authentication tokens

PR Description:
Connection now automatically refreshes when token expires,
preventing users from seeing authentication errors.
```

**Breaking change:**
```
PR Title: feat!: remove deprecated cmdlet parameters

PR Description:
BREAKING CHANGE: The following parameters have been removed:
- Get-DataverseRecord: -UseDeprecatedAPI
- Set-DataverseRecord: -LegacyBehavior

Users should update scripts to use the new API.
```

**With scope:**
```
PR Title: fix(auth): handle expired tokens correctly

PR Description:
Updated token refresh logic to prevent authentication errors.
```

## PR Template

When you create a PR, the template will include instructions for formatting your PR title. The CI/CD pipeline will parse your PR title to determine the next version number.

Example PR:

```markdown
PR Title: feat: add batch operations support

## Description
This PR adds support for batch operations and fixes a connection timeout issue.

## Changes Made
- Added new BatchRequest handler
- Updated connection retry logic
- Added comprehensive tests

## Testing
- [x] Unit tests pass
- [x] E2E tests pass
- [x] Manual testing completed
```

## Coding Conventions

### C# Cmdlets

- Use `[Cmdlet(Verb, "Noun")]` with approved verbs
- Use `[Parameter]` attributes with appropriate settings
- Call `ShouldProcess()` before destructive operations
- Use `WriteObject()`, `WriteVerbose()`, `WriteWarning()` for output
- Follow existing patterns for error handling

### PowerShell Scripts

- Start with `$ErrorActionPreference = "Stop"`
- Use approved verbs
- Use PascalCase for parameter names
- Document functions with comment-based help

### Testing

- Add tests for all new functionality
- Use Pester 5.x format
- Run tests via `tests/All.Tests.ps1`
- Use FakeXrmEasy for mocking

## Building and Testing

### Prerequisites

- .NET SDK 8.0+
- PowerShell 7+ or PowerShell 5.1+
- Pester 5.x

### Build

```bash
# Clean
dotnet clean

# Build
dotnet build
```

### Test

```powershell
# Set module path
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

# Install Pester
Install-Module -Force -Scope CurrentUser Pester -MinimumVersion 5.0

# Run all tests (5+ minutes)
$config = New-PesterConfiguration
$config.Run.Path = 'tests/All.Tests.ps1'
$config.Run.PassThru = $true
$config.Output.Verbosity = 'Normal'
$result = Invoke-Pester -Configuration $config

# Or run filtered tests (20-30 seconds)
$config.Filter.FullName = '*YourFeature*'
$result = Invoke-Pester -Configuration $config
```

## Documentation

- Update cmdlet help in code with `[Parameter(HelpMessage="...")]`
- Update markdown docs in `Rnwood.Dataverse.Data.PowerShell/docs/`
- Run `updatehelp.ps1` to sync documentation
- Add examples that demonstrate the new functionality

## Questions?

- Open an issue for questions about contributing
- Check existing issues and PRs for similar discussions
- Review the [README](README.md) and documentation

## Code of Conduct

Be respectful and professional in all interactions. We want this to be a welcoming community for all contributors.
