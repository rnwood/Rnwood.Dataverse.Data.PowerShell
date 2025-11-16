
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
- **Default views**: Mark views as default for their table
- **Query types**: Support all Dataverse view types (MainApplicationView, AdvancedSearch, SubGrid, etc.)

## Creating Views

### Basic View Creation

Create a personal view with simple filters (default):

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
Set-DataverseView -Connection $c -PassThru `
    -Name "All Active Contacts" `
    -TableName contact `
    -ViewType "System" `
    -Columns @("fullname", "emailaddress1", "telephone1") `
    -FilterValues @{ statecode = 0 }
```

### Complex Filters

Use nested hashtables for complex logical expressions:

```powershell
Set-DataverseView -Connection $c -PassThru `
    -Name "High Value Opportunities" `
-TableName opportunity `
    -ViewType "System" `
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

### Using Link Entities

Create views with related data using link entities:

```powershell
# Using Links parameter with DataverseLinkEntity objects
Set-DataverseView -Connection $c -PassThru `
    -Name "Contacts with Accounts" `
  -TableName contact `
    -Columns @("firstname", "lastname", "emailaddress1") `
    -Links @(
        [PSCustomObject]@{
            LinkToEntityName = "account"
            LinkFromAttributeName = "parentcustomerid"
   LinkToAttributeName = "accountid"
EntityAlias = "account"
   Columns = @("name")
        }
    ) `
    -FilterValues @{ statecode = 0 }
```

### Custom Layout XML

For complete control over view layout, specify custom LayoutXML:

```powershell
$layoutXml = @"
<grid name="resultset" object="contact" jump="contactid" select="1" icon="1" preview="1">
<row name="result" id="contactid">
    <cell name="firstname" width="150" />
    <cell name="lastname" width="150" />
  <cell name="emailaddress1" width="200" />
  </row>
</grid>
"@

Set-DataverseView -Connection $c -PassThru `
    -Name "Custom Layout View" `
    -TableName contact `
    -FetchXml $fetchXml `
    -LayoutXml $layoutXml
```

## Updating Views

### Update with Upsert Pattern

Provide the view ID to update an existing view. You must also specify the ViewType:

```powershell
# Update view name and description
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -Name "Updated View Name" `
    -Description "Updated description"
```

### Add Columns

Add new columns to an existing view:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @(
     @{ name = "address1_city"; width = 150 },
     @{ name = "birthdate"; width = 100 }
    )
```

Add columns at specific positions:

```powershell
# Insert before a specific column
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @("jobtitle") `
    -InsertColumnsBefore "emailaddress1"

# Insert after a specific column
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @("mobilephone", "fax") `
    -InsertColumnsAfter "telephone1"
```

### Remove Columns

Remove columns from a view:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -RemoveColumns @("fax", "address1_line1")
```

### Update Column Properties

Change column widths or other properties:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
  -ViewType "Personal" `
    -UpdateColumns @(
        @{ name = "firstname"; width = 200 },
        @{ name = "emailaddress1"; width = 300 }
    )
```

### Update Filters

Change the view's filter criteria:

```powershell
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
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
    -ViewType "Personal" `
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
Get-DataverseView -Connection $c -ViewType "System"

# Personal views only
Get-DataverseView -Connection $c -ViewType "Personal"

# System views for a specific table
Get-DataverseView -Connection $c -TableName contact -ViewType "System"
```

### Filter by Query Type

Views have different types for different purposes:

```powershell
# Get Advanced Find views
Get-DataverseView -Connection $c -QueryType AdvancedSearch

# Get Lookup views
Get-DataverseView -Connection $c -QueryType LookupView

# Get Main Application views
Get-DataverseView -Connection $c -QueryType MainApplicationView
```

Common query types:
- `MainApplicationView` — Main Application View
- `AdvancedSearch` — Advanced Search (default for new views)
- `SubGrid` — Sub-Grid
- `QuickFindSearch` — Quick Find Search
- `LookupView` — Lookup View
- `Reporting` — Reporting View

### Get Raw Values

Retrieve views with raw attribute values instead of parsed properties:

```powershell
$view = Get-DataverseView -Connection $c -Id $viewId -Raw
# $view contains all raw attributes from the savedquery/userquery record
# Including fetchxml, layoutxml, querytype, etc.
```

By default, Get-DataverseView returns parsed properties:
- `Columns` — Array of column configurations with names and widths
- `Filters` — Parsed filter hashtables
- `Links` — Parsed link entity configurations
- `OrderBy` — Array of sort specifications

## Deleting Views

### Delete by ID

Delete a personal view (default):

```powershell
Remove-DataverseView -Connection $c -Id $viewId
```

### Delete with Confirmation

The cmdlet prompts for confirmation by default. Suppress with `-Confirm:$false`:

```powershell
Remove-DataverseView -Connection $c -Id $viewId -Confirm:$false
```

### Delete if Exists

Suppress errors if the view doesn't exist:

```powershell
Remove-DataverseView -Connection $c -Id $viewId -IfExists -Confirm:$false
```

### Delete System Views

Specify `-ViewType "System"` when deleting system views:

```powershell
Remove-DataverseView -Connection $c -Id $viewId -ViewType "System" -Confirm:$false
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
Set-DataverseView -Connection $c -Id $viewId `
    -ViewType "Personal" `
    -Name "Updated" `
  -NoCreate

# Only create if doesn't exist, don't update
Set-DataverseView -Connection $c `
    -Name "New View" `
    -TableName contact `
    -Columns @("firstname") `
    -NoUpdate
```

### PassThru

Return the view ID after creation or update:

```powershell
$viewId = Set-DataverseView -Connection $c -PassThru `
    -Name "My View" `
    -TableName contact `
    -Columns @("firstname", "lastname")
```

## Common Scenarios

### Clone a View

```powershell
# Get existing view
$originalView = Get-DataverseView -Connection $c -Id $originalViewId

# Create new view with same columns
# Note: Get-DataverseView returns Columns in the format expected by Set-DataverseView
Set-DataverseView -Connection $c -PassThru `
    -Name "$($originalView.Name) (Copy)" `
    -TableName $originalView.TableName `
    -ViewType $originalView.ViewType `
    -Columns $originalView.Columns `
    -FilterValues $originalView.Filters `
    -Links $originalView.Links `
    -OrderBy $originalView.OrderBy
```

The `Columns` property returned by `Get-DataverseView` contains the column configuration (name and width) in the same format accepted by `Set-DataverseView`, making it easy to clone or modify existing views.

### Create Views for Multiple Tables

```powershell
$tables = @("contact", "account", "lead")

foreach ($table in $tables) {
    Set-DataverseView -Connection $c -PassThru `
        -Name "Active $table Records" `
     -TableName $table `
        -ViewType "System" `
     -Columns @("createdon", "modifiedon") `
        -FilterValues @{ statecode = 0 }
}
```

### Audit View Configuration

```powershell
# Get all system views and export metadata
$views = Get-DataverseView -Connection $c -ViewType "System"

$views | Select-Object Name, TableName, ViewType, IsDefault, QueryType |
    Export-Csv -Path "view-audit.csv" -NoTypeInformation
```

## See Also

- [Querying Records](querying.md) — Understanding filters and FetchXML
- [Get-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseView.md)
- [Set-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseView.md)
- [Remove-DataverseView cmdlet reference](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseView.md)
