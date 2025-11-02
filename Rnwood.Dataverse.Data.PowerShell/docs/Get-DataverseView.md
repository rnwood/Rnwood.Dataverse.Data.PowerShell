---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseView

## SYNOPSIS
Retrieves view information (savedquery or userquery) from a Dataverse environment.

## SYNTAX

```
Get-DataverseView [[-Id] <Guid>] [-Name <String>] [-TableName <String>] [-ViewType <String>]
 [-QueryType <QueryType>] [-Raw] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseView cmdlet retrieves view definitions from Dataverse environments. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

By default, the cmdlet returns parsed view information with Columns, Filters, and Links properties extracted from the FetchXML. Use the -RawXml parameter to return the raw FetchXML and LayoutXML instead.

Views can be filtered by ID, name (with wildcard support), table name, view type (system vs personal), and query type.

## EXAMPLES

### Example 1: Get a specific view by ID
```powershell
PS C:\> Get-DataverseView -Connection $c -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific view by its ID.

### Example 2: Get all views for a table
```powershell
PS C:\> Get-DataverseView -Connection $c -TableName contact
```

Retrieves all views (both system and personal) for the contact table.

### Example 3: Get system views only
```powershell
PS C:\> Get-DataverseView -Connection $c -SystemView
```

Retrieves all system views (savedquery entities).

### Example 4: Get personal views only
```powershell
PS C:\> Get-DataverseView -Connection $c -PersonalView
```

Retrieves all personal views (userquery entities).

### Example 5: Find views by name with wildcards
```powershell
PS C:\> Get-DataverseView -Connection $c -Name "Active*"
```

Finds all views whose names start with "Active".

### Example 6: Get views with raw XML
```powershell
PS C:\> Get-DataverseView -Connection $c -Id $viewId -RawXml
```

Retrieves a view with raw FetchXML and LayoutXML instead of parsed properties.

### Example 7: Get Advanced Find views
```powershell
PS C:\> Get-DataverseView -Connection $c -QueryType AdvancedFind
```

Retrieves all Advanced Find views.

## PARAMETERS

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

### -Id
The ID of the view to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
The name of the view to retrieve.
Supports wildcards.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -QueryType
View type to filter by. Valid values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, ExportFieldTranslationsView, OutlookTemplate, AddressBookFilters, OutlookFilters, CopilotView

```yaml
Type: QueryType
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Logical name of the table to retrieve views for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

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

### -Raw
Return raw values instead of display values

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

### -ViewType
Retrieve views of the specified type

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

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
