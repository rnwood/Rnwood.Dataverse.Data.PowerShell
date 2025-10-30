---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseView

## SYNOPSIS
Modifies an existing view (savedquery or userquery) in Dataverse.

## SYNTAX

### Simple
```
Set-DataverseView -Id <Guid> [-SystemView] [-Name <String>] [-Description <String>]
 [-AddColumns <Object[]>] [-RemoveColumns <String[]>] [-UpdateColumns <Hashtable[]>]
 [-FilterValues <Hashtable[]>] [-LayoutXml <String>] [-IsDefault]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### FetchXml
```
Set-DataverseView -Id <Guid> [-SystemView] [-Name <String>] [-Description <String>]
 -FetchXml <String> [-LayoutXml <String>] [-IsDefault]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Modifies an existing view (savedquery for system views or userquery for personal views) in Dataverse. You can:
- Update view metadata (name, description)
- Add, remove, or update columns
- Modify filters
- Replace FetchXml and LayoutXml
- Set as default view (system views only)

The cmdlet supports both system views and personal views. Use the `-SystemView` switch when modifying system views.

**Column Operations:**
- **Add Columns**: Use `-AddColumns` with column names or hashtables containing column configuration
- **Remove Columns**: Use `-RemoveColumns` with an array of column names to remove
- **Update Columns**: Use `-UpdateColumns` with hashtables containing column name and new properties (e.g., width)

**Filter Operations:**
When using `-FilterValues`, the existing filter is replaced with the new filter specification.

## EXAMPLES

### Example 1: Update view name
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId -Name "Updated View Name"
```

Updates the name of an existing view.

### Example 2: Add columns to a view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @("telephone1", "fax")
```

Adds telephone and fax columns to the view.

### Example 3: Add columns with custom widths
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @(
        @{name="telephone1"; width=150},
        @{name="emailaddress1"; width=250}
    )
```

Adds columns with customized column widths.

### Example 4: Remove columns from a view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -RemoveColumns @("fax", "telephone2")
```

Removes specific columns from the view.

### Example 5: Update column properties
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -UpdateColumns @(
        @{name="firstname"; width=200},
        @{name="lastname"; width=200}
    )
```

Updates the width of existing columns in the view.

### Example 6: Update view filter
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -FilterValues @{
        and = @(
            @{statecode = 0},
            @{createdon = @{operator='LastXDays'; value=30}}
        )
    }
```

Replaces the view's filter with a new filter showing active records created in the last 30 days.

### Example 7: Replace view query with FetchXml
```powershell
PS C:\> $newFetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="fullname" />
    <attribute name="emailaddress1" />
    <attribute name="telephone1" />
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
    </filter>
    <order attribute="fullname" descending="false" />
  </entity>
</fetch>
"@

PS C:\> Set-DataverseView -Connection $c -Id $viewId -FetchXml $newFetchXml
```

Replaces the entire view query with custom FetchXml.

### Example 8: Multiple modifications in one call
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -Name "Updated Active Contacts" `
    -Description "Shows all active contacts with recent activity" `
    -AddColumns @("lastusedincampaign") `
    -RemoveColumns @("fax") `
    -FilterValues @{statecode = 0}
```

Performs multiple modifications to the view in a single operation.

### Example 9: Set as default view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId -SystemView -IsDefault
```

Sets a system view as the default view for its entity.

### Example 10: Modify system view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $systemViewId -SystemView `
    -Name "All Active Accounts" `
    -AddColumns @("revenue", "numberofemployees")
```

Modifies a system view by updating its name and adding columns.

## PARAMETERS

### -Id
ID of the view to modify.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SystemView
Modify a system view (savedquery) instead of a personal view (userquery).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
New name for the view.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
New description for the view.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AddColumns
Columns to add to the view. Can be an array of column names or hashtables with column configuration (name, width, etc.).

```yaml
Type: Object[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoveColumns
Columns to remove from the view.

```yaml
Type: String[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UpdateColumns
Columns to update in the view. Hashtables with column configuration (name, width, etc.).

```yaml
Type: Hashtable[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterValues
Filter values to add or replace in the view. One or more hashtables to filter records.

```yaml
Type: Hashtable[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FetchXml
FetchXml query to replace the view's query.

```yaml
Type: String
Parameter Sets: FetchXml
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LayoutXml
Layout XML to replace the view's layout.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsDefault
Set this view as the default view for the table. Only applicable for system views.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Dataverse connection to use for the operation.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
The ID of the view can be provided via pipeline.

## OUTPUTS

### None

## NOTES
If no modifications are specified, a warning is displayed and the view is not updated.

## RELATED LINKS
- [New-DataverseView](New-DataverseView.md)
- [Remove-DataverseView](Remove-DataverseView.md)
- [Get-DataverseRecord](Get-DataverseRecord.md)
