# Solution Management

You can manage Dataverse solutions from this module. The module provides cmdlets for:
- Exporting solutions (`Export-DataverseSolution`)
- Importing solutions (`Import-DataverseSolution`)
- Listing installed solutions (`Get-DataverseSolution`)
- Parsing solution files (`Get-DataverseSolutionFile`)
- Creating/updating solutions (`Set-DataverseSolution`)
- Removing solutions (`Remove-DataverseSolution`)
- Publishing customizations (`Publish-DataverseCustomizations`)
- Packing and unpacking solutions for source control (`Compress-DataverseSolutionFile`, `Expand-DataverseSolutionFile`)

For advanced control, use the `Invoke-` variants documented in the `docs/` folder.

## Listing Solutions

`Get-DataverseSolution` retrieves information about installed solutions in a Dataverse environment.
- Supports filtering by name, managed status, and excluding system solutions.
- See the full parameter reference: [Get-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSolution.md).

Examples:

```powershell
# List all solutions
Get-DataverseSolution -Connection $c

# List managed solutions only
Get-DataverseSolution -Connection $c -Managed
# List unmanaged solutions only
Get-DataverseSolution -Connection $c -Unmanaged

# Get details for a specific solution
Get-DataverseSolution -Connection $c -UniqueName "MySolution"
```

## Creating and Updating Solutions

`Set-DataverseSolution` creates a new solution if it doesn't exist, or updates an existing solution.
- Supports updating friendly name, description, version (unmanaged only), and publisher.
- See the full parameter reference: [Set-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSolution.md).

Examples:

```powershell
# Create a new solution
Set-DataverseSolution -Connection $c -UniqueName "MySolution" `
    -Name "My Solution" -Description "My custom solution" `
    -Version "1.0.0.0" -PublisherUniqueName "mycompany"

# Update solution properties
Set-DataverseSolution -Connection $c -UniqueName "MySolution" `
    -Description "Updated description" -Version "1.1.0.0"
```

## Exporting Solutions

`Export-DataverseSolution` exports a solution and can save it to disk or output to the pipeline. It supports including solution settings and reports progress for long-running exports.

Examples:
```powershell
# Export unmanaged solution to file
Export-DataverseSolution -Connection $c -SolutionName "MySolution" -OutFile "C:\Exports\MySolution.zip"

# Export managed solution and capture bytes
$b = Export-DataverseSolution -Connection $c -SolutionName "MySolution" -Managed -PassThru
[System.IO.File]::WriteAllBytes("C:\Exports\MySolution_managed.zip", $b)
```

### Export and Unpack in One Step

You can streamline your workflow by exporting and unpacking in a single command using the `-OutFolder` parameter:

```powershell
# Export and automatically unpack to a folder
Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -OutFolder "MySolution_Src" -UnpackMsApp

# Export both managed and unmanaged solutions and unpack to a folder
Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -OutFolder "MySolution_Src" -PackageType Both
```

This command:
1. Exports the solution from Dataverse to a temporary ZIP file (or files if PackageType is Both)
2. Unpacks the ZIP file(s) to `MySolution_Src`
3. Unpacks any Canvas Apps (due to `-UnpackMsApp`)
4. Cleans up the temporary ZIP file(s)

## Importing Solutions

`Import-DataverseSolution` imports a solution file with intelligent logic by default. It automatically determines the best import method:
- If the solution doesn't exist, performs a regular import
- If the solution exists and is managed, performs a stage-and-upgrade operation
- If the solution exists and is unmanaged, performs a regular import (upgrade)

Use `-Mode NoUpgrade` to force a regular import regardless of solution status.
Use `-Mode StageAndUpgrade` to explicitly perform a stage-and-upgrade operation.
Use `-Mode HoldingSolution` to import as a holding solution for upgrade.

See the full parameter reference: [Import-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Import-DataverseSolution.md).

Examples:

```powershell
# Intelligent import (default behavior - automatically chooses best method)
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip"

# Force regular import (no upgrade logic)
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode NoUpgrade

# Explicitly perform stage-and-upgrade
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode StageAndUpgrade

# Import as holding solution
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" -Mode HoldingSolution

