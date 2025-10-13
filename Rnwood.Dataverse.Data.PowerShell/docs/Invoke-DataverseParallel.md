# Invoke-DataverseParallel

## SYNOPSIS
Processes input objects in parallel using chunked batches with cloned Dataverse connections.

## SYNTAX

```
Invoke-DataverseParallel [-ScriptBlock] <ScriptBlock> -InputObject <PSObject> [-ChunkSize <Int32>]
 [-MaxDegreeOfParallelism <Int32>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Invoke-DataverseParallel` cmdlet processes input objects in parallel using multiple runspaces. It automatically chunks the input data, clones the Dataverse connection for each parallel worker, and makes the cloned connection available as the default connection within each script block.

This cmdlet is useful for improving performance when processing large numbers of records where the operations are independent and can be executed concurrently.

Key features:
- Automatic chunking of input data for efficient batch processing
- Connection cloning to ensure thread-safe parallel execution
- Default connection parameter injection for convenience
- Works on both PowerShell 5.1 and PowerShell 7+

## EXAMPLES

### Example 1: Update records in parallel
```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET

# Get records and update them in parallel
Get-DataverseRecord -Connection $connection -TableName contact -Top 1000 |
  Invoke-DataverseParallel -Connection $connection -ChunkSize 50 -MaxDegreeOfParallelism 8 -ScriptBlock {
    # $_ is the current input object
    # The cloned connection is automatically available
    $_.emailaddress1 = "updated-$($_.contactid)@example.com"
    Set-DataverseRecord -TableName contact -InputObject $_ -UpdateOnly
  }
```

Updates 1000 contact records in parallel with 8 concurrent workers, processing 50 records per chunk.

### Example 2: Query related records in parallel
```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET

# Get account IDs and query their contacts in parallel
$accountIds = 1..100 | ForEach-Object { [Guid]::NewGuid() }

$accountIds | Invoke-DataverseParallel -Connection $connection -ChunkSize 10 -ScriptBlock {
  # Query contacts for this account
  Get-DataverseRecord -TableName contact -Filter "parentcustomerid eq $_" |
    Select-Object contactid, fullname, emailaddress1
}
```

Queries contacts for 100 accounts in parallel, processing 10 accounts per chunk.

### Example 3: Call WhoAmI in parallel (testing connection cloning)
```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET

# Execute WhoAmI 10 times in parallel to verify connection cloning works
1..10 | Invoke-DataverseParallel -Connection $connection -ChunkSize 3 -ScriptBlock {
  $whoami = Get-DataverseWhoAmI
  [PSCustomObject]@{
    Item = $_
    UserId = $whoami.UserId
    OrganizationId = $whoami.OrganizationId
  }
}
```

Executes 10 WhoAmI calls in parallel, demonstrating that each parallel worker has a properly cloned connection.

### Example 4: Simple data transformation
```powershell
$connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET

# Process simple data in parallel
1..100 | Invoke-DataverseParallel -Connection $connection -ChunkSize 20 -ScriptBlock {
  # Perform some calculation
  $_ * 2
}
```

Processes 100 integers in parallel, doubling each value.

## PARAMETERS

### -ChunkSize
Number of input objects to process in each parallel batch. Default is 50.

Larger chunk sizes reduce overhead but increase memory usage. Smaller chunks provide better load balancing but increase coordination overhead. A good starting point is to match your BatchSize parameter on Set-DataverseRecord or Remove-DataverseRecord.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 50
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Dataverse connection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

The connection will be cloned for each parallel worker to ensure thread-safe execution. The cloned connection is automatically set as the default connection for Dataverse cmdlets within each script block.

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

### -InputObject
Input objects to process in parallel. Accepts pipeline input.

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

### -MaxDegreeOfParallelism
Maximum number of parallel operations. Default is the number of processors.

This controls how many chunks can be processed concurrently. Be careful not to set this too high as it may overwhelm the Dataverse service and trigger throttling. A good starting point is between 4 and 8 for most scenarios.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Environment.ProcessorCount
Accept pipeline input: False
Accept wildcard characters: False
```

### -ScriptBlock
Script block to execute for each input object. The current input object is available as `$_` within the script block.

The cloned Dataverse connection is automatically available as the default connection, so you don't need to specify `-Connection` on Dataverse cmdlets unless you want to use a different connection.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject
Accepts any object from the pipeline.

## OUTPUTS

### System.Management.Automation.PSObject
Returns the output from the script block for each input object.

## NOTES
- Parallel processing introduces overhead. For small numbers of records (< 100), sequential processing may be faster.
- Connection cloning requires a valid cloud-based Dataverse connection. Mock connections used in testing do not support cloning.
- Be mindful of Dataverse service protection limits when setting MaxDegreeOfParallelism. Too many concurrent requests may trigger throttling.
- Each parallel worker operates in an isolated runspace, so variables from the parent scope are not directly accessible. Use `$using:variableName` syntax if you need to reference parent variables.

## RELATED LINKS

[Get-DataverseConnection](Get-DataverseConnection.md)

[Get-DataverseRecord](Get-DataverseRecord.md)

[Set-DataverseRecord](Set-DataverseRecord.md)

[Remove-DataverseRecord](Remove-DataverseRecord.md)

[ForEach-Object](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/foreach-object)
