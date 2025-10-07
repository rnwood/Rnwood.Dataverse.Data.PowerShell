---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecord

## SYNOPSIS
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

## SYNTAX

```
Set-DataverseRecord -InputObject <PSObject> -TableName <String> [-BatchSize <UInt32>]
 [-IgnoreProperties <String[]>] [-Id <Guid>] [-MatchOn <String[][]>] [-PassThru] [-NoUpdate] [-NoCreate]
 [-NoUpdateColumns <String[]>] [-CallerId <Guid>] [-UpdateAllColumns] [-CreateOnly] [-Upsert]
 [-LookupColumns <Hashtable>] [-BypassBusinessLogicExecution <BusinessLogicTypes[]>]
 [-BypassBusinessLogicExecutionStepIds <Guid[]>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

By default, this command will check for existing record matching the input object's primary key(s) and will update if there is a match and there are changes, or create if not. 

The `TableName` and `Id` properties will normally be read from the pipeine if present, but can be overriden as separate arguments.

See the various options below which can vary if/how the record is matched and if new records will be created or not.

**Data Type Conversion:**

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

**Batch Operations:**

When multiple records are passed through the pipeline or provided in a single invocation, this cmdlet automatically uses `ExecuteMultipleRequest` to batch operations for improved performance. 

Key Batch Behavior:
- Default batch size is 100 records per request (configurable via `-BatchSize` parameter)
- Set `-BatchSize 1` to disable batching and send one request at a time
- Batching continues on error - all operations are attempted and errors are reported at the end
- Each error includes the input record for correlation
- For best performance with `-CallerId`, sort records by caller ID since a new batch is needed when the caller changes

How Create vs Update is Determined:

The cmdlet follows this decision logic for each record:

1. **With `-Upsert` switch:**
   - Uses `UpsertRequest` which lets Dataverse decide based on alternate keys
   - No check for existing record is performed
   - Dataverse creates if no match, updates if match found
   - `-MatchOn` must specify an alternate key defined on the table

2. **With `-CreateOnly` switch:**
   - Always attempts to create new records
   - No check for existing record is performed
   - Use when you know records don't exist (best performance)

3. **With `-NoUpdate` switch:**
   - Checks for existing records but will not update them
   - Only creates new records when no match found

4. **With `-NoCreate` switch:**
   - Checks for existing records but will not create them
   - Only updates records when match found

5. **Default behavior (no switches):**
   - Checks for existing record by:
     - Primary ID if provided via `-Id` parameter or `Id` property
     - `-MatchOn` columns if specified (tries each match set in order)
   - If existing record found:
     - Retrieves current values
     - Removes unchanged columns (unless `-UpdateAllColumns` specified)
     - Sends `UpdateRequest` if changes detected
     - Skips update if nothing changed
   - If no existing record found:
     - Sends `CreateRequest` for new record

After create/update, additional operations are batched if needed:
- Assignment (`AssignRequest`) if `ownerid` column is set
- Status change (`SetStateRequest`) if `statecode` or `statuscode` columns are set

## EXAMPLES

### Example 1: Create a single record
```powershell
PS C:\> [PSCustomObject] @{
	TableName = "contact"
	lastname = "Simpson"
	firstname = "Homer"
} | Set-DataverseRecord -Connection $c
```

Creates a new contact record with a last name and first name.

### Example 2: Update existing records
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName contact -Columns statuscode | 
    ForEach-Object { $_.statuscode = "Inactive" } | 
    Set-DataverseRecord -Connection $c
```

Retrieves all existing contacts and sets their status reason to `Inactive`.

### Example 3: Batch create multiple records
```powershell
PS C:\> $contacts = @(
    @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
    @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
    @{ firstname = "Bob"; lastname = "Johnson"; emailaddress1 = "bob@example.com" }
)

PS C:\> $contacts | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly
```

Creates multiple contact records in a single batch. The `-CreateOnly` switch improves performance by skipping the existence check since we know these are new records. By default, all 3 records will be sent in a single ExecuteMultipleRequest.

### Example 4: Batch update with choice columns
```powershell
PS C:\> $accounts = Get-DataverseRecord -Connection $c -TableName account -Top 50

PS C:\> $accounts | ForEach-Object { 
    $_.industrycode = "Retail"  # Can use label name
    $_.accountratingcode = 1     # Or numeric value
} | Set-DataverseRecord -Connection $c
```

Retrieves 50 accounts and updates their industry (using label) and rating (using numeric value). Choice/option set columns accept either the numeric value or the display label.

### Example 5: Batch operations with state/status changes
```powershell
PS C:\> $cases = Get-DataverseRecord -Connection $c -TableName incident -Filter @{ statecode = 0 }

PS C:\> $cases | ForEach-Object {
    $_.statuscode = "Resolved"  # Can use status label
    $_.statecode = 1             # Or numeric state value
} | Set-DataverseRecord -Connection $c
```

Closes all active cases by setting their state and status. The cmdlet automatically handles state/status changes as separate SetStateRequest operations after the main update.

### Example 6: Upsert with alternate key
```powershell
PS C:\> @(
    @{ emailaddress1 = "user1@example.com"; firstname = "Alice"; lastname = "Anderson" }
    @{ emailaddress1 = "user2@example.com"; firstname = "Bob"; lastname = "Brown" }
) | Set-DataverseRecord -Connection $c -TableName contact -Upsert -MatchOn emailaddress1
```

Uses upsert operation with alternate key on emailaddress1. If a contact with the email exists, it will be updated; otherwise a new one is created. Requires that `emailaddress1` is defined as an alternate key on the contact table.