# Import from bytes instead of file
Import-DataverseSolution -Connection $c -SolutionBytes $bytes
```

### Pack and Import in One Step

You can pack a solution folder and import it in a single command using the `-InFolder` parameter:

```powershell
# Pack from a folder and import to Dataverse
Import-DataverseSolution -Connection $conn -InFolder "MySolution_Src" -PackageType Unmanaged
```

This command:
1. Packs the `MySolution_Src` folder to a temporary ZIP file
2. Automatically handles Canvas App packing (re-zipping .msapp folders)
3. Imports the ZIP file to Dataverse
4. Cleans up the temporary ZIP file

## Packing and Unpacking Solutions

The module provides cmdlets that support solution packing and unpacking using the Power Apps CLI (`pac`). These are essential for managing Dataverse solutions in source control and enable collaborative development workflows.

**Prerequisites:**
- Power Apps CLI must be installed and available in your system PATH
- Install from: https://aka.ms/PowerAppsCLI

### Unpacking Solutions

`Expand-DataverseSolutionFile` unpacks a solution ZIP file to a folder.

```powershell
# Unpack a solution
Expand-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip" -OutputPath "C:\Solutions\MySolution_Src"

# Unpack and extract .msapp files
Expand-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip" -OutputPath "C:\Solutions\MySolution_Src" -UnpackMsapp
```

### Packing Solutions

`Compress-DataverseSolutionFile` packs a solution folder to a ZIP file.

```powershell
# Pack a solution
Compress-DataverseSolutionFile -Path "C:\Solutions\MySolution_Src" -OutputPath "C:\Solutions\MySolution.zip"
```

### Working with Canvas Apps (.msapp files)

Canvas Apps in Dataverse solutions are stored as `.msapp` files, which are themselves ZIP archives. To track Canvas Apps in source control effectively, you need to unpack these files as well.

**Unpacking:** Use the `-UnpackMsapp` switch with `Expand-DataverseSolutionFile` or `Export-DataverseSolution` to automatically extract `.msapp` files into folders.

**Packing:** Canvas App folders with `.msapp` extension are automatically detected and packed by `Compress-DataverseSolutionFile` and `Import-DataverseSolution`.

### Package Types

All packing/unpacking cmdlets support the `-PackageType` parameter:
- `Unmanaged` (Default): For unmanaged solutions
- `Managed`: For managed solutions
- `Both`: For dual managed and unmanaged operation (creates separate folders)

## Publishing Customizations

`Publish-DataverseCustomizations` publishes customizations to make them available to users.
- Can publish all customizations or specific entity customizations.
- See the full parameter reference: [Publish-DataverseCustomizations](../../Rnwood.Dataverse.Data.PowerShell/docs/Publish-DataverseCustomizations.md).

Examples:
```powershell
# Publish all customizations
Publish-DataverseCustomizations -Connection $c
# Publish customizations for a specific entity
Publish-DataverseCustomizations -Connection $c -EntityName "contact"
```

## Parsing Solution Files

`Get-DataverseSolutionFile` parses a solution ZIP file and extracts metadata without requiring a Dataverse connection.
- Useful for inspecting solution files before importing or for automation scripts.
- See the full parameter reference: [Get-DataverseSolutionFile](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseSolutionFile.md).

Examples:
```powershell
# Parse a solution file and display metadata
$info = Get-DataverseSolutionFile -Path "C:\Solutions\MySolution_1_0_0_0.zip"
Write-Host "Solution: $($info.Name) v$($info.Version)"
Write-Host "Publisher: $($info.PublisherName)"
Write-Host "Managed: $($info.IsManaged)"
```

## Applying Staged Solution Upgrades

`Invoke-DataverseSolutionUpgrade` completes a solution upgrade that was previously staged using `Import-DataverseSolution` with `-Mode HoldingSolution` or `-Mode StageAndUpgrade`.
- It deletes the original solution and promotes the holding solution (named `SolutionName_Upgrade`) to become the active solution.
- Uses the `Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest` to perform the upgrade atomically.
- Use `-IfExists` to check if the holding solution exists before attempting the upgrade.
- See the full parameter reference: [Invoke-DataverseSolutionUpgrade](../../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSolutionUpgrade.md).

**Typical upgrade workflow:**
1. Import a new version of one or more solutions using `Import-DataverseSolution -Mode HoldingSolution` or `-Mode StageAndUpgrade` (creates `SolutionName_Upgrade`).
2. Apply any data migration steps that are needed before old tables and columns disappear.
3. Run `Invoke-DataverseSolutionUpgrade -SolutionName "SolutionName"` to complete the upgrade on the solutions.

For step 1, solutions must be in dependency order.
For step 3, solutions must be reversed.

## Analyzing Solution Components

> [!NOTE]
> The following cmdlets are experimental and incomplete.

The module provides experimental cmdlets for analyzing solution components:

### Get-DataverseSolutionComponent

Retrieves components from a solution in a Dataverse environment:

```powershell
# Get components by solution name
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution"

