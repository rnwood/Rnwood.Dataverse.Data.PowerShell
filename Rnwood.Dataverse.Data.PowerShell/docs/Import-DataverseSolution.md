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

### FromFile (Default)
```
Import-DataverseSolution [-InFile] <String> [-OverwriteUnmanagedCustomizations] [-PublishWorkflows]
 [-SkipProductUpdateDependencies] [-HoldingSolution] [-ConnectionReferences <Hashtable>]
 [-EnvironmentVariables <Hashtable>] [-ConvertToManaged] [-SkipQueueRibbonJob]
 [-LayerDesiredOrder <LayerDesiredOrder>] [-AsyncRibbonProcessing] [-PollingIntervalSeconds <Int32>]
 [-TimeoutSeconds <Int32>] [-SkipConnectionReferenceValidation] [-SkipEnvironmentVariableValidation]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FromBytes
```
Import-DataverseSolution -SolutionFile <Byte[]> [-OverwriteUnmanagedCustomizations] [-PublishWorkflows]
 [-SkipProductUpdateDependencies] [-HoldingSolution] [-ConnectionReferences <Hashtable>]
 [-EnvironmentVariables <Hashtable>] [-ConvertToManaged] [-SkipQueueRibbonJob]
 [-LayerDesiredOrder <LayerDesiredOrder>] [-AsyncRibbonProcessing] [-PollingIntervalSeconds <Int32>]
 [-TimeoutSeconds <Int32>] [-SkipConnectionReferenceValidation] [-SkipEnvironmentVariableValidation]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet imports a Dataverse solution using an asynchronous job and monitors the job progress, providing real-time progress updates.

The cmdlet:
1. Initiates an asynchronous solution import using ImportSolutionAsyncRequest
2. Monitors the async operation status by polling the asyncoperation table
3. Reports progress using PowerShell's progress bar
4. Outputs the import job ID when complete
5. Supports setting connection references via a hashtable
6. Automatically falls back to regular import if using -HoldingSolution on a solution that doesn't exist

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
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution_v2.zip" -HoldingSolution
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

### -HoldingSolution
Import the solution as a holding solution staged for upgrade. Automatically falls back to regular import if solution doesn't exist.

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
Timeout in seconds for the import operation. Default is 1800 (30 minutes).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1800
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

### System.Byte[]
## OUTPUTS

### System.Object
## NOTES

This cmdlet uses the ImportSolutionAsyncRequest API which imports the solution in the background. The cmdlet monitors the async operation and outputs the job details when complete.

For synchronous imports (useful for small solutions), use Invoke-DataverseImportSolution.

**Connection References & Environment Variables:**
When importing solutions with connection references or environment variables, use the `-ConnectionReferences` and `-EnvironmentVariables` parameters with hashtables mapping names to values. The cmdlet converts these to the ComponentParameters format required by the API.

Connection references use the `connectionreference` entity with `connectionreferencelogicalname` and `connectionid` attributes.
Environment variables use the `environmentvariablevalue` entity with `environmentvariabledefinitionid` (mandatory lookup to the definition) and `Value` attributes. The cmdlet queries the target environment to find the definition ID by schema name.

**Automatic Validation:**
By default, the cmdlet validates that all connection references and environment variables in the solution are either:
1. Provided in the `-ConnectionReferences` or `-EnvironmentVariables` parameters, OR
2. Already exist in the target environment with values set

If any required components are missing, the cmdlet will throw an error listing the missing items. This helps catch configuration errors before the import begins.

Use `-SkipConnectionReferenceValidation` to bypass validation of connection references.
Use `-SkipEnvironmentVariableValidation` to bypass validation of environment variables.

**Upgrade Scenarios:**
When using -HoldingSolution to import a solution as an upgrade, the cmdlet extracts the solution's unique name from the solution.xml file within the ZIP and queries the target environment to check if it already exists. If it doesn't exist, the cmdlet automatically falls back to a regular import. This prevents errors when deploying to new environments.

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
[Invoke-DataverseImportSolution]()
[Invoke-DataverseImportSolutionAsync]()
