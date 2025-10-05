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

This cmdlet retrieves records from Dataverse tables using either QueryExpression (via simple parameters) or FetchXML.

- Filtering by ID, name, complex hashtable-based filters or custom SDK FilterExpression
- Column selection (including excluding specific columns)
- Ordering and pagination
- Lookup value handling (can return name or ID)
- System column exclusion by default
- Automatic paging through all results

Results are returned as PowerShell objects with properties matching the column names.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseRecord -connection $connection -tablename contact
```

Get all contacts returning all non-system columns.

### Example 2
```powershell
PS C:\> Get-DataverseRecord -connection $connection -tablename contact -columns firstname -filtervalues @{
	"firstname:Like" = "Rob%"
}
```

Get all contacts where firstname starts with 'Rob' and return the firstname column only.

### Example 3 (nested hashtable operator)
```powershell
PS C:\> Get-DataverseRecord -connection $connection -tablename contact -filtervalues @(
	@{
		age = @{
			value = 25
			operator = 'GreaterThan'
		}
	}
)
```

Find contacts with age greater than 25 by using a nested hashtable to specify operator and value.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

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
List of hashtables of field names/values to exclude. Values default to `Equal` (or `Null` when `$null` is supplied).
Each hashtable's entries are combined with AND to form a sub-filter.
Multiple hashtables are combined using `AND` by default (a record is excluded only if it matches all sub-filters); use `-ExcludeFilterOr` to combine them using `OR` instead (a record is excluded if it matches any sub-filter).

Examples:

- Default (AND):



-ExcludeFilterValues @(
	@{
		firstname = 'bob'
		age = 25
	},
	@{
		lastname = 'smith'
	}
)

Excludes only records matching `(firstname = 'bob' AND age = 25) AND (lastname = 'smith')`.

- With `-ExcludeFilterOr` (OR):



-ExcludeFilterValues @(
	@{
		firstname = 'bob'
		age = 25
	},
	@{
		lastname = 'smith'
	}
) -ExcludeFilterOr

Excludes records matching `(firstname = 'bob' AND age = 25) OR (lastname = 'smith')`.

```yaml
Type: Hashtable[]
Parameter Sets: Simple
Aliases: ExcludeFilter

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
One or more hashtables that define filters to apply to the query. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.

Filter keys may be `column` or `column:Operator` where `Operator` is a name from the `ConditionOperator` enum (for example `GreaterThan`, `NotEqual`).

Values may be:
- A literal (e.g. `"bob"`)
- An array (treated as `IN`)
- `$null` (treated as `IS NULL`)
- A nested hashtable in the form `@{ value = <value>; operator = '<OperatorName>' }` to explicitly specify operator and value (for example `@{ age = @{ value = 25; operator = 'GreaterThan' } }`).

When an operator is omitted a default `Equal` (or `Null`/`NotNull` for `$null`) is used. Valid operators are those exposed by the SDK's `ConditionOperator` enum; for full list see the SDK docs. The provided value types must match the column type expected by Dataverse for the chosen operator.

Example:



-FilterValues @(
	@{
		firstname = 'bob'
		age = 25
	},
	@{
		firstname = 'sue'
	}
)

=> `(firstname = 'bob' AND age = 25) OR (firstname = 'sue')`.

Group filters using a wrapping hashtable with an `and` or `or` key
to combine multiple sub-hashtables. This allows building complex
logical expressions with arbitrary nesting depth. The value of the
`and`/`or` key must be a hashtable or an array of hashtables.

Examples:
- Require both firstname='Rob' AND lastname='One':
	

-FilterValues @{
	    'and' = @(
	        @{
	            firstname = 'Rob'
	        },
	        @{
	            lastname = 'One'
	        }
	    )
	}

- Combine OR inside AND to match ( (firstname = 'Rob' OR firstname = 'Joe') AND lastname = 'One' ):
	

-FilterValues @{
	    'and' = @(
	        @{
	            'or' = @(
					@{
						firstname = 'Rob'
					},
					@{
						firstname = 'Joe'
					}
	            )
	        },
				@{
					lastname = 'One'
				}
	    )
	}

Grouped exclude filters work the same way when passed to `-ExcludeFilterValues`.

Examples for exclude filters:

Exclude records where firstname is 'Rob':



-ExcludeFilterValues @{
	'not' = @{
		firstname = 'Rob'
	}
}

Exclude records where exactly one of several alternatives matches (XOR):



-ExcludeFilterValues @{
	'xor' = @(
		@{
			firstname = 'Rob'
		},
		@{
			firstname = 'Joe'
		}
	)
}

Exclude where (firstname = 'Rob' AND lastname = 'One') OR lastname = 'Smith':



-ExcludeFilterValues @{
	'or' = @(
		@{
			firstname = 'Rob'
			lastname = 'One'
		},
		@{
			lastname = 'Smith'
		}
	)
}

Examples showing include + exclude together:

Include contacts with lastname 'One' or 'Two' but exclude where exactly one of email vs mobile exists (XOR exclusion):



-FilterValues @{
	'or' = @(
		@{
			lastname = 'One'
		},
		@{
			lastname = 'Two'
		}
	)
} -ExcludeFilterValues @{
	'xor' = @(
		@{
			emailaddress1 = @{
				operator = 'NotNull'
			}
		},
		@{
			mobilephone = @{
				operator = 'NotNull'
			}
		}
	)
}

Warning: When using `xor` in an exclude filter the cmdlet must construct the complement (NOT XOR) which can expand combinatorially. To prevent excessive expansion the cmdlet enforces a maximum of 8 items inside an `xor` group; supplying more items will throw an error. For large alternative lists use FetchXML or SQL instead.

```yaml
Type: Hashtable[]
Parameter Sets: Simple
Aliases: IncludeFilter

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
Include system columns in output. By default system columns are excluded; use this switch to include them. Ignored if `-Columns` parameter is used.

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
Link entities to apply to query. Accepts `DataverseLinkEntity` objects (a lightweight wrapper around the SDK `LinkEntity` that allows easier pipeline usage and serialization). `DataverseLinkEntity` supports implicit conversion from `LinkEntity`.

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
