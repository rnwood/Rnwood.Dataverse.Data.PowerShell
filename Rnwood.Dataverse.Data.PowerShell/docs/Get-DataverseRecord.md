---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseRecord

## SYNOPSIS
Retrieves records from Dataverse tables using a variety of strategies to specify what should be retrieved.

## SYNTAX

### Simple
```
Get-DataverseRecord -Connection <ServiceClient> [-TableName] <String> [-VerboseRecordCount] [-RecordCount]
 [-FilterValues <Hashtable[]>] [-Criteria <FilterExpression>] [-Links <DataverseLinkEntity[]>]
 [-ExcludeFilterValues <Hashtable[]>] [-ExcludeFilterOr] [-ActiveOnly] [-Id <Guid[]>] [-Name <String[]>]
 [-ExcludeId <Guid[]>] [-Columns <String[]>] [-ExcludeColumns <String[]>] [-OrderBy <String[]>] [-Top <Int32>]
 [-PageSize <Int32>] [-LookupValuesReturnName] [-IncludeSystemColumns] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### FetchXml
```
Get-DataverseRecord -Connection <ServiceClient> [-VerboseRecordCount] [-RecordCount] [-FetchXml <String>]
 [-Top <Int32>] [-PageSize <Int32>] [-LookupValuesReturnName] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseRecord -connection $connection -tablename contact
```

Get all contacts returning all non-system columns.

### Example 2
```powershell
PS C:\> Get-DataverseRecord -connection $connection -tablename contact -columns firstname -filtervalues @{"firstname:Like"="Rob%"}
```

Get all contacts where firstname starts with 'Rob' and return the firstname column only.

## PARAMETERS

### -ActiveOnly
If specified only active records (statecode=0 or isactive=true) will be output

```yaml
Type: SwitchParameter
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnnection cmdlet.

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

### -Criteria
Extra criteria to apply to query. This is specified using the Dataverse SDK type `FilterExpression`.

```yaml
Type: FilterExpression
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExcludeColumns
List of columns to exclude from records (default is none). Ignored if Columns parameter is used.

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

### -ExcludeFilterOr
If specified the exclude filters will be logically combined using OR instead of the default of AND

```yaml
Type: SwitchParameter
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExcludeFilterValues
List of hashsets of fields names,values to filter records by using an NOTEQUALS condition (or ISNOTNULL if null value).
If more than one hashset is provided then they are logically combined using an AND condition by default.
e.g.
@{firstname="bob", age=25}, @{firstname="sue"} will find records where (firstname\<\>bob AND age\<\>25) OR (firstname\<\>sue)

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

### -ExcludeId
List of record ids to exclude

```yaml
Type: Guid[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FetchXml
FetchXml to use. See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/fetchxml/overview

```yaml
Type: String
Parameter Sets: FetchXml
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilterValues
List of hashsets of @{"fieldname"="value"} or @{"fieldname:operator"="value"} to filter records by. If operator is not provided, uses an EQUALS condition (or ISNULL if null value).
If more than one hashset is provided then they are logically combined using an OR condition.
e.g.
@{firstname="bob", age=25}, @{firstname="sue"} will find records where (firstname=bob AND age=25) OR (firstname=sue)

Valid operators:
Equal, NotEqual, GreaterThan, LessThan, GreaterEqual, LessEqual, Like, NotLike, In, NotIn, Between, NotBetween, Null, NotNull, Yesterday, Today, Tomorrow, Last7Days, Next7Days, LastWeek, ThisWeek, NextWeek, LastMonth, ThisMonth, NextMonth, On, OnOrBefore, OnOrAfter, LastYear, ThisYear, NextYear, LastXHours, NextXHours, LastXDays, NextXDays, LastXWeeks, NextXWeeks, LastXMonths, NextXMonths, LastXYears, NextXYears, EqualUserId, NotEqualUserId, EqualBusinessId, NotEqualBusinessId, ChildOf, Mask, NotMask, MasksSelect, Contains, DoesNotContain, EqualUserLanguage, NotOn, OlderThanXMonths, BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, ThisFiscalYear, ThisFiscalPeriod, NextFiscalYear, NextFiscalPeriod, LastFiscalYear, LastFiscalPeriod, LastXFiscalYears, LastXFiscalPeriods, NextXFiscalYears, NextXFiscalPeriods, InFiscalYear, InFiscalPeriod, InFiscalPeriodAndYear, InOrBeforeFiscalPeriodAndYear, InOrAfterFiscalPeriodAndYear, EqualUserTeams, EqualUserOrUserTeams, Under, NotUnder, UnderOrEqual, Above, AboveOrEqual, EqualUserOrUserHierarchy, EqualUserOrUserHierarchyAndTeams, OlderThanXYears, OlderThanXWeeks, OlderThanXDays, OlderThanXHours, OlderThanXMinutes, ContainValues, DoesNotContainValues, EqualRoleBusinessId

The type of value must use those expected by the SDK for the column type and operator.

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

### -Id
List of primary keys (IDs) of records to retrieve.

```yaml
Type: Guid[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeSystemColumns
Excludes system columns from output. Default is all columns except system columns. Ignored if Columns parameter is used.

```yaml
Type: SwitchParameter
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Links
Link entities to apply to query. Specified using the Dataverse SDK type `LinkEntity`

```yaml
Type: DataverseLinkEntity[]
Parameter Sets: Simple
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LookupValuesReturnName
Outputs Names for lookup values.
The default behaviour is to output the ID.

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

### -Name
List of names (primary attribute value) of records to retrieve.

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

### -OrderBy
List of fields to order records by.
Suffix field name with - to sort descending.
e.g "age-", "lastname" will sort by age descending then lastname ascending

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

### -PageSize
Number of records to request per page.
Default is 1000.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecordCount
If set writes total record count matching query to output output instead of results

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

### -TableName
Logical name of entity for which to retrieve records

```yaml
Type: String
Parameter Sets: Simple
Aliases: EntityName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Top
Number of records to limit result to.
Default is all results.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerboseRecordCount
If set writes total record count matching query to verbose output

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Collections.Generic.IEnumerable`1[[System.Management.Automation.PSObject, System.Management.Automation, Version=7.4.6.500, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]
## NOTES

## RELATED LINKS