# Include implied subcomponents
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" -IncludeImpliedSubcomponents
```

### Get-DataverseSolutionFileComponent

Extracts components from a solution file:

```powershell
# Extract from file
Get-DataverseSolutionFileComponent -SolutionFile "MySolution.zip"

# Extract from bytes
$bytes = [System.IO.File]::ReadAllBytes("MySolution.zip")
$bytes | Get-DataverseSolutionFileComponent
```

### Compare-DataverseSolutionComponents

Compares solution components between files and environments:

```powershell
# Compare file with environment
Compare-DataverseSolutionComponents -Connection $c -SolutionFile "MySolution.zip"

# Compare two files
Compare-DataverseSolutionComponents -SolutionFile "v1.zip" -TargetSolutionFile "v2.zip"

# Reverse comparison
Compare-DataverseSolutionComponents -Connection $c -SolutionFile "MySolution.zip" -ReverseComparison
```

These cmdlets help understand what components are included in solutions, their behavior settings, and differences between versions or environments.

## Managing Solution Components

The module provides cmdlets for managing individual solution components within a solution:

### Set-DataverseSolutionComponent

Adds or updates a solution component with automatic handling of behavior changes. Since Dataverse doesn't allow updating the root component behavior directly, this cmdlet automatically removes and re-adds the component when the behavior needs to change.

```powershell
# Add an entity to a solution with default behavior (Include Subcomponents)
$entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customtable"
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId -ComponentType 1 -Behavior 0

# Change behavior from "Include Subcomponents" to "Do Not Include Subcomponents"
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId -ComponentType 1 -Behavior 1 -PassThru

# Add an attribute component
$attributeMetadata = Get-DataverseAttributeMetadata -Connection $c `
    -EntityName "new_customtable" -AttributeName "new_field"
Set-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $attributeMetadata.MetadataId -ComponentType 2 -Behavior 0
```

**Behavior values:**
- `0` = Include Subcomponents (default) - Includes all subcomponents like attributes, forms, views
- `1` = Do Not Include Subcomponents - Includes only the root component
- `2` = Include As Shell - Not fully supported by the API (will display a warning)

**Key features:**
- Idempotent operation - no action if component already exists with the same behavior
- Automatic behavior change - removes and re-adds component when behavior changes
- Supports all component types (entities, attributes, forms, views, etc.)
- Returns solutioncomponentid and other details when using -PassThru

See the full parameter reference: [Set-DataverseSolutionComponent](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseSolutionComponent.md).

### Remove-DataverseSolutionComponent

Removes a solution component from an unmanaged solution. Note that this only removes the component from the solution - the component itself remains in the environment.

```powershell
# Remove an entity component from a solution
$entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customtable"
Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId -ComponentType 1

# Remove with IfExists flag (no error if component doesn't exist in solution)
Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
    -ComponentId $componentId -ComponentType 2 -IfExists

# Remove multiple components
$components = Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution"
$components | Where-Object { $_.ComponentType -eq 26 } | ForEach-Object {
    Remove-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" `
        -ComponentId $_.ObjectId -ComponentType $_.ComponentType -Confirm:$false
}
```

**Important distinction:**
- `Remove-DataverseSolutionComponent` removes the component from the solution (component remains in environment)
- `Remove-DataverseEntityMetadata` / `Remove-DataverseAttributeMetadata` delete the component from the environment entirely

See the full parameter reference: [Remove-DataverseSolutionComponent](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSolutionComponent.md).

### Common Component Types

| Component Type | Description |
|----------------|-------------|
| 1 | Entity (Table) |
| 2 | Attribute (Column) |
| 9 | Option Set (Choice) |
| 10 | Entity Relationship |
| 24 | Form |
| 26 | View |
| 29 | Web Resource |
| 60 | Chart |
| 80 | Process (Workflow) |

For a complete list, see [Microsoft's Solution Component documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file).

## Uninstalling Solutions

`Remove-DataverseSolution` removes (uninstalls) a solution from a Dataverse environment using the asynchronous uninstall process. The operation is asynchronous and the cmdlet monitors the uninstall progress.
- When removing a solution:
  - All customizations contained in the solution are removed (for managed solutions)
  - Unmanaged solutions only remove the solution container, not the customizations
  - Dependencies must be resolved before removal (e.g., remove dependent solutions first)
- The cmdlet monitors the uninstall operation and reports progress
- See the full parameter reference: [Remove-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseSolution.md).

Examples:
```powershell
# Remove a solution
Remove-DataverseSolution -Connection $c -UniqueName "MySolution"
# Remove with confirmation
Remove-DataverseSolution -Connection $c -UniqueName "MySolution" -Confirm

