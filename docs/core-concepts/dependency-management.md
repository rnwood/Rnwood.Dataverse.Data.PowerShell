# Dependency Management

This guide covers managing and understanding component dependencies in Dataverse.

## Overview

In Dataverse, components (such as entities, attributes, forms, views, and web resources) can have dependencies on other components. Understanding these dependencies is critical when:
- Deleting components
- Uninstalling solutions
- Importing solutions to new environments
- Managing customizations

## Key Concepts

### What are Dependencies?

Dependencies in Dataverse represent relationships between components where one component requires another to function properly. For example:
- A form depends on attributes it displays
- A view depends on attributes it includes in its columns
- A workflow depends on entities and attributes it references
- A web resource (JavaScript) depends on entities it manipulates

### Types of Dependencies

1. **Required Component Dependencies** - Components that another component needs to exist and function
2. **Dependent Components** - Components that rely on a particular component
3. **Missing Dependencies** - Required components that don't exist in the target environment
4. **Uninstall Dependencies** - External dependencies that prevent solution uninstallation

## Cmdlets

### Get-DataverseDependency

Retrieves dependencies that would prevent a component from being deleted.

**Use when:**
- Planning to delete an entity, attribute, form, or other component
- Understanding what other components rely on a specific component
- Troubleshooting why a component cannot be deleted

```powershell
# Check what depends on an entity before deleting it
$entityMetadata = Get-DataverseEntityMetadata -EntityName "new_customtable"
$dependencies = Get-DataverseDependency -ObjectId $entityMetadata.MetadataId -ComponentType 1

if ($dependencies) {
    Write-Host "Cannot delete - found $($dependencies.Count) dependencies:"
    $dependencies | Format-Table dependentcomponentobjectid, dependentcomponenttype
} else {
    # Safe to delete
    Remove-DataverseEntityMetadata -EntityName "new_customtable"
}
```

### Get-DataverseDependentComponent

Retrieves all components that depend on a specified component.

**Use when:**
- Understanding the impact of changing a component
- Finding all forms that use a specific attribute
- Identifying components affected by schema changes

```powershell
# Find all forms and views that use a specific attribute
$attrMetadata = Get-DataverseAttributeMetadata -EntityName "contact" -AttributeName "new_customfield"
$dependents = Get-DataverseDependentComponent -ObjectId $attrMetadata.MetadataId -ComponentType 2

# Group by component type to see the breakdown
$dependents | Group-Object dependentcomponenttype | ForEach-Object {
    $typeName = switch ($_.Name) {
        "24" { "Forms" }
        "26" { "Views" }
        "29" { "Web Resources" }
        "80" { "Workflows" }
        default { "Type $($_.Name)" }
    }
    Write-Host "$typeName: $($_.Count)"
}
```

### Get-DataverseMissingDependency

Retrieves missing dependencies for a solution.

**Use when:**
- Validating a solution before import
- Troubleshooting solution import failures
- Planning environment preparation for solution deployment

```powershell
# Check for missing dependencies before importing
$missingDeps = Get-DataverseMissingDependency -SolutionUniqueName "MyCustomSolution"

if ($missingDeps) {
    Write-Host "Missing $($missingDeps.Count) dependencies - import will fail"
    
    # Create report of missing components
    $missingDeps | ForEach-Object {
        [PSCustomObject]@{
            ComponentId = $_.missingcomponentid
            ComponentType = $_.missingcomponenttype
            Description = "Missing component required by solution"
        }
    } | Format-Table
    
    Write-Host "Import these dependencies first or add them to the solution"
} else {
    Write-Host "All dependencies satisfied - ready to import"
    # Proceed with import
    Import-DataverseSolution -SolutionFile "MyCustomSolution.zip"
}
```

### Get-DataverseUninstallDependency

Retrieves dependencies that would prevent a solution from being uninstalled.

**Use when:**
- Planning to uninstall a solution
- Understanding what external components reference solution components
- Preparing for solution removal

```powershell
# Check if a solution can be safely uninstalled
$dependencies = Get-DataverseUninstallDependency -SolutionUniqueName "MyOldSolution"

if ($dependencies) {
    Write-Host "Cannot uninstall - found $($dependencies.Count) external dependencies"
    
    # Show which components from other solutions depend on this solution
    $dependencies | Format-Table @{
        Label="External Component"
        Expression={$_.dependentcomponentobjectid}
    }, @{
        Label="Depends On"
        Expression={$_.requiredcomponentobjectid}
    }
    
    Write-Host "Remove these dependencies before uninstalling"
} else {
    # Safe to uninstall
    Remove-DataverseSolution -SolutionUniqueName "MyOldSolution"
}
```

## Common Scenarios

### Scenario 1: Safely Deleting a Custom Entity

```powershell
# Step 1: Get the entity metadata
$entity = Get-DataverseEntityMetadata -EntityName "new_customentity"

# Step 2: Check for dependencies
$dependencies = Get-DataverseDependency -ObjectId $entity.MetadataId -ComponentType 1

# Step 3: Analyze dependencies
if ($dependencies.Count -eq 0) {
    Write-Host "✓ Entity has no dependencies - safe to delete"
    Remove-DataverseEntityMetadata -EntityName "new_customentity"
} else {
    Write-Host "✗ Found $($dependencies.Count) dependencies:"
    
    # Group by type
    $dependencies | Group-Object dependentcomponenttype | ForEach-Object {
        Write-Host "  - Type $($_.Name): $($_.Count) components"
    }
    
    Write-Host "`nResolve these dependencies first:"
    Write-Host "  1. Remove forms, views, and relationships"
    Write-Host "  2. Update workflows and business rules"
    Write-Host "  3. Check web resources for references"
}
```

### Scenario 2: Preparing for Solution Import

```powershell
# Check source solution for any missing dependencies in target environment
$missingDeps = Get-DataverseMissingDependency -SolutionUniqueName "MyApp"

