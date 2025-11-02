---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseView

## SYNOPSIS
Creates or updates Dataverse views (savedquery and userquery entities) with flexible column and filter configuration.

## SYNTAX

```
Set-DataverseView [-Id <Guid>] [-Name <String>] [-TableName <String>] [-ViewType <String>]
 [-Description <String>] [-Columns <Object[]>] [-AddColumns <Object[]>] [-InsertBefore <String>]
 [-InsertAfter <String>] [-RemoveColumns <String[]>] [-UpdateColumns <Hashtable[]>]
 [-FilterValues <Hashtable[]>] [-FetchXml <String>] [-Links <DataverseLinkEntity[]>] [-LayoutXml <String>]
 [-IsDefault] [-QueryType <QueryType>] [-NoUpdate] [-NoCreate] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseView cmdlet creates new views or updates existing ones using an upsert pattern. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

The cmdlet supports both simplified syntax (using Columns and FilterValues parameters) and advanced FetchXML-based configuration. Column configurations can be specified as simple strings or detailed hashtables with width and other properties.

When updating existing views, you can add, remove, or update specific columns without affecting other view properties.

## EXAMPLES

### Example 1: Create a basic personal view
```powershell
PS C:\> Set-DataverseView -Connection $c -PassThru `
    -Name "My Active Contacts" `
    -TableName contact `
    -Columns @("firstname", "lastname", "emailaddress1", "telephone1") `
    -FilterValues @{ statecode = 0 }
```

Creates a personal view showing active contacts with specified columns.

### Example 2: Create a system view with column widths
```powershell
PS C:\> Set-DataverseView -Connection $c -PassThru -SystemView `
    -Name "All Active Contacts" `
    -TableName contact `
    -Columns @(
        @{ name = "firstname"; width = 100 },
        @{ name = "lastname"; width = 150 },
        @{ name = "emailaddress1"; width = 250 },
        @{ name = "telephone1"; width = 120 }
    ) `
    -FilterValues @{ statecode = 0 }
```

Creates a system view with specific column widths.

### Example 3: Update an existing view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -Name "Updated Contact View" `
    -Description "Shows active contacts with email"
```

Updates the name and description of an existing view.

### Example 4: Add columns to a view
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @(
        @{ name = "address1_city"; width = 150 },
        @{ name = "birthdate"; width = 100 }
    )
```

Adds new columns to an existing view.

### Example 5: Add columns at specific positions
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @("mobilephone", "fax") `
    -InsertAfter "telephone1"
```

Adds mobile phone and fax columns after the telephone1 column.

### Example 6: Insert columns before a specific column
```powershell
PS C:\> Set-DataverseView -Connection $c -Id $viewId `
    -AddColumns @("jobtitle") `
    -InsertBefore "emailaddress1"
```

Inserts the job title column before the email address column.

### Example 7: Clone a view using Columns from Get-DataverseView
```powershell
PS C:\> $originalView = Get-DataverseView -Connection $c -Id $originalViewId
PS C:\> Set-DataverseView -Connection $c -PassThru `
    -Name "$($originalView.name) (Copy)" `
    -TableName $originalView.returnedtypecode `
    -Columns $originalView.Columns `
    -FetchXml $originalView.fetchxml
```

Retrieves a view and creates a copy using the Columns property returned by Get-DataverseView.

### Example 8: Create view with complex filters
```powershell
PS C:\> Set-DataverseView -Connection $c -PassThru -SystemView `
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

Creates a view with nested logical filter expressions.

### Example 9: Use FetchXML for advanced queries
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

PS C:\> Set-DataverseView -Connection $c -PassThru `
    -Name "Recent Contacts" `
    -TableName contact `
    -FetchXml $fetchXml
```

Creates a view using FetchXML for complex query definitions.

## PARAMETERS

### -AddColumns
Columns to add to the view.
Can be an array of column names or hashtables with column configuration (name, width, etc.)
Use InsertBefore or InsertAfter parameters to control the position where columns are inserted.

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
Columns to include in the view.
Can be an array of column names (strings) or hashtables with column configuration.
When using hashtables, supported properties include:
- `name` (required): The logical name of the column
- `width`: Display width in pixels (optional, defaults vary by column type)

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -Description
Description of the view

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -FetchXml
FetchXml query to use for the view

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

### -FilterValues
One or more hashtables to filter records.
Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.
Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name).
Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'.
Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases: Filters

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Id
ID of the view to update.
If not specified or if the view doesn't exist, a new view is created.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsDefault
Set this view as the default view for the table

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -LayoutXml
Layout XML for the view.
If not specified when creating, a default layout will be generated from Columns

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

### -Name
Name of the view.
Required when creating a new view.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoCreate
If specified, then no view will be created even if no existing view matching the ID is found

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoUpdate
If specified, existing views matching the ID will not be updated

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, returns the ID of the created or updated view

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -QueryType
View type. Valid values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, ExportFieldTranslationsView, OutlookTemplate, AddressBookFilters, OutlookFilters, CopilotView.
Default is AdvancedSearch

```yaml
Type: QueryType
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -RemoveColumns
Columns to remove from the view

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Logical name of the table this view is for.
Required when creating a new view.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -UpdateColumns
Columns to update in the view.
Hashtables with column configuration (name, width, etc.)

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Links
Link entities to apply to the view query. Accepts DataverseLinkEntity objects or simplified hashtable syntax

```yaml
Type: DataverseLinkEntity[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -InsertAfter
Column name to insert new columns after.
Used with AddColumns parameter to specify the position where new columns should be inserted.
Cannot be used together with InsertBefore.

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

### -InsertBefore
Column name to insert new columns before.
Used with AddColumns parameter to specify the position where new columns should be inserted.
Cannot be used together with InsertAfter.

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

### -ViewType
Work with a system view (savedquery) instead of a personal view (userquery)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Guid
## NOTES

- Uses upsert pattern: creates new views if ID is not specified or view doesn't exist, updates existing views if ID matches.
- The `Columns` parameter accepts the same format as returned by `Get-DataverseView`, enabling easy view cloning.
- When updating existing views, only specified parameters are modified; other properties remain unchanged.
- System views (savedquery) are accessible to all users; personal views (userquery) are user-specific.
- Complex filters support nested logical expressions using 'and', 'or', 'not', and 'xor' keys.
- The cmdlet supports WhatIf and Confirm for safe operation.

## RELATED LINKS

[View Management Documentation](../../docs/core-concepts/view-management.md)

[Get-DataverseView](Get-DataverseView.md)

[Remove-DataverseView](Remove-DataverseView.md)

[Querying Records](../../docs/core-concepts/querying.md)
