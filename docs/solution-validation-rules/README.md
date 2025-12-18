# Solution Validation Rules

This directory contains detailed documentation for each validation rule used by the `Test-DataverseSolution` cmdlet.

## Overview

The Test-DataverseSolution cmdlet validates Dataverse solutions against best practices to identify potential issues before deployment. Each rule has:
- A unique identifier (e.g., SV001)
- A severity level (Error, Warning, or Info)
- Detailed explanation of why it matters
- Examples of how to fix the issue
- Links to related Microsoft documentation

## Available Rules

### [SV001: Managed Table Include Subcomponents](SV001.md)
**Severity:** Error

Managed table components should not be included in a solution with the "Include Subcomponents" behavior. This can cause conflicts when the managed table has been customized in the target environment.

**Common Cause:** Adding a managed table to a solution without properly selecting which subcomponents to include.

**Fix:** Change to "Do Not Include Subcomponents" and explicitly add only the customized subcomponents.

---

### [SV002: Managed Non-Table Not Customized](SV002.md)
**Severity:** Warning

Managed non-table components (web resources, workflows, plugins, forms, views, etc.) should only be included if they have been customized. Including unmodified managed components bloats the solution unnecessarily.

**Common Cause:** Automatically adding all components related to a feature without checking if they're actually customized.

**Fix:** Remove unmodified managed components from the solution.

---

### [SV003: Managed Subcomponent Not Customized](SV003.md)
**Severity:** Warning

Managed table subcomponents (attributes, relationships, forms, views, etc.) should only be included if they have been customized. Including unmodified managed subcomponents is unnecessary.

**Common Cause:** Including a managed table with "Include Subcomponents" which adds all subcomponents regardless of customization status.

**Fix:** Remove unmodified subcomponents or change the parent table's behavior to "Do Not Include Subcomponents".

---

## Usage Example

```powershell
# Validate a solution
$results = Test-DataverseSolution -Connection $conn -UniqueName "MySolution" -Verbose

# Check if valid
if ($results.IsValid) {
    Write-Host "✓ Solution is valid and ready for deployment"
} else {
    Write-Host "✗ Solution has validation issues:"
    
    # Group issues by rule
    $results.Issues | Group-Object RuleId | ForEach-Object {
        Write-Host ""
        Write-Host "  $($_.Name) - $($_.Count) issue(s)"
        $_.Group | ForEach-Object {
            Write-Host "    - $($_.ComponentIdentifier): $($_.Message)"
            Write-Host "      See: $($_.DocumentationUrl)"
        }
    }
}
```

## Best Practices

### 1. Validate Before Export
Always validate solutions before exporting them for deployment:

```powershell
$results = Test-DataverseSolution -Connection $conn -UniqueName "MySolution"
if ($results.IsValid) {
    Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -Path ".\MySolution.zip"
}
```

### 2. Fix Errors First
Address all errors (severity level) before deploying to production. Errors indicate issues that could cause deployment failures or conflicts.

### 3. Review Warnings
Review and address warnings when possible. While warnings don't prevent deployment, they indicate unnecessary complexity or potential maintenance issues.

### 4. Document Exceptions
If you have a valid reason to ignore a specific rule violation:
- Document the reason in your solution documentation
- Consider if there's an alternative approach
- Be prepared to maintain that decision as the solution evolves

### 5. Integrate with CI/CD
Add solution validation to your continuous integration pipeline:

```powershell
# In your deployment script
$results = Test-DataverseSolution -Connection $conn -UniqueName $solutionName

if ($results.ErrorCount -gt 0) {
    throw "Solution validation failed with $($results.ErrorCount) error(s)"
}

if ($results.WarningCount -gt 0) {
    Write-Warning "Solution has $($results.WarningCount) warning(s) - review recommended"
}
```

## Understanding Severity Levels

### Error (Severity = 2)
Issues that should be fixed before deploying the solution. Errors indicate:
- High risk of deployment failures
- Likely conflicts with existing customizations
- Violations of critical best practices

**Action Required:** Fix before deployment to production.

### Warning (Severity = 1)
Issues that should be reviewed and ideally fixed. Warnings indicate:
- Unnecessary complexity
- Maintenance challenges
- Potential future issues
- Non-critical best practice violations

**Action Recommended:** Review and fix when possible, especially for new solutions.

### Info (Severity = 0)
Informational messages for awareness. Info messages provide:
- Useful context about the solution
- Suggestions for improvement
- Educational information

**Action Optional:** No action required, but information may be useful.

## Contributing

If you encounter solution issues not covered by these rules, please:
1. Open an issue on the GitHub repository
2. Describe the problem and why it matters
3. Provide examples of the issue
4. Suggest validation logic if possible

## See Also

- [Test-DataverseSolution cmdlet documentation](../Test-DataverseSolution.md)
- [Power Platform ALM Guide](https://learn.microsoft.com/en-us/power-platform/alm/)
- [Solution concepts](https://learn.microsoft.com/en-us/power-platform/alm/solution-concepts-alm)
- [Solution layering](https://learn.microsoft.com/en-us/power-platform/alm/solution-layers-alm)
