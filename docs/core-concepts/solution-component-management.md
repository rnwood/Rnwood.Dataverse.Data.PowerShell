<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Solution Component Management](#solution-component-management)
  - [Overview](#overview)
  - [Key Concepts](#key-concepts)
    - [Component Types](#component-types)
    - [Root Component Behavior](#root-component-behavior)
  - [Managing Components](#managing-components)
    - [Adding Components to Solutions](#adding-components-to-solutions)
    - [Changing Component Behavior](#changing-component-behavior)
    - [Adding Different Component Types](#adding-different-component-types)
      - [Entity (Table) - Type 1](#entity-table---type-1)
      - [Attribute (Column) - Type 2](#attribute-column---type-2)
      - [View - Type 26](#view---type-26)
      - [Form - Type 24](#form---type-24)
    - [Listing Solution Components](#listing-solution-components)
  - [Advanced Scenarios](#advanced-scenarios)
    - [Adding with Required Dependencies](#adding-with-required-dependencies)
    - [Idempotent Operations](#idempotent-operations)
    - [Preview Changes with WhatIf](#preview-changes-with-whatif)
    - [Getting Output Details](#getting-output-details)
  - [Best Practices](#best-practices)
  - [Related Topics](#related-topics)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Solution Component Management

This guide covers managing individual solution components within Dataverse solutions.

## Overview

Solution components are the building blocks of a solution - entities, attributes, forms, views, processes, and other customizations. This module provides cmdlets to add, update, and manage these components within solutions.

## Key Concepts

### Component Types

Every solution component has a type represented by a numeric value:

| Type | Component | Example |
|------|-----------|---------|
| 1 | Entity (Table) | contact, account, custom tables |
| 2 | Attribute (Column) | firstname, emailaddress1 |
| 9 | Option Set (Choice) | Status reason, custom choices |
| 10 | Entity Relationship | N:1, 1:N, N:N relationships |
| 24 | Form | Main form, Quick create form |
| 26 | View | Active contacts, My open opportunities |
| 29 | Web Resource | JavaScript, CSS, HTML, images |
| 60 | Chart | Pie chart, Bar chart |
| 80 | Process (Workflow) | Business process flow, Workflow |

For a complete list, see [Microsoft's documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file).

### Root Component Behavior

When adding a component to a solution, you can specify how its subcomponents should be included:

- **0 - Include Subcomponents** (default): Includes all subcomponents (e.g., attributes, forms, views for an entity)
- **1 - Do Not Include Subcomponents**: Includes only the root component
- **2 - Include As Shell**: Includes the component shell only (limited API support)

**Important:** Dataverse does not allow updating the behavior of an existing component directly. To change behavior, the component must be removed and re-added.

## Managing Components

### Adding Components to Solutions

Use `Set-DataverseSolutionComponent` to add a component to a solution:

```powershell
# Get the component's metadata ID
$entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customtable"

# Add to solution with default behavior (Include Subcomponents)
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId `
    -ComponentType 1 `
    -Behavior 0
```

### Changing Component Behavior

To change the behavior of an existing component, use the same cmdlet. It will automatically remove and re-add the component:

```powershell
# Change from "Include Subcomponents" (0) to "Do Not Include Subcomponents" (1)
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId `
    -ComponentType 1 `
    -Behavior 1 `
    -PassThru

# Output includes:
# - SolutionComponentId: GUID of the solution component record
# - WasUpdated: True (indicates behavior was changed)
```

### Adding Different Component Types

#### Entity (Table) - Type 1

```powershell
$entityMeta = Get-DataverseEntityMetadata -Connection $c -EntityName "account"
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMeta.MetadataId -ComponentType 1 -Behavior 0
```

#### Attribute (Column) - Type 2

```powershell
$attributeMeta = Get-DataverseAttributeMetadata -Connection $c `
    -EntityName "account" -AttributeName "telephone1"
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $attributeMeta.MetadataId -ComponentType 2 -Behavior 0
```

#### View - Type 26

```powershell
# Get view ID from the savedquery table
$view = Get-DataverseRecord -Connection $c -TableName savedquery `
    -Filter @{ "name" = "Active Accounts" }

Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $view.savedqueryid -ComponentType 26 -Behavior 0
```

#### Form - Type 24

```powershell
# Get form ID from the systemform table
$form = Get-DataverseRecord -Connection $c -TableName systemform `
    -Filter @{ "name" = "Account" }

Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $form.formid -ComponentType 24 -Behavior 0
```

### Removing Components from Solutions

Use `Remove-DataverseSolutionComponent` to remove a component from a solution:

```powershell
# Remove an entity component
$entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customtable"
Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId -ComponentType 1

# Remove an attribute component
$attributeMetadata = Get-DataverseAttributeMetadata -Connection $c `
    -EntityName "account" -AttributeName "new_customfield"
Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $attributeMetadata.MetadataId -ComponentType 2 -Confirm:$false
```

**Important:** This only removes the component from the solution. The component itself remains in the environment. To delete the component entirely, use the appropriate `Remove-Dataverse*Metadata` cmdlet.

### Listing Solution Components

Use `Get-DataverseSolutionComponent` to view components in a solution:

```powershell
# List all components in a solution
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution"

# Include implied subcomponents
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -IncludeImpliedSubcomponents
```

## Advanced Scenarios

### Adding with Required Dependencies

Some components require other components. Use `-AddRequiredComponents` to automatically include dependencies:

```powershell
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $viewId -ComponentType 26 `
    -AddRequiredComponents -Behavior 0
```

### Idempotent Operations

`Set-DataverseSolutionComponent` is idempotent - running it multiple times with the same parameters has no effect:

```powershell
# First run: adds component
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityId -ComponentType 1 -Behavior 0

# Second run: no action (component already exists with same behavior)
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityId -ComponentType 1 -Behavior 0
```

Similarly, `Remove-DataverseSolutionComponent` supports `-IfExists` for idempotent removal:

```powershell
# Remove component if it exists, no error if it doesn't
Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityId -ComponentType 1 -IfExists
```

### Preview Changes with WhatIf

Use `-WhatIf` to preview changes without making them:

```powershell
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityId -ComponentType 1 -Behavior 1 -WhatIf
```

### Getting Output Details

Use `-PassThru` to get detailed information about the operation:

```powershell
$result = Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityId -ComponentType 1 -Behavior 0 -PassThru

Write-Host "Solution Component ID: $($result.SolutionComponentId)"
Write-Host "Behavior: $($result.Behavior)"
Write-Host "Was Updated: $($result.WasUpdated)"
```

## Best Practices

1. **Use metadata IDs**: Always use the MetadataId from metadata cmdlets (Get-DataverseEntityMetadata, Get-DataverseAttributeMetadata) for entities and attributes.

2. **Check behavior before changing**: Changing behavior triggers a remove/re-add operation. Use `Get-DataverseSolutionComponent` to check current behavior first.

3. **Include required components**: Use `-AddRequiredComponents` when adding views, forms, or other components that depend on entities.

4. **Publish after changes**: Remember to publish customizations after modifying solution components:
   ```powershell
   Publish-DataverseCustomizations -Connection $c
   ```

5. **Use WhatIf for validation**: Always use `-WhatIf` first when making significant changes to understand the impact.

6. **Work with unmanaged solutions**: Solution component management only works with unmanaged solutions. Managed solutions are read-only.

## Related Topics

- [Solution Management](../advanced/solution-management.md) - Import, export, and manage solutions
- [Working with Metadata](metadata.md) - Managing entities, attributes, and relationships
- [Set-DataverseSolutionComponent](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSolutionComponent.md) - Full cmdlet reference
- [Get-DataverseSolutionComponent](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSolutionComponent.md) - Full cmdlet reference
- [Remove-DataverseSolutionComponent](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSolutionComponent.md) - Full cmdlet reference
