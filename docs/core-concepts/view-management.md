<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [View Management](#view-management)
  - [Overview](#overview)
  - [Key Features](#key-features)
  - [Creating Views](#creating-views)
    - [Basic View Creation](#basic-view-creation)
    - [System Views](#system-views)
    - [Complex Filters](#complex-filters)
    - [Column Configuration](#column-configuration)
    - [Using FetchXML](#using-fetchxml)
  - [Updating Views](#updating-views)
    - [Update with Upsert Pattern](#update-with-upsert-pattern)
    - [Add Columns](#add-columns)
    - [Remove Columns](#remove-columns)
    - [Update Column Properties](#update-column-properties)
    - [Update Filters](#update-filters)
    - [Combined Updates](#combined-updates)
  - [Retrieving Views](#retrieving-views)
    - [Get All Views](#get-all-views)
    - [Get by Table](#get-by-table)
    - [Get by ID](#get-by-id)
    - [Get by Name](#get-by-name)
    - [Filter by Type](#filter-by-type)
    - [Filter by Query Type](#filter-by-query-type)
  - [Deleting Views](#deleting-views)
    - [Delete by ID](#delete-by-id)
    - [Delete with Confirmation](#delete-with-confirmation)
    - [Delete if Exists](#delete-if-exists)
    - [Delete System Views](#delete-system-views)
    - [Delete Multiple Views](#delete-multiple-views)
  - [Control Flags](#control-flags)
    - [NoCreate and NoUpdate](#nocreate-and-noupdate)
    - [PassThru](#passthru)
  - [Best Practices](#best-practices)
  - [Common Scenarios](#common-scenarios)
    - [Clone a View](#clone-a-view)
    - [Create Views for Multiple Tables](#create-views-for-multiple-tables)
    - [Audit View Configuration](#audit-view-configuration)
  - [See Also](#see-also)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# View Management

Dataverse views define how records are displayed in model-driven apps and other interfaces. This module provides cmdlets to create, update, retrieve, and delete both system views (savedquery) and personal views (userquery).

## Overview

- **[`Get-DataverseView`](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseView.md)** — Retrieve views with flexible filtering
- **[`Set-DataverseView`](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseView.md)** — Create or update views (upsert pattern)
- **[`Remove-DataverseView`](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseView.md)** — Delete views

## Key Features

- **Upsert pattern**: `Set-DataverseView` creates new views or updates existing ones
- **Dual syntax**: Use simplified hashtable filters or direct FetchXML
- **System and personal views**: Manage both savedquery (system) and userquery (personal) entities
- **Column management**: Add, remove, or update columns with width configuration
- **Complex filters**: Support for nested AND/OR/NOT/XOR logical expressions
- **Wildcard searches**: Find views using pattern matching

## Creating Views

### Basic View Creation

Create a personal view with simple filters:

```powershell
$viewId = Set-DataverseView -Connection $c -PassThru `
    -Name "My Active Contacts" `
    -TableName contact `
    -Columns @("firstname", "lastname", "emailaddress1", "telephone1") `
    -FilterValues @{ statecode = 0 }
```

### System Views

Create a system view accessible to all users:

```powershell
Set-DataverseView -Connection $c -PassThru -SystemView `
    -Name "All Active Contacts" `
    -TableName contact `
    -Columns @("fullname", "emailaddress1", "telephone1") `
    -FilterValues @{ statecode = 0 }
```

### Complex Filters

Use nested hashtables for complex logical expressions:

```powershell
Set-DataverseView -Connection $c -PassThru -SystemView `
    -Name "High Value Opportunities" `
    -TableName opportunity `
    -Columns @("name", "estimatedvalue", "closeprobability", "actualclosedate") `
    -FilterValues @{
        and = @(
            @{ statecode = 0 },
            @{ or = @(
                @{ estimatedvalue = @{ value = 100000; operator = 'GreaterThan' } },
                @{ closeprobability = @{ value = 80; operator = 'GreaterThan' } }
            )}
        )
    }
```

### Column Configuration

Specify column widths and display order:

```powershell
Set-DataverseView -Connection $c -PassThru `
    -Name "Contact Details" `
    -TableName contact `
    -Columns @(
        @{ name = "firstname"; width = 100 },
        @{ name = "lastname"; width = 150 },
        @{ name = "emailaddress1"; width = 250 },
        @{ name = "telephone1"; width = 120 }
    ) `
    -FilterValues @{ statecode = 0 }
```

### Using FetchXML

For advanced scenarios, use FetchXML directly:

```powershell
$fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
      <condition attribute="createdon" operator="last-x-days" value="30" />
    </filter>
  </entity>
</fetch>
"@

Set-DataverseView -Connection $c -PassThru `
    -Name "Recent Contacts" `
    -TableName contact `
    -FetchXml $fetchXml
```

## Updating Views

### Update with Upsert Pattern

Provide the view ID to update an existing view:

```powershell
# Update view name and description
Set-DataverseView -Connection $c -Id $viewId `
    -Name "Updated View Name" `
    -Description "Updated description"
```

### Add Columns

Add new columns to an existing view:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @(
        @{ name = "address1_city"; width = 150 },
        @{ name = "birthdate"; width = 100 }
    )
```

### Remove Columns

Remove columns from a view:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -RemoveColumns @("fax", "address1_line1")
```

### Update Column Properties

Change column widths or order:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -UpdateColumns @(
        @{ name = "firstname"; width = 200 },
        @{ name = "emailaddress1"; width = 300 }
    )
```

### Update Filters

Change the view's filter criteria:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -FilterValues @{
        and = @(
            @{ statecode = 0 },
            @{ emailaddress1 = @{ operator = 'NotNull' } }
        )
    }
```

### Combined Updates

Perform multiple updates in one call:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -Name "Updated Contact View" `
    -Description "Shows active contacts with email" `
    -AddColumns @(@{ name = "mobilephone"; width = 120 }) `
    -RemoveColumns @("fax") `
    -FilterValues @{ statecode = 0; emailaddress1 = @{ operator = 'NotNull' } }
```

## Retrieving Views

### Get All Views

Retrieve all views (both system and personal):

```powershell
Get-DataverseView -Connection $c
```

### Get by Table

Get all views for a specific table:

```powershell
Get-DataverseView -Connection $c -TableName contact
```

### Get by ID

Retrieve a specific view:

```powershell
$view = Get-DataverseView -Connection $c -Id $viewId
```

### Get by Name

Find views by name (supports wildcards):

```powershell
# Exact match
Get-DataverseView -Connection $c -Name "Active Contacts"

# Wildcard search
Get-DataverseView -Connection $c -Name "Active*"
Get-DataverseView -Connection $c -Name "*Contact*"
```

### Filter by Type

Get only system or personal views:

```powershell
# System views only
Get-DataverseView -Connection $c -SystemView

# Personal views only
Get-DataverseView -Connection $c -PersonalView

# System views for a specific table
Get-DataverseView -Connection $c -TableName contact -SystemView
```

### Filter by Query Type

Views have different types for different purposes:

```powershell
# Get Advanced Find views (QueryType = 2)
Get-DataverseView -Connection $c -QueryType 2

# Get Lookup views (QueryType = 64)
Get-DataverseView -Connection $c -QueryType 64
```

Common query types:
- `0` — Other View
- `1` — Public View (default)
- `2` — Advanced Find
- `4` — Sub-Grid
- `64` — Lookup View
- `128` — Main Application View

## Deleting Views

### Delete by ID

```powershell
Remove-DataverseView -Connection $c -Id $viewId -Confirm:$false
```

### Delete with Confirmation

The cmdlet prompts for confirmation by default:

```powershell
Remove-DataverseView -Connection $c -Id $viewId
```

### Delete if Exists

Suppress errors if the view doesn't exist:

```powershell
Remove-DataverseView -Connection $c -Id $viewId -IfExists -Confirm:$false
```

### Delete System Views

Specify `-SystemView` when deleting system views:

```powershell
Remove-DataverseView -Connection $c -Id $viewId -SystemView -Confirm:$false
```

### Delete Multiple Views

Use the pipeline to delete multiple views:

```powershell
Get-DataverseView -Connection $c -Name "Test*" |
    Remove-DataverseView -Connection $c -Confirm:$false
```

## Control Flags

### NoCreate and NoUpdate

Control whether views are created or updated:

```powershell
# Only update if exists, don't create
Set-DataverseView -Connection $c -Id $viewId -Name "Updated" -NoCreate

# Only create if doesn't exist, don't update
Set-DataverseView -Connection $c -Id $viewId -Name "New View" -NoUpdate
```

### PassThru

Return the view ID after creation or update:

```powershell
$viewId = Set-DataverseView -Connection $c -PassThru `
    -Name "My View" `
    -TableName contact `
    -Columns @("firstname", "lastname")
```

## Best Practices

1. **Use meaningful names**: Choose descriptive names that indicate the view's purpose
2. **Start with personal views**: Test with personal views before creating system views
3. **Use simplified filters when possible**: Hashtable filters are easier to maintain than FetchXML
4. **Specify column widths**: Provide width configuration for better user experience
5. **Use WhatIf**: Preview changes with `-WhatIf` before updating production views
6. **Document complex filters**: Add comments explaining business logic in complex filter expressions
7. **Test thoroughly**: Verify views display correctly in the application before deploying to production

## Common Scenarios

### Clone a View

```powershell
# Get existing view
$originalView = Get-DataverseView -Connection $c -Id $originalViewId

# Create new view with same columns and filters
Set-DataverseView -Connection $c -PassThru `
    -Name "$($originalView.name) (Copy)" `
    -TableName $originalView.returnedtypecode `
    -FetchXml $originalView.fetchxml
```

### Create Views for Multiple Tables

```powershell
$tables = @("contact", "account", "lead")

foreach ($table in $tables) {
    Set-DataverseView -Connection $c -PassThru -SystemView `
        -Name "Active $table Records" `
        -TableName $table `
        -Columns @("createdon", "modifiedon") `
        -FilterValues @{ statecode = 0 }
}
```

### Audit View Configuration

```powershell
# Get all system views and export metadata
$views = Get-DataverseView -Connection $c -SystemView

$views | Select-Object name, returnedtypecode, ViewType, isdefault |
    Export-Csv -Path "view-audit.csv" -NoTypeInformation
```

## See Also

- [Querying Records](querying.md) — Understanding filters and FetchXML
- [Get-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseView.md)
- [Set-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseView.md)
- [Remove-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseView.md)
