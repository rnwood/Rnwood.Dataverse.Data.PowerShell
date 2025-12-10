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
Invoke-DataverseSql -Sql <String> [-UseTdsEndpoint] [-Timeout <Int32>] [-Parameters <PSObject>]
 [-BatchSize <Int32>] [-MaxDegreeOfParallelism <Int32>] [-BypassCustomPluginExecution] [-UseBulkDelete]
 [-ReturnEntityReferenceAsGuid] [-UseLocalTimezone] [-AdditionalConnections <Hashtable>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sql4Cds is a powerfull engine which can translate many SELECT, INSERT, UPDATE and DELETE Sql queries and execute them against Dataverse. This Cmdlet uses Sql4Cds to execute such queries.

If the query returns a result set, it will output to the pipeline with an object per row having a property per column in the result set.
If applicable (e.g. for UPDATE), the affected row count is written to verbose output.

`@parameters` in the query will have their values taken from the `Parameters` property. This can be from the pipeline to allow the query to be executed multiple times.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSql -connection $connection -sql "SELECT TOP 1 createdon FROM Contact WHERE lastname=@lastname" -parameters @{
	lastname = "Wood"
}

createdon
---------
28/11/2024 16:28:12
```

Returns the rows from the SELECT query matching the @lastname parameter which is supplied.

### Example 2
```powershell
PS C:\> @(
)	@{
		lastname = "Wood"
	},
	@{
		lastname = "Cat2"
	}
) | Invoke-DataverseSql -connection $c -sql "SELECT TOP 1 lastname, createdon FROM Contact WHERE lastname=@lastname"

lastname createdon
-------- ---------
Wood     28/11/2024 16:28:12
Cat2     28/11/2024 16:42:30
```

Returns the rows from the SELECT query matching the @lastname parameters which are supplied via the pipeline. The query is executed once for each of the pipeline objects.

### Example 3
```powershell
PS C:\> # Create connections to different environments
PS C:\> $primaryConnection = Get-DataverseConnection -url "https://primary.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret
PS C:\> $secondaryConnection = Get-DataverseConnection -url "https://secondary.crm.dynamics.com" -ClientId $clientId -ClientSecret $secret

PS C:\> # Create hashtable with additional connections
PS C:\> $additionalConnections = @{
	"secondary" = $secondaryConnection
}

PS C:\> # Execute cross-datasource query
PS C:\> Invoke-DataverseSql -Connection $primaryConnection -AdditionalConnections $additionalConnections -Sql "
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

## PARAMETERS

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

