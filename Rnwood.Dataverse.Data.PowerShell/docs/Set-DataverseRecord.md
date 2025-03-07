---
external help file: Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecord

## SYNOPSIS
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

## SYNTAX

```
Set-DataverseRecord -Connection <ServiceClient> -InputObject <PSObject> [-BatchSize <UInt32>]
 -TableName <String> [-IgnoreProperties <String[]>] [-Id <Guid>] [-MatchOn <String[][]>] [-PassThru]
 [-NoUpdate] [-NoCreate] [-NoUpdateColumns <String[]>] [-CallerId <Guid>] [-UpdateAllColumns] [-CreateOnly]
 [-Upsert] [-LookupColumns <Hashtable>] [-BypassBusinessLogicExecution <BusinessLogicTypes[]>]
 [-BypassBusinessLogicExecutionStepIds <Guid[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

By default, this command will check for existing record matching the input object's primary key(s) and will update if there is a match and there are changes, or create if not. 

The `TableName` and `Id` properties will normally be read from the pipeine if present, but can be overriden as separate arguments.

See the various options below which can vary if/how the record is matched and if new records will be created or not.

Primitive types (text, number, yes/no) can be specified directly using the PowerShell numeric/string/bool types etc. A conversion will be attempted from other types.

Date and date/time columns can be specified directly using the PowerShell DateTime type. A conversion will be attempted from string and other types, but take care to include TZ offset.

Choice (option set), State and Status columns will accept any or:
- Numeric value of the choice
- Label
- $null or empty string

Lookup and UniqueIdentifier columns will accept any of:
 - the name of the target record (as long as it is unique)
 - the Id of a record in one of the tables the lookup targets
 - an object which has `TableName`/`LogicalName`/`EntityName` and `Id` properties. This includes `EntityReference` instances from the SDK and values as returned from `Get-DataverseRecord`.
 - $null or empty string.

Party list columns accept a collection of objects each of which need to be convertible to a `activityparty` table record using the rules above.

## EXAMPLES

### Example 1
```powershell
PS C:\> [PSCustomObject] @{"TableName"="contact"; "lastname"="Simpson"} | Set-DataverseRecord -connection $c
```

Creates a new contact record with a last name.

### Example 2
```powershell
PS C:\> Get-DataverseRecord -connection $c -columns statuscode | ForEach-Object { $_.statuscode = "Inactive"} | Set-DataverseRecord -connection $c
```

Retrieves all existing contacts and sets their status reason to `Inactive`.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnnection cmdlet,

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Object containing values to be used.
Property names must match the logical names of Dataverse fields in the specified entity and the property values are used to set the values of the Dataverse record being created/updated.
The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -BatchSize
Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.

When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. 

Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Logical name of entity

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName, LogicalName

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IgnoreProperties
List of properties on the input object which are ignored and not attemted to be mapped to the record.
Default is none.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Id
ID of record to be created or updated.

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

### -MatchOn
List of list of field names that identify an existing record to update based on the values of those fields in the InputObject.
For create/update these are used if a record with and Id matching the value of the Id cannot be found.
The first list that returns a match is used.
e.g.
("firstname", "lastname"), "fullname" will try to find an existing record based on the firstname AND listname from the InputObject and if not found it will try by fullname.
For upsert only a single list is allowed and it must match the properties of an alternate key defined on the table.

```yaml
Type: String[][]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, the InputObject is written to the pipeline with an Id property set indicating the primary key of the affected record (even if nothing was updated).

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
If specified existing records matching the ID and or MatchOn fields will not be updated.

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

### -NoCreate
If specified then no records will be created even if no existing records matching the ID and or MatchOn fields is found.

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

### -NoUpdateColumns
List of column names which will not be included when updating existing records.

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

### -CallerId
If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.

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

### -UpdateAllColumns
If specified an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided

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

### -CreateOnly
If specified, no check for existing record is made and records will always be attempted to be created.
Use this option when it's known that no existing matching records will exist to improve performance.
See the -noupdate option for an alternative.

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

### -Upsert
If specified, upsert request will be used to create/update existing records as appropriate.
-MatchOn is not supported with this option

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

### -LookupColumns
Hashset of lookup column name in the target entity to column name in the referred to entity with which to find the records.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

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

### -ProgressAction
See standard PS docs.

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

### -BypassBusinessLogicExecution
Specifies the types of business logic (for example plugins) to bypass

```yaml
Type: BusinessLogicTypes[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BypassBusinessLogicExecutionStepIds
Specifies the IDs of plugin steps to bypass

```yaml
Type: Guid[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS
