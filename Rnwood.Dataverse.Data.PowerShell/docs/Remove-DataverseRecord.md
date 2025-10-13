---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseRecord

## SYNOPSIS
Deletes an existing Dataverse record, including M:M association records.

## SYNTAX

```
Remove-DataverseRecord [-InputObject <PSObject>] -TableName <String> -Id <Guid> [-BatchSize <UInt32>]
 [-MaxDegreeOfParallelism <Int32>] [-IfExists] [-BypassBusinessLogicExecution <BusinessLogicTypes[]>]
 [-BypassBusinessLogicExecutionStepIds <Guid[]>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `TableName` and `Id` can be read from the pipeline when piping in a record obtained from `get-dataverserecord` instead of being specified separately. This allows you to delete a stream of records from the pipeline.

## EXAMPLES

### Example 1
```powershell
PS C:\> get-dataverserecord -connection $c -tablename contact | remove-dataverserecord -connection $c
```

Deletes all contact records.

### Example 2
```powershell
PS C:\> remove-dataverserecord -connection $c -tablename contact -id 4CE66D51-C605-4429-8565-8C7AFA4B9550
```

Deletes the single contact with the specified ID.

### Example 3: Handle errors in batch delete operations
```powershell
PS C:\> $recordsToDelete = Get-DataverseRecord -Connection $c -TableName contact -Filter @{ lastname = "TestUser" }

PS C:\> $errors = @()
PS C:\> $recordsToDelete | Remove-DataverseRecord -Connection $c -ErrorVariable +errors -ErrorAction SilentlyContinue