if ($missingDeps) {
    Write-Host "Pre-requisites needed:"
    
    # Create installation order
    $componentTypes = @{
        1 = "Entities"
        2 = "Attributes"
        9 = "Option Sets"
        10 = "Relationships"
    }
    
    $missingDeps | Group-Object missingcomponenttype | ForEach-Object {
        $typeName = $componentTypes[[int]$_.Name]
        if (-not $typeName) { $typeName = "Component Type $($_.Name)" }
        Write-Host "  - $typeName`: $($_.Count)"
    }
    
    Write-Host "`nNext steps:"
    Write-Host "  1. Import dependent solutions first"
    Write-Host "  2. Or add missing components to this solution"
} else {
    Write-Host "✓ All dependencies satisfied"
    Import-DataverseSolution -SolutionFile "MyApp.zip"
}
```

### Scenario 3: Understanding Component Impact

```powershell
# Analyze what would be affected by removing an attribute
$attribute = Get-DataverseAttributeMetadata -EntityName "account" -AttributeName "new_score"
$dependents = Get-DataverseDependentComponent -ObjectId $attribute.MetadataId -ComponentType 2

Write-Host "Impact Analysis for removing 'new_score' attribute:"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

$summary = $dependents | Group-Object dependentcomponenttype | ForEach-Object {
    $typeName = switch ($_.Name) {
        "24" { "Forms" }
        "26" { "Views" }
        "29" { "Web Resources" }
        "59" { "Charts" }
        "80" { "Workflows/Business Rules" }
        default { "Other Components (Type $($_.Name))" }
    }
    
    [PSCustomObject]@{
        Component = $typeName
        Count = $_.Count
        Action = "Must be updated before deletion"
    }
}

$summary | Format-Table -AutoSize

if ($summary) {
    Write-Host "`n⚠ Warning: Removing this attribute will break $($dependents.Count) components"
} else {
    Write-Host "`n✓ No dependencies found - attribute can be safely removed"
}
```

### Scenario 4: Solution Uninstall Readiness

```powershell
# Check all solutions for uninstall readiness
$solutions = Get-DataverseSolution | Where-Object { -not $_.ismanaged }

$readiness = $solutions | ForEach-Object {
    $deps = Get-DataverseUninstallDependency -SolutionUniqueName $_.uniquename
    
    [PSCustomObject]@{
        Solution = $_.friendlyname
        UniqueName = $_.uniquename
        Dependencies = $deps.Count
        CanUninstall = ($deps.Count -eq 0)
    }
}

Write-Host "Solution Uninstall Readiness Report"
Write-Host "═══════════════════════════════════"
$readiness | Format-Table -AutoSize

$canUninstall = $readiness | Where-Object { $_.CanUninstall }
if ($canUninstall) {
    Write-Host "`n✓ Can safely uninstall:"
    $canUninstall | ForEach-Object { Write-Host "  - $($_.Solution)" }
}

$cannotUninstall = $readiness | Where-Object { -not $_.CanUninstall }
if ($cannotUninstall) {
    Write-Host "`n✗ Cannot uninstall (have dependencies):"
    $cannotUninstall | ForEach-Object { 
        Write-Host "  - $($_.Solution) ($($_.Dependencies) dependencies)" 
    }
}
```

## Component Type Reference

Common component types used in dependency operations:

| Type | Component | Description |
|------|-----------|-------------|
| 1 | Entity (Table) | Data tables in your environment |
| 2 | Attribute (Column) | Fields/columns on entities |
| 9 | Option Set (Choice) | Picklist/choice definitions |
| 10 | Relationship | Entity relationships (1:N, N:1, N:N) |
| 24 | Form | Data entry and display forms |
| 26 | View | Saved queries and views |
| 29 | Web Resource | JavaScript, HTML, CSS, images |
| 59 | Chart | Visual data representations |
| 60 | Dashboard | Dashboard definitions |
| 80 | Process | Workflows, business rules, actions |

For a complete list, see [Microsoft's Component Type documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file).

## Best Practices

1. **Always check dependencies before deletion**
   - Use `Get-DataverseDependency` before removing any component
   - Document dependencies for impact analysis

2. **Validate solutions before import**
   - Use `Get-DataverseMissingDependency` in pre-deployment scripts
   - Automate dependency checking in CI/CD pipelines

3. **Plan for solution uninstall**
   - Check `Get-DataverseUninstallDependency` before removing solutions
   - Resolve external dependencies first

4. **Understand impact of changes**
   - Use `Get-DataverseDependentComponent` to see what uses a component
   - Review dependencies when planning refactoring

5. **Include dependencies in solutions**
   - When adding components to solutions, include their dependencies
   - Use the "Add Required Components" option in solution component management

6. **Document custom dependencies**
   - Keep track of custom JavaScript that references entities/attributes
   - Document dependencies in code comments and README files

## Related Topics

- [Solution Component Management](solution-component-management.md) - Managing components in solutions
- [Solution Management](../advanced/solution-management.md) - Import, export, and lifecycle
- [Working with Metadata](metadata.md) - Understanding entities, attributes, and metadata

## Related Cmdlets

- [`Get-DataverseDependency`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseDependency.md)
- [`Get-DataverseDependentComponent`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseDependentComponent.md)
- [`Get-DataverseMissingDependency`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseMissingDependency.md)
- [`Get-DataverseUninstallDependency`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseUninstallDependency.md)