### Example 7: Control batch size
```powershell
PS C:\> $records = 1..500 | ForEach-Object {
    @{ name = "Account $_"; telephone1 = "555-$_" }
}

PS C:\> $records | Set-DataverseRecord -Connection $c -TableName account -BatchSize 50 -CreateOnly
```

Creates 500 records in batches of 50. This will result in 10 ExecuteMultipleRequest calls, each containing 50 CreateRequest operations.

### Example 8: Disable batching for detailed error handling
```powershell
PS C:\> $records | Set-DataverseRecord -Connection $c -TableName contact -BatchSize 1
```

Disables batching by setting BatchSize to 1. Each record is sent in a separate request, which stops immediately on first error rather than continuing through the batch.

### Example 9: MatchOn with multiple columns
```powershell
PS C:\> @(
    @{ firstname = "John"; lastname = "Doe"; telephone1 = "555-0001" }
    @{ firstname = "Jane"; lastname = "Smith"; telephone1 = "555-0002" }
) | Set-DataverseRecord -Connection $c -TableName contact -MatchOn ("firstname", "lastname")
```

Looks for existing contacts matching BOTH firstname AND lastname. If found, updates them; otherwise creates new records.

### Example 10: Assignment with batching
```powershell
PS C:\> $newOwnerId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"

PS C:\> Get-DataverseRecord -Connection $c -TableName account -Top 100 | 
    ForEach-Object { $_.ownerid = $newOwnerId } | 
    Set-DataverseRecord -Connection $c
```

Reassigns 100 accounts to a new owner. The cmdlet handles this as update operations followed by AssignRequest operations, all batched together.

### Example 11: Using lookup columns with names
```powershell
PS C:\> @{
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = "Contoso Ltd"  # Lookup by account name
    "parentcustomerid@logicalname" = "account"
} | Set-DataverseRecord -Connection $c -TableName contact
```

Creates a contact with a lookup to an account by name. The module will automatically resolve "Contoso Ltd" to the account's GUID if the name is unique.

### Example 12: Handle errors in batch operations
```powershell
PS C:\> $records = @(
    @{ firstname = "Alice"; lastname = "Valid"; emailaddress1 = "alice@example.com" }
    @{ firstname = "Bob"; lastname = ""; emailaddress1 = "bob@example.com" }  # Invalid - required field missing
    @{ firstname = "Charlie"; lastname = "Valid"; emailaddress1 = "charlie@example.com" }
)

PS C:\> $errors = @()
PS C:\> $records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue

PS C:\> # Process errors - each error's TargetObject contains the input record that failed
PS C:\> foreach ($err in $errors) {
    Write-Host "Failed to create contact: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

Demonstrates batch error handling. When using batching (default BatchSize of 100), the cmdlet continues processing all records even if some fail. Each error written to the error stream includes the original input object as the `TargetObject`, allowing you to correlate which input caused the error. Use `-ErrorVariable` to collect errors and `-ErrorAction SilentlyContinue` to prevent them from stopping execution.

### Example 13: Stop on first error with BatchSize 1
```powershell
PS C:\> $records = @(
    @{ firstname = "Alice"; lastname = "Valid" }
    @{ firstname = "Bob"; lastname = "" }  # Invalid - will cause error
    @{ firstname = "Charlie"; lastname = "Valid" }  # Won't be processed
)

PS C:\> try {
    $records | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -BatchSize 1 -ErrorAction Stop
} catch {
    Write-Host "Error creating record: $($_.TargetObject.firstname)"
    Write-Host "Remaining records were not processed"
}
```

Disables batching by setting `-BatchSize 1`. With this setting, each record is sent in a separate request, and execution stops immediately on the first error (when using `-ErrorAction Stop`). This is useful when you need to stop processing on the first failure rather than attempting all records.

### Example 14: Get IDs of created records using PassThru
```powershell
PS C:\> $contacts = @(
    @{ firstname = "Alice"; lastname = "Anderson" }
    @{ firstname = "Bob"; lastname = "Brown" }
    @{ firstname = "Charlie"; lastname = "Clark" }
)

PS C:\> $createdRecords = $contacts | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru

PS C:\> # Access the IDs of created records
PS C:\> foreach ($record in $createdRecords) {
    Write-Host "Created contact $($record.firstname) $($record.lastname) with ID: $($record.Id)"
}
```

Creates multiple contact records and returns the input objects with their `Id` property set. The `-PassThru` parameter causes the cmdlet to write each input object to the pipeline after the record is created, with the `Id` property populated with the GUID of the newly created record. This allows you to capture and use the IDs in subsequent operations.

### Example 15: Use PassThru to create and link records
```powershell
PS C:\> # Create parent account and get its ID
PS C:\> $account = @{ name = "Contoso Ltd" } | 
    Set-DataverseRecord -Connection $c -TableName account -CreateOnly -PassThru

PS C:\> # Create child contact linked to the account
PS C:\> $contact = @{ 
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = $account.Id
    "parentcustomerid@logicalname" = "account"
} | Set-DataverseRecord -Connection $c -TableName contact -CreateOnly -PassThru

PS C:\> Write-Host "Created contact $($contact.Id) linked to account $($account.Id)"
```

Demonstrates using `-PassThru` to chain record creation operations. First creates an account and captures its ID using `-PassThru`, then uses that ID to create a related contact record. This pattern is useful when you need to establish relationships between newly created records.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet,

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
### System.String[]
### System.Guid
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
