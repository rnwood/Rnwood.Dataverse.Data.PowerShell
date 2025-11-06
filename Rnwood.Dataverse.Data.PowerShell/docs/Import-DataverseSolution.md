---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Import-DataverseSolution

## SYNOPSIS
Imports a solution to Dataverse using an asynchronous job with progress reporting.

## SYNTAX

### FromFile
```
Import-DataverseSolution [-InFile] <String> [-OverwriteUnmanagedCustomizations] [-PublishWorkflows]
 [-SkipProductUpdateDependencies] [-Mode <ImportMode>] [-ConnectionReferences <Hashtable>]
 [-EnvironmentVariables <Hashtable>] [-ConvertToManaged] [-SkipQueueRibbonJob]
 [-LayerDesiredOrder <LayerDesiredOrder>] [-AsyncRibbonProcessing] [-UseUpdateIfAdditive]
 [-PollingIntervalSeconds <Int32>] [-TimeoutSeconds <Int32>] [-SkipConnectionReferenceValidation]
 [-SkipEnvironmentVariableValidation] [-SkipIfSameVersion] [-SkipIfLowerVersion] [-Connection <ServiceClient>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FromBytes
```
Import-DataverseSolution -SolutionFile <Byte[]> [-OverwriteUnmanagedCustomizations] [-PublishWorkflows]
 [-SkipProductUpdateDependencies] [-Mode <ImportMode>] [-ConnectionReferences <Hashtable>]
 [-EnvironmentVariables <Hashtable>] [-ConvertToManaged] [-SkipQueueRibbonJob]
 [-LayerDesiredOrder <LayerDesiredOrder>] [-AsyncRibbonProcessing] [-UseUpdateIfAdditive]
 [-PollingIntervalSeconds <Int32>] [-TimeoutSeconds <Int32>] [-SkipConnectionReferenceValidation]
 [-SkipEnvironmentVariableValidation] [-SkipIfSameVersion] [-SkipIfLowerVersion] [-Connection <ServiceClient>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet imports a Dataverse solution using an asynchronous job and monitors the job progress, providing real-time progress updates.

The cmdlet:
1. Initiates an asynchronous solution import using ImportSolutionAsyncRequest or StageAndUpgradeAsyncRequest
2. Monitors the async operation status by polling the asyncoperation table
3. Reports progress using PowerShell's progress bar
4. Outputs the import job ID when complete
5. Supports setting connection references via a hashtable
6. Automatically determines the appropriate import method based on the -Mode parameter

**Import Modes:**
- **Auto (default)**: Automatically determines the best import method based on solution existence and type. Uses StageAndUpgradeAsyncRequest if the solution exists and is managed, otherwise uses regular import.
- **NoUpgrade**: Forces regular import using ImportSolutionAsyncRequest, bypassing any upgrade logic.
- **StageAndUpgrade**: Explicitly requests stage and upgrade mode. Uses StageAndUpgradeAsyncRequest if the solution exists, otherwise falls back to regular import.
- **HoldingSolution**: Imports the solution as a holding solution for upgrade. If the solution doesn't exist, automatically falls back to regular import.

This is particularly useful for importing large solutions where the synchronous import would time out.

## EXAMPLES

### Example 1: Import a solution from a file
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip"
```

Imports the solution from the specified file with default settings.

### Example 2: Import with connection references and environment variables
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" `
    -ConnectionReferences @{
        'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
        'new_sql' = '87654321-4321-4321-4321-210987654321'
    } `
    -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
    }
```

Imports the solution and sets connection references for two connections and environment variables for two settings.

### Example 3: Import as holding solution (upgrade)
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution_v2.zip" -Mode HoldingSolution
```

Imports the solution as a holding solution for upgrade. If the solution doesn't already exist, it automatically falls back to a regular import.

### Example 4: Import with overwrite and publish workflows
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -OverwriteUnmanagedCustomizations -PublishWorkflows -Verbose
```

Imports the solution, overwrites unmanaged customizations, publishes workflows, and shows verbose output.

### Example 5: Import solution bytes from pipeline
```powershell
PS C:\> $bytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")
PS C:\> $bytes | Import-DataverseSolution -OverwriteUnmanagedCustomizations
```

Imports solution from a byte array via pipeline.

### Example 6: Import with custom timeout
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\LargeSolution.zip" -TimeoutSeconds 3600 -PollingIntervalSeconds 10
```

Imports a large solution with a 60-minute timeout and checks status every 10 seconds.

### Example 7: Skip validation for pre-configured environments
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation
```

Imports the solution and skips validation checks, useful when connection references and environment variables are already configured in the target environment.

### Example 8: Force regular import (skip upgrade logic)
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -Mode NoUpgrade
```

Imports the solution using regular import, bypassing any upgrade logic. Useful for fresh deployments or when you want to ensure a clean import.

### Example 9: Explicit stage and upgrade (when conditions are met)
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MyManagedSolution.zip" -Mode StageAndUpgrade
```

Explicitly requests stage and upgrade mode. The cmdlet will check if the solution exists and use StageAndUpgradeAsyncRequest if it does, otherwise falls back to regular import.

### Example 10: Skip import if same version is already installed
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution_1.0.0.0.zip" -SkipIfSameVersion
```

Skips the import if the solution version in the file (e.g., 1.0.0.0) is the same as the version already installed in the target environment. Useful for deployment scripts that should be idempotent.

### Example 11: Skip import if a newer version is already installed
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution_1.0.0.0.zip" -SkipIfLowerVersion
```

Skips the import if the solution version in the file is lower than the version already installed in the target environment. Prevents accidental downgrades.

### Example 12: Combine version checks
```powershell
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -SkipIfSameVersion -SkipIfLowerVersion
```

Skips the import if the solution version in the file is the same as or lower than the version installed. Only imports if the file contains a newer version.

## PARAMETERS

### -AsyncRibbonProcessing
For internal use only.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -ConnectionReferences
Hashtable of connection reference schema names to connection IDs. Used to set connection references during import.

Example: @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'}

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

### -ConvertToManaged
Obsolete. The system will convert unmanaged solution components to managed when you import a managed solution.

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

### -EnvironmentVariables
Hashtable of environment variable schema names to values. Used to set environment variable values during import.

Example: @{'new_apiurl' = 'https://api.example.com'}

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

### -InFile
Path to the solution file (.zip) to import.

```yaml
Type: String
Parameter Sets: FromFile
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LayerDesiredOrder
For internal use only.

```yaml
Type: LayerDesiredOrder
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mode
The import mode to use. Auto (default) automatically determines the best method based on solution existence and managed status.

```yaml
Type: ImportMode
Parameter Sets: (All)
Aliases:
Accepted values: Auto, NoUpgrade, StageAndUpgrade, HoldingSolution

Required: False
Position: Named
Default value: Auto
Accept pipeline input: False
Accept wildcard characters: False
```

### -OverwriteUnmanagedCustomizations
Overwrite any unmanaged customizations that have been applied over existing managed solution components.

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

### -PollingIntervalSeconds
Polling interval in seconds for checking job status. Default is 5.

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

### -PublishWorkflows
Activate any processes (workflows) included in the solution after import.

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

### -SkipConnectionReferenceValidation
Skip validation that all required connection references are provided.

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

### -SkipEnvironmentVariableValidation
Skip validation that all required environment variables are provided.

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

### -SkipIfLowerVersion
Skip import if the solution version in the file is lower than the installed version in the target environment.

This parameter allows you to prevent accidental downgrades by checking the version before importing. If the solution file version is lower than the installed version, the import is skipped with a warning message.

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

### -SkipIfSameVersion
Skip import if the solution version in the file is the same as the installed version in the target environment.

This parameter makes the import operation idempotent by skipping the import if the same version is already installed. Useful for deployment scripts that may be run multiple times.

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

### -SkipProductUpdateDependencies
Skip enforcement of dependencies related to product updates.

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

### -SkipQueueRibbonJob
For internal use only.

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

### -SolutionFile
Solution file bytes to import.

```yaml
Type: Byte[]
Parameter Sets: FromBytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -TimeoutSeconds
Timeout in seconds for the import operation. Default is 7200 (2 hours).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 7200
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseUpdateIfAdditive
Use update if additive mode (experimental and incomplete). Only valid with Auto (default) mode. If the solution already exists in the target environment, compares the solution file with the target environment. If there are zero items in 'TargetOnly' or 'InSourceAndTarget_BehaviourLessInclusiveInSource' status, uses simple install mode (no stage and upgrade). Use Compare-DataverseSolutionComponents to see what the comparison would show before using this switch.

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
Shows what would happen if the cmdlet runs. The cmdlet is not run.

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

### System.Byte[]

## OUTPUTS

### System.Object
## NOTES

This cmdlet uses the ImportSolutionAsyncRequest or StageAndUpgradeAsyncRequest APIs which import the solution in the background. The cmdlet monitors the async operation and outputs the job details when complete.

**Import Modes:**
- **Auto (default)**: Intelligently chooses the import method based on solution existence and type. Uses StageAndUpgradeAsyncRequest if the solution exists and the source is managed, otherwise uses regular import.
- **NoUpgrade**: Forces regular import using ImportSolutionAsyncRequest, bypassing any upgrade logic.
- **StageAndUpgrade**: Explicitly requests stage and upgrade mode. Uses StageAndUpgradeAsyncRequest if the solution exists, otherwise falls back to regular import.
- **HoldingSolution**: Imports as a holding solution for upgrade. If the solution doesn't exist, automatically falls back to regular import.

This provides optimal upgrade behavior while avoiding issues with unmanaged solutions.

For synchronous imports (useful for small solutions), use Invoke-DataverseImportSolution.

**Connection References & Environment Variables:**
When importing solutions with connection references or environment variables, use the `-ConnectionReferences` and `-EnvironmentVariables` parameters with hashtables mapping names to values. The cmdlet converts these to the ComponentParameters format required by the API.

Connection references use the `connectionreference` entity with `connectionreferencelogicalname` and `connectionid` attributes.
Environment variables use the `environmentvariablevalue` entity with `schemaname` and `value` attributes. The cmdlet queries the target environment for existing value records and includes the `environmentvariablevalueid` if found (for updates rather than creates).

**Automatic Validation:**
By default, the cmdlet validates that all connection references and environment variables in the solution are either:
1. Provided in the `-ConnectionReferences` or `-EnvironmentVariables` parameters, OR
2. Already exist in the target environment with values set

If any required components are missing, the cmdlet will throw an error listing the missing items. This helps catch configuration errors before the import begins.

Use `-SkipConnectionReferenceValidation` to bypass validation of connection references.
Use `-SkipEnvironmentVariableValidation` to bypass validation of environment variables.

**Upgrade Scenarios:**
When using -HoldingSolution to import a solution as an upgrade, the cmdlet extracts the solution's unique name from the solution.xml file within the ZIP and queries the target environment to check if it already exists. If it doesn't exist, the cmdlet automatically falls back to a regular import. This prevents errors when deploying to new environments.

**UseUpdateIfAdditive Mode:**
The -UseUpdateIfAdditive switch (experimental) performs a component comparison between the solution file and the target environment when the solution already exists. If the comparison shows only additive changes (no components removed or behavior changes that would remove data), it uses the simpler ImportSolutionAsyncRequest instead of StageAndUpgradeAsyncRequest for better performance. This switch is only valid when using the default Auto mode. Use Compare-DataverseSolutionComponents to preview what the comparison would show before using this switch.

**Version Checking:**
The `-SkipIfSameVersion` and `-SkipIfLowerVersion` switches allow you to control whether an import should proceed based on version comparison:
- `-SkipIfSameVersion`: Skips the import if the solution file version matches the installed version. Useful for idempotent deployment scripts.
- `-SkipIfLowerVersion`: Skips the import if the solution file version is lower than the installed version. Prevents accidental downgrades.

Both switches can be combined to ensure only newer versions are imported. The version is extracted from the solution.xml file in the ZIP and compared to the version in the target environment's solution table. If the solution doesn't exist in the target environment, these switches have no effect and the import proceeds normally.

Progress is reported using PowerShell's progress API and shows:
- Current status (Waiting, In progress, etc.)
- Percentage complete
- Status messages from the async operation

The async operation can be canceled by pressing Ctrl+C.

See also:
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.importsolutionasyncrequest
- https://learn.microsoft.com/en-us/power-platform/alm/conn-ref-env-variables-build-tools

## RELATED LINKS

[Get-DataverseConnection]()
[Export-DataverseSolution]()
[Compare-DataverseSolutionComponents]()
[Invoke-DataverseImportSolution]()
[Invoke-DataverseImportSolutionAsync]()