PS C:\> # Process any errors that occurred
PS C:\> foreach ($err in $errors) {
    Write-Host "Failed to delete record: $($err.TargetObject.Id)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

Demonstrates batch error handling for delete operations. When using batching (default BatchSize of 100), the cmdlet continues processing all records even if some deletions fail. Each error written to the error stream includes the original input object as the `TargetObject`, allowing you to correlate which record caused the error. Use `-ErrorVariable` to collect errors and `-ErrorAction SilentlyContinue` to prevent them from stopping execution.

### Example 4: Access full error details from server
```powershell
PS C:\> $recordsToDelete = Get-DataverseRecord -Connection $c -TableName contact -Top 10

PS C:\> $errors = @()
PS C:\> $recordsToDelete | Remove-DataverseRecord -Connection $c -ErrorVariable +errors -ErrorAction SilentlyContinue

PS C:\> # Access detailed error information from the server
PS C:\> foreach ($err in $errors) {
    # TargetObject contains the input record that failed to delete
    Write-Host "Failed to delete record: $($err.TargetObject.Id)"
    
    # Exception.Message contains full server response including:
    # - OrganizationServiceFault ErrorCode and Message
    # - TraceText with server-side trace details
    # - InnerFault details (if any)
    Write-Host "Full error details from server:"
    Write-Host $err.Exception.Message
    
    # You can also access individual components:
    Write-Host "Error category: $($err.CategoryInfo.Category)"
    Write-Host "Exception type: $($err.Exception.GetType().Name)"
}
```

Demonstrates how to access comprehensive error details from the Dataverse server when deleting records. The `Exception.Message` property contains the full server response formatted by the cmdlet, including the OrganizationServiceFault error code, message, server-side trace text, and any nested inner fault details. The `TargetObject` identifies which input record caused the deletion error, while the full error message provides all diagnostic information needed for troubleshooting server-side issues.

### Example 5: Stop on first delete error with BatchSize 1
```powershell
PS C:\> $recordsToDelete = Get-DataverseRecord -Connection $c -TableName contact -Top 100

PS C:\> try {
    $recordsToDelete | Remove-DataverseRecord -Connection $c -BatchSize 1 -ErrorAction Stop
} catch {
    Write-Host "Error deleting record: $($_.TargetObject.Id)"
    Write-Host "Remaining records were not deleted"
}
```

Disables batching by setting `-BatchSize 1`. With this setting, each record is deleted in a separate request, and execution stops immediately on the first error (when using `-ErrorAction Stop`). This is useful when you need to stop processing on the first failure rather than attempting to delete all records.

### Example 6: Remove M:M associations by deleting from intersect table
```powershell
PS C:\> # Find the intersect record to delete
PS C:\> $intersectRecord = Get-DataverseRecord -Connection $c -TableName "account_accounts" -FilterValues @{
    accountid = $account1Id
    accountid2 = $account2Id
}

PS C:\> # Delete the association
PS C:\> $intersectRecord | Remove-DataverseRecord -Connection $c
```

Removes a many-to-many association by deleting the corresponding record from the intersect table. This disassociates the two related entities without affecting the entities themselves.

### Example 7: Simple parallel delete with MaxDOP
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName contact -FilterValues @{ statecode = 1 } |
    Remove-DataverseRecord -Connection $c -MaxDOP 4 -BatchSize 50
```

Deletes all inactive contacts in parallel with 4 workers, each processing batches of 50 records. Much simpler than manual chunking and parallel loops.

### Example 8: High-throughput parallel deletion
```powershell
PS C:\> $recordsToDelete = Get-DataverseRecord -Connection $c -TableName systemjob -FilterValues @{
    statecode = 3  # Completed
    createdon = @{ operator = "lt"; value = (Get-Date).AddMonths(-6) }
}

PS C:\> $recordsToDelete | Remove-DataverseRecord -Connection $c -MaxDOP 8 -BatchSize 100 -Verbose
```

Deletes old system jobs in parallel with 8 workers. The cmdlet automatically:
- Chunks records into groups of (BatchSize × MaxDOP) = 800
- Spawns up to 8 parallel workers
- Clones connections for thread safety
- Executes batch delete requests per worker

### Example 9: MaxDOP with IfExists for safe cleanup
```powershell
PS C:\> $recordIds = @("guid1", "guid2", "guid3")

PS C:\> $recordIds | ForEach-Object { 
    [PSCustomObject]@{ TableName = "contact"; Id = $_ } 
} | Remove-DataverseRecord -Connection $c -MaxDOP 4 -IfExists -ErrorAction SilentlyContinue
```

Deletes records by ID in parallel, silently skipping any that don't exist. Useful for cleanup scripts where some records may already be deleted.

## PARAMETERS

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

### -BypassBusinessLogicExecution
Specifies the types of business logic (for example plugins) to bypass

```yaml
Type: BusinessLogicTypes[]
Parameter Sets: (All)
Aliases:
Accepted values: CustomSync, CustomAsync

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

### -Id
Id of record to process

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

### -IfExists
If specified, the cmdlet will not raise an error if the record does not exist.

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

### -InputObject
Record from pipeline. This allows piping in record to delete.

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

### -MaxDegreeOfParallelism
Maximum degree of parallelism for processing records. When set to a value greater than 1, records are automatically chunked (based on BatchSize) and processed in parallel with cloned connections for thread safety.

**Key behaviors:**
- Default is 1 (no parallelism - traditional sequential processing)
- When > 1: Records are buffered and processed in chunks of (BatchSize × MaxDegreeOfParallelism)
- Each parallel worker gets its own cloned connection
- Combines seamlessly with BatchSize to optimize throughput
- Much simpler than manual chunking + ForEach-Object -Parallel

**Performance tips:**
- Start with MaxDOP=4 and BatchSize=50-100 for most scenarios
- Increase MaxDOP for high-latency connections or high-volume deletions
- Monitor Dataverse throttling limits when increasing parallelism
- Use -Verbose to see parallel processing progress

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: MaxDOP

Required: False
Position: Named
Default value: 1
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Name of entity. For many-to-many relationships, specify the intersect table name (e.g., "listcontact", "account_accounts").

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### System.Management.Automation.PSObject
### System.String
### System.Guid
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