# Remove with custom timeout for large solutions
Remove-DataverseSolution -Connection $c -UniqueName "LargeSolution" -TimeoutSeconds 1200 -PollingIntervalSeconds 10
```

## Handling Connection References and Environment Variables

When importing solutions that contain connection references (for API connections) or environment variables (custom settings), you must provide values for these components unless they already exist in the target environment with values set. The cmdlet validates this by default to prevent unexpected behaviour of your solution after import if these values are missing.

**Connection References:**
- You must supply the connection ID (GUID) for each connection reference schema name.
- If not provided and not already configured in the environment, the import will fail.

**Environment Variables:**
- You must supply the value for each environment variable schema name.
- If not provided and not already set in the environment, the import will fail.

**Important Notes:**
- The cmdlet only includes component parameters (connection references and environment variables) that are discovered in the solution file being imported.
- Any extra parameters provided via `-ConnectionReferences` or `-EnvironmentVariables` that are not in the solution file will be ignored and a verbose message will be logged.
- This filtering ensures that only relevant parameters are passed to the import operation.

**Setting Values During Import:**

```powershell
# Import with connection references and environment variables
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -ConnectionReferences @{
        'new_sharepointconnection' = '12345678-1234-1234-1234-123456789012'
        'new_sqlconnection' = '87654321-4321-4321-4321-210987654321'
    } `
    -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
    }

# You can provide extra parameters - they will be ignored if not in the solution
# This allows using a common set of parameters for multiple solution imports
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -ConnectionReferences @{
        'new_sharepointconnection' = '12345678-1234-1234-1234-123456789012'
        'new_sqlconnection' = '87654321-4321-4321-4321-210987654321'
        'new_otherconnection' = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'  # Ignored if not in solution
    } `
    -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
        'new_othersetting' = 'value'  # Ignored if not in solution
    }

# Skip validation if you want to ignore for some reason
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation
```

**Setting Values Independently:**

You can also set connection references and environment variables independently of solution import using dedicated cmdlets:

```powershell
# Set environment variable value (definition must exist)
Set-DataverseEnvironmentVariableValue -Connection $c `
    -SchemaName "new_apiurl" `
    -Value "https://api.production.example.com"

# Set multiple environment variable values at once
Set-DataverseEnvironmentVariableValue -Connection $c -EnvironmentVariableValues @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_apikey' = 'prod-key-12345'
    'new_timeout' = '30'
}

# Create environment variable definition
Set-DataverseEnvironmentVariableDefinition -Connection $c `
    -SchemaName "new_customsetting" `
    -DisplayName "Custom Setting" `
    -Description "Custom configuration value"

# Set the value separately
Set-DataverseEnvironmentVariableValue -Connection $c `
    -SchemaName "new_customsetting" `
    -Value "customvalue"

# Set a single connection reference (creates if doesn't exist)
Set-DataverseConnectionReference -Connection $c `
    -ConnectionReferenceLogicalName "new_sharepointconnection" `
    -ConnectionId "12345678-1234-1234-1234-123456789012" `
    -ConnectorId "98765432-4321-4321-4321-210987654321" `
    -DisplayName "SharePoint Connection" `
    -Description "Connection to the main SharePoint site"

# Set multiple connection references at once (update only - must already exist)
Set-DataverseConnectionReference -Connection $c -ConnectionReferences @{
    'new_sharepointconnection' = '12345678-1234-1234-1234-123456789012'
    'new_sqlconnection' = '87654321-4321-4321-4321-210987654321'
}
```

See the full documentation:
- [Environment Variables and Connection References Guide](environment-variables-connection-references.md)
- [Set-DataverseEnvironmentVariableDefinition](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableDefinition.md)
- [Set-DataverseEnvironmentVariableValue](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableValue.md)
- [Set-DataverseConnectionReference](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseConnectionReference.md)

**Notes:**
- Connection reference values must be valid connection IDs from the target environment which the user importing the solution has access to.
- Environment variable values are strings that will be set during import.
- The dedicated cmdlets create new environment variable value records if they don't exist, or update existing ones.
- Connection reference records can be created using the single parameter set of Set-DataverseConnectionReference, or must already exist when using the multiple parameter set.
