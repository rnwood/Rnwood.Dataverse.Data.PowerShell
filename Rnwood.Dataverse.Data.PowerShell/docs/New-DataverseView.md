---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# New-DataverseView

## SYNOPSIS
Creates a new view (savedquery or userquery) in Dataverse.

## SYNTAX

### Simple
```
New-DataverseView -Name <String> -TableName <String> [-SystemView] [-Description <String>]
 -Columns <Object[]> [-FilterValues <Hashtable[]>] [-LayoutXml <String>] [-IsDefault]
 [-QueryType <Int32>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FetchXml
```
New-DataverseView -Name <String> -TableName <String> [-SystemView] [-Description <String>]
 -FetchXml <String> [-LayoutXml <String>] [-IsDefault] [-QueryType <Int32>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Creates a new view (savedquery for system views or userquery for personal views) in Dataverse. Views can be created using either:
- **Simple syntax**: Specify columns and filters using PowerShell-friendly hashtable syntax
- **FetchXml syntax**: Provide FetchXml directly for maximum flexibility

The cmdlet supports both system views (organization-owned) and personal views (user-owned). System views are visible to all users while personal views are only visible to the creating user.

**View Types (QueryType):**
- 0 = Other View
- 1 = Public View (default)
- 2 = Advanced Find
- 4 = SubGrid
- 8 = Dashboard
- 16 = Mobile Client View
- 64 = Lookup View
- 128 = Main Application View
- 256 = Quick Find Search
- 512 = Associated
- 1024 = Calendar View
- 2048 = Interactive Experience

## EXAMPLES

### Example 1: Create a personal view with simple filter
```powershell
PS C:\> New-DataverseView -Connection $c -Name "Active Contacts" -TableName contact `
    -Columns @("firstname", "lastname", "emailaddress1") `
    -FilterValues @{statecode = 0}
```

Creates a personal view showing first name, last name, and email for active contacts.

### Example 2: Create a system view with column widths
```powershell
PS C:\> New-DataverseView -Connection $c -Name "My Active Accounts" -TableName account `
    -SystemView `
    -Columns @(
        @{name="name"; width=200},
        @{name="accountnumber"; width=100},
        @{name="revenue"; width=150}
    ) `
    -FilterValues @{statecode = 0}
```

Creates a system view with customized column widths.

### Example 3: Create view with complex filter
```powershell
PS C:\> New-DataverseView -Connection $c -Name "Recent Opportunities" -TableName opportunity `
    -Columns @("name", "estimatedvalue", "createdon") `
    -FilterValues @{
        and = @(
            @{statecode = 0},
            @{or = @(
                @{estimatedvalue = @{value=10000; operator='GreaterThan'}},
                @{opportunityrating = 3}
            )}
        )
    }
```

Creates a view with nested AND/OR filter logic.

### Example 4: Create view using FetchXml
```powershell
PS C:\> $fetchXml = @"
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

PS C:\> New-DataverseView -Connection $c -Name "Contacts Created Last 30 Days" `
    -TableName contact -FetchXml $fetchXml
```

Creates a view using FetchXml for advanced query control.

### Example 5: Create a default system view
```powershell
PS C:\> New-DataverseView -Connection $c -Name "All Active Contacts" -TableName contact `
    -SystemView -IsDefault `
    -Columns @("fullname", "emailaddress1", "telephone1") `
    -FilterValues @{statecode = 0}
```

Creates a default system view for the contact entity.

### Example 6: Create an Advanced Find view
```powershell
PS C:\> New-DataverseView -Connection $c -Name "Custom Search View" -TableName contact `
    -QueryType 2 `
    -Columns @("firstname", "lastname", "company") `
    -FilterValues @{firstname = "John"}
```

Creates an Advanced Find view (QueryType = 2) for searching contacts.

## PARAMETERS

### -Name
Name of the view.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Logical name of the table this view is for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SystemView
Create a system view (savedquery) instead of a personal view (userquery).

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

### -Description
Description of the view.

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

### -Columns
Columns to include in the view. Can be an array of column names or hashtables with column configuration (name, width, etc.).

```yaml
Type: Object[]
Parameter Sets: Simple
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterValues
One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.

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
FetchXml query to use for the view.

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
Layout XML for the view. If not specified, a default layout will be generated from Columns.

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

### -QueryType
View type: 0=OtherView, 1=PublicView, 2=AdvancedFind, 4=SubGrid, 8=Dashboard, 16=MobileClientView, 64=LookupView, 128=MainApplicationView, 256=QuickFindSearch, 512=Associated, 1024=CalendarView, 2048=InteractiveExperience. Default is 1 (PublicView).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1
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

### None

## OUTPUTS

### System.Guid
Returns the ID of the created view.

## NOTES

## RELATED LINKS
- [Set-DataverseView](Set-DataverseView.md)
- [Remove-DataverseView](Remove-DataverseView.md)
- [Get-DataverseRecord](Get-DataverseRecord.md)
