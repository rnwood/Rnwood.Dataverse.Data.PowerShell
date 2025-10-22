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
Remove-DataverseRecord [-InputObject <PSObject>] -TableName <String> [-Id <Guid>] [-MatchOn <String[][]>]
 [-AllowMultipleMatches] [-BatchSize <UInt32>] [-RetrievalBatchSize <UInt32>] [-IfExists]
 [-BypassBusinessLogicExecution <BusinessLogicTypes[]>] [-BypassBusinessLogicExecutionStepIds <Guid[]>]
 [-Retries <Int32>] [-InitialRetryDelay <Int32>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `TableName` and `Id` can be read from the pipeline when piping in a record obtained from `get-dataverserecord` instead of being specified separately. This allows you to delete a stream of records from the pipeline.

**Finding Records:**

Records can be identified for deletion in two ways:
- By `-Id` parameter: Specify the GUID of the record to delete
- By `-MatchOn` parameter: Specify column name(s) to match against values in the InputObject. The first matching set of columns that returns records is used.

When using `-MatchOn`, if multiple records match the criteria, an error is raised unless `-AllowMultipleMatches` is specified.

**Batch Operations:**

When multiple records are deleted, batching is automatically used to improve performance. The batch continues on error and any errors are reported once the batch has completed. Each error includes the input record for correlation.

**Retry Logic:**

Individual batch items can be automatically retried on failure using exponential backoff:
- Use `-Retries` parameter to specify the number of retry attempts (default is 0 - no retries)
- Use `-InitialRetryDelay` to set the initial delay in seconds before the first retry (default is 5s)
- Each subsequent retry doubles the delay time (exponential backoff)
- Records are added to the next batch after the delay period has elapsed
- If only retries are pending at the end of processing, the cmdlet will wait for the appropriate delay before retrying
- After all retries are exhausted, failures are written as errors as usual
- Retries are particularly useful for transient errors like throttling or temporary connectivity issues

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

### Example 7: Using retry logic for transient delete failures
```powershell
PS C:\> $recordsToDelete = Get-DataverseRecord -Connection $c -TableName contact -Filter @{ lastname = "TestUser" }

PS C:\> # Retry each failed delete up to 3 times with exponential backoff
PS C:\> $recordsToDelete | Remove-DataverseRecord -Connection $c -Retries 3 -InitialRetryDelay 500 -Verbose
```

Deletes multiple contacts with automatic retry on transient failures. Failed deletion requests will be retried up to 3 times with delays of 500ms, 1000ms, and 2000ms respectively. The `-Verbose` flag shows retry attempts and wait times.

### Example 8: Custom retry delay for rate limiting
```powershell
PS C:\> # Handle rate limiting with longer initial delay
PS C:\> $largeDataset | Remove-DataverseRecord -Connection $c -Retries 5 -InitialRetryDelay 2000
```

Deletes a large dataset with automatic retry configured for rate limiting scenarios. Uses 5 retry attempts with an initial 2-second delay, doubling on each retry (2s, 4s, 8s, 16s, 32s).

### Example 9: Delete record using MatchOn with single column
```powershell
PS C:\> @{ emailaddress1 = "user@example.com" } | Remove-DataverseRecord -Connection $c -TableName contact -MatchOn emailaddress1
```

Deletes a contact record by matching on the email address. If multiple contacts have the same email, an error is raised unless -AllowMultipleMatches is used.

### Example 10: Delete record using MatchOn with multiple columns
```powershell
PS C:\> @{ firstname = "John"; lastname = "Doe" } | Remove-DataverseRecord -Connection $c -TableName contact -MatchOn @("firstname", "lastname")
```

Deletes a contact record by matching on both firstname and lastname. This helps ensure you're deleting the correct record when names might not be unique individually.

### Example 11: Delete multiple matching records with AllowMultipleMatches
```powershell
PS C:\> @{ lastname = "TestUser" } | Remove-DataverseRecord -Connection $c -TableName contact -MatchOn lastname -AllowMultipleMatches
```

Deletes all contact records with the lastname "TestUser". The -AllowMultipleMatches switch allows deletion of multiple records that match the criteria.

### Example 12: Use multiple MatchOn criteria with fallback
```powershell
PS C:\> $record = @{ 
    emailaddress1 = "user@example.com"
    firstname = "John"
    lastname = "Doe"
}
PS C:\> $record | Remove-DataverseRecord -Connection $c -TableName contact -MatchOn @("emailaddress1"), @("firstname", "lastname")
```

Attempts to match first on emailaddress1, then falls back to matching on firstname+lastname if no email match is found. Uses the first matching set that returns records.

## PARAMETERS

### -AllowMultipleMatches
If specified, allows deletion of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.

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
Id of record to process. Either -Id or -MatchOn must be specified.

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

### -InitialRetryDelay
Initial delay in seconds before first retry. Subsequent retries use exponential backoff (delay doubles each time). Default is 5s.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 5
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

### -MatchOn
List of list of column names that identify records to delete based on the values of those columns in the InputObject. The first list that returns a match is used. If AllowMultipleMatches is not specified, an error will be raised if more than one record matches.

For example:
- `@("emailaddress1")` - Match on email address alone
- `@("firstname", "lastname")` - Match on both first and last name together
- `@("emailaddress1"), @("firstname", "lastname")` - Try email first, fall back to name if no email match

Either -Id or -MatchOn must be specified.

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

### -Retries
Number of times to retry each batch item on failure. Default is 0 (no retries). Each retry uses exponential backoff based on the `-InitialRetryDelay` parameter.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetrievalBatchSize
Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.

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
