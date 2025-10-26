<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Solution Management](#solution-management)
      - [Parsing solution files](#parsing-solution-files)
      - [Exporting solutions](#exporting-solutions)
      - [Listing solutions](#listing-solutions)
      - [Creating and updating solutions](#creating-and-updating-solutions)
      - [Publishing customizations](#publishing-customizations)
      - [Importing solutions](#importing-solutions)
      - [Analyzing Solution Components](#analyzing-solution-components)
        - [Get-DataverseSolutionComponent](#get-dataversesolutioncomponent)
        - [Get-DataverseSolutionFileComponent](#get-dataversesolutionfilecomponent)
        - [Compare-DataverseSolutionComponents](#compare-dataversesolutioncomponents)
      - [Uninstalling/removing solutions](#uninstallingremoving-solutions)
        - [Handling Connection References and Environment Variables](#handling-connection-references-and-environment-variables)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Solution Management

<!-- TOC -->
    - [Parsing solution files](#parsing-solution-files)
    - [Exporting solutions](#exporting-solutions)
    - [Listing solutions](#listing-solutions)
    - [Creating and updating solutions](#creating-and-updating-solutions)
    - [Publishing customizations](#publishing-customizations)
    - [Importing solutions](#importing-solutions)
    - [Analyzing Solution Components](#analyzing-solution-components)
    - [Uninstalling/removing solutions](#uninstallingremoving-solutions)
      - [Handling Connection References and Environment Variables](#handling-connection-references-and-environment-variables)
<!-- /TOC -->


You can manage Dataverse solutions from this module. The module provides cmdlets for:
- Exporting solutions (`Export-DataverseSolution`)
- Importing solutions (`Import-DataverseSolution`)
- Listing installed solutions (`Get-DataverseSolution`)
- Parsing solution files (`Get-DataverseSolutionFile`)
- Creating/updating solutions (`Set-DataverseSolution`)
- Removing solutions (`Remove-DataverseSolution`)
- Publishing customizations (`Publish-DataverseCustomizations`)
For advanced control, use the `Invoke-` variants documented in the `docs/` folder.
#### Parsing solution files
- `Get-DataverseSolutionFile` parses a solution ZIP file and extracts metadata without requiring a Dataverse connection.
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
#### Exporting solutions
- `Export-DataverseSolution` exports a solution and can save it to disk or output to the pipeline. It supports including solution settings and reports progress for long-running exports.
Examples:
```powershell
# Export unmanaged solution to file
Export-DataverseSolution -Connection $c -SolutionName "MySolution" -OutFile "C:\Exports\MySolution.zip"
# Export managed solution and capture bytes
$b = Export-DataverseSolution -Connection $c -SolutionName "MySolution" -Managed -PassThru
[System.IO.File]::WriteAllBytes("C:\Exports\MySolution_managed.zip", $b)
```

#### Listing solutions

- `Get-DataverseSolution` retrieves information about installed solutions in a Dataverse environment.
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

#### Creating and updating solutions

- `Set-DataverseSolution` creates a new solution if it doesn't exist, or updates an existing solution.
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
#### Publishing customizations
- `Publish-DataverseCustomizations` publishes customizations to make them available to users.
- Can publish all customizations or specific entity customizations.
- See the full parameter reference: [Publish-DataverseCustomizations](../../Rnwood.Dataverse.Data.PowerShell/docs/Publish-DataverseCustomizations.md).
Examples:
```powershell
# Publish all customizations
Publish-DataverseCustomizations -Connection $c
# Publish customizations for a specific entity
Publish-DataverseCustomizations -Connection $c -EntityName "contact"
```
#### Importing solutions
- `Import-DataverseSolution` imports a solution file with intelligent by default logic. By default, it automatically determines the best import method:
  - If the solution doesn't exist, performs a regular import
  - If the solution exists and is managed, performs a stage-and-upgrade operation
  - If the solution exists and is unmanaged, performs a regular import (upgrade)
  - Use `-UseUpdateIfAdditive` (experimental) to perform component comparison and use simple import mode if only additive changes are detected. This boosts import performance when nothing has been removed (full upgrader is needed to remove things). Only valid with Auto (default) mode.
- Use `-Mode NoUpgrade` to force a regular import regardless of solution status
- Use `-Mode StageAndUpgrade` to explicitly perform a stage-and-upgrade operation
- Use `-Mode HoldingSolution` to import as a holding solution for upgrade
- See the full parameter reference: [Import-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Import-DataverseSolution.md).

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

#### Analyzing Solution Components

> [!NOTE]
> The following cmdlets are experimental and incomplete.

The module provides experimental cmdlets for analyzing solution components:

##### Get-DataverseSolutionComponent

Retrieves components from a solution in a Dataverse environment:

```powershell
# Get components by solution name
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution"

# Include subcomponents
Get-DataverseSolutionComponent -Connection $c -SolutionName "MySolution" -IncludeSubcomponents
```

##### Get-DataverseSolutionFileComponent

Extracts components from a solution file:

```powershell
# Extract from file
Get-DataverseSolutionFileComponent -SolutionFile "MySolution.zip"

# Extract from bytes
$bytes = [System.IO.File]::ReadAllBytes("MySolution.zip")
$bytes | Get-DataverseSolutionFileComponent
```

##### Compare-DataverseSolutionComponents

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

#### Uninstalling/removing solutions
- `Remove-DataverseSolution` removes (uninstalls) a solution from a Dataverse environment using the asynchronous uninstall process. The operation is asynchronous and the cmdlet monitors the uninstall progress.
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

##### Handling Connection References and Environment Variables

When importing solutions that contain connection references (for API connections) or environment variables (custom settings), you must provide values for these components unless they already exist in the target environment with values set. The cmdlet validates this by default to prevent unexpected behaviour of your solution after import if these values are missing.

**Connection References:**
- You must supply the connection ID (GUID) for each connection reference schema name.
- If not provided and not already configured in the environment, the import will fail.

**Environment Variables:**
- You must supply the value for each environment variable schema name.
- If not provided and not already set in the environment, the import will fail.

Examples:

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

# Skip validation if you want to ignore for some reason
Import-DataverseSolution -Connection $c -InFile "C:\Solutions\MySolution.zip" `
    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation
```
**Notes:**
- Connection reference values must be valid connection IDs from the target environment which the user importing the solution has access to.
- Environment variable values are strings that will be set during import.


