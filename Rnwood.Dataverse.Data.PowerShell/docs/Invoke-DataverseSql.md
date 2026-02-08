---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSql

## SYNOPSIS
Invokes a Dataverse SQL query using Sql4Cds and writes any resulting rows to the pipeline.

## SYNTAX

```
Invoke-DataverseSql [-Sql] <String> [-UseTdsEndpoint] [-Timeout <Int32>] [-Parameters <PSObject>]
 [-BatchSize <Int32>] [-MaxDegreeOfParallelism <Int32>] [-BypassCustomPluginExecution] [-UseBulkDelete]
 [-ReturnEntityReferenceAsGuid] [-UseLocalTimezone] [-AdditionalConnections <Hashtable>]
 [-DataSourceName <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sql4Cds is a powerfull engine which can translate many SELECT, INSERT, UPDATE and DELETE Sql queries and execute them against Dataverse. This Cmdlet uses Sql4Cds to execute such queries.

If the query returns a result set, it will output to the pipeline with an object per row having a property per column in the result set.
If applicable (e.g. for UPDATE), the affected row count is written to verbose output.

`@parameters` in the query will have their values taken from the `Parameters` property. This can be from the pipeline to allow the query to be executed multiple times.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSql -sql "SELECT TOP 1 createdon FROM Contact WHERE lastname=@lastname" -parameters @{
lastname = "Wood"
}

createdon
---------
28/11/2024 16:28:12
```

Returns the rows from the SELECT query matching the @lastname parameter which is supplied.

### Example 2
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSql "SELECT TOP 1 createdon FROM Contact WHERE lastname='Wood'"

createdon
---------
28/11/2024 16:28:12
```

Demonstrates the simplified positional syntax where the SQL query can be specified without the `-Sql` parameter name. The SQL parameter is positional (position 0), allowing for more natural command line usage.

### Example 3
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @(
@{
lastname = "Wood"
},
@{
lastname = "Cat2"
}
) | Invoke-DataverseSql -sql "SELECT TOP 1 lastname, createdon FROM Contact WHERE lastname=@lastname"

lastname createdon
-------- ---------
Wood     28/11/2024 16:28:12
Cat2     28/11/2024 16:42:30
```

Returns the rows from the SELECT query matching the @lastname parameters which are supplied via the pipeline. The query is executed once for each of the pipeline objects.

### Example 4
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create connections to different environments
PS C:\> $primaryConnection = Get-DataverseConnection -url "https://primary.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret
PS C:\> $secondaryConnection = Get-DataverseConnection -url "https://secondary.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret

PS C:\> # Create hashtable with additional connections
PS C:\> $additionalConnections = @{
"secondary" = $secondaryConnection
}

PS C:\> # Execute cross-datasource query
PS C:\> Invoke-DataverseSql -AdditionalConnections $additionalConnections -Sql "
SELECT p.fullname AS primary_user, s.fullname AS secondary_user
FROM primary_org.systemuser p
CROSS JOIN secondary.systemuser s
WHERE p.domainname = s.domainname
"

primary_user           secondary_user
------------           --------------
John Smith             John Smith
Jane Doe               Jane Doe
```

Executes a cross-datasource query that joins data from two different Dataverse environments. The AdditionalConnections parameter allows registering named data sources that can be referenced in the SQL query using the syntax `datasource_name.table_name`.

### Example 5
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create connections with explicit data source names for repeatability
PS C:\> $devConnection = Get-DataverseConnection -url "https://dev-org.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret
PS C:\> $prodConnection = Get-DataverseConnection -url "https://prod-org.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret

PS C:\> # Use DataSourceName to ensure consistent naming across environments
PS C:\> $additionalConnections = @{
	"production" = $prodConnection
}

PS C:\> # Query uses explicit "primary" name instead of dev org's unique name
PS C:\> Invoke-DataverseSql -DataSourceName "primary" -AdditionalConnections $additionalConnections -Sql "
	SELECT p.fullname, prod.email
	FROM primary.systemuser p
	LEFT JOIN production.systemuser prod ON p.domainname = prod.domainname
"

fullname           email
--------           -----
John Smith         jsmith@company.com
Jane Doe           jdoe@company.com
```

Uses the DataSourceName parameter to assign a stable name ("primary") to the main connection. This ensures the SQL query works consistently across dev, test, and prod environments without modification, as the organization unique names differ between environments.

## PARAMETERS

### -AdditionalConnections
Additional data sources to register with Sql4Cds, allowing queries across multiple connections. This is a Hashtable where keys are data source names (strings) and values are ServiceClient connections.

When specified, these additional connections can be referenced in SQL queries using the syntax: `datasource_name.table_name`. This enables cross-datasource queries, such as joining data from multiple Dataverse environments.

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

### -BatchSize
Controls the batch size used by Sql4Cds.

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

### -BypassCustomPluginExecution
Bypasses custom plugins. See Sql4Cds docs.

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -DataSourceName
Specifies the name for the primary data source. If not specified, defaults to the organization unique name from the connection. Use this parameter to ensure consistent data source names across different environments for repeatable queries when using AdditionalConnections.

This is particularly useful when writing queries that need to work across different environments (dev, test, prod) where the organization unique names differ. By explicitly setting the data source name, your SQL queries remain portable.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None (uses organization unique name)
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxDegreeOfParallelism
Maximum number of threads to use. See Sql4Cds docs.

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

### -Parameters
Specifies values for the `@parameters` used in the Sql. This can be a Hashtable or any PSObject with properties.

This can be read from the pipeline to allow the query to be executed once per input object using different values.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### -ReturnEntityReferenceAsGuid
Returns lookup column values as simple Guid as opposed to SqlEntityReference type. See Sql4Cds docs.

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

### -Sql
SQL to execute. See Sql4Cds docs for supported queries. Can contain @parameters.

This parameter is positional (position 0), so you can omit the `-Sql` parameter name and simply provide the query string as the first argument (e.g., `Invoke-DataverseSql "SELECT * FROM contact"`).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Timeout
Timeout for query to execute. See Sql4Cds docs.

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

### -UseBulkDelete
Uses bulk delete for supported DELETE operations. See Sql4Cds docs.

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

### -UseLocalTimezone
When working with date values, this property indicates the local time zone should be used. See Sql4Cds docs.

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

### -UseTdsEndpoint
Let Sql4Cds use the TDS endpoint or not for compatible queries. The default is to not use this.

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run. Does not apply to read only queries.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES
A special thanks to Mark Carrington for his amazing open-source project that has enabled this.

## RELATED LINKS

[https://github.com/MarkMpn/Sql4Cds](https://github.com/MarkMpn/Sql4Cds)

