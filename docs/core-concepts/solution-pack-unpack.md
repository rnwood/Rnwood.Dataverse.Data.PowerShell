# Solution Pack and Unpack

## Overview

The `Expand-DataverseSolutionFile` and `Compress-DataverseSolutionFile` cmdlets provide a PowerShell interface to the Power Apps CLI's solution pack and unpack functionality. These cmdlets are essential for managing Dataverse solutions in source control and enable collaborative development workflows.

## Why Pack and Unpack Solutions?

When working with Dataverse solutions, you typically:

1. **Export** a solution from an environment as a `.zip` file
2. **Unpack** the solution into individual files for version control
3. **Edit** the individual files (e.g., customizations.xml, forms, views)
4. **Pack** the solution back into a `.zip` file
5. **Import** the solution to a target environment

This workflow allows you to:
- Track changes in source control (Git, Azure DevOps, etc.)
- Review diffs between versions
- Collaborate with team members
- Automate deployments with CI/CD pipelines

## Power Apps CLI Integration

These cmdlets wrap the Power Apps CLI (`pac`) commands:
- `Expand-DataverseSolutionFile` uses `pac solution unpack`
- `Compress-DataverseSolutionFile` uses `pac solution pack`

**Prerequisites:**
- Power Apps CLI must be installed and available in your system PATH
- Install from: https://aka.ms/PowerAppsCLI
- The cmdlets will search for `pac.exe` (Windows) or `pac` (Unix) in your PATH

## Working with Canvas Apps (.msapp files)

Canvas Apps in Dataverse solutions are stored as `.msapp` files, which are themselves ZIP archives. To track Canvas Apps in source control effectively, you need to unpack these files as well.

### Unpacking Canvas Apps

Use the `-UnpackMsapp` switch to automatically extract `.msapp` files:

```powershell
# Export and unpack a solution with Canvas Apps
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -OutFile "MySolution.zip"

Expand-DataverseSolutionFile -Path "MySolution.zip" -OutputPath "MySolution_Src" -UnpackMsapp
```

This will:
1. Unpack the solution ZIP to `MySolution_Src/`
2. Find all `.msapp` files in the unpacked solution
3. Extract each `.msapp` file to a folder with the same name (e.g., `MyApp.msapp` → `MyApp/`)

### Packing Canvas Apps

Canvas App folders with `.msapp` extension are automatically packed:

```powershell
# Pack a solution with Canvas Apps (automatic detection)
Compress-DataverseSolutionFile -Path "MySolution_Src" -OutputPath "MySolution.zip"
```

This will:
1. Detect any folders with `.msapp` extension (e.g., `MyApp.msapp/`)
2. Create a temporary copy of the solution folder
3. Zip each `.msapp` folder into an `.msapp` file
4. Pack the solution using the modified structure
5. Clean up temporary files

The operation doesn't modify your source folder, ensuring your unpacked Canvas Apps remain in place for version control.

## Complete Workflow Example

Here's a complete example of exporting, editing, and re-importing a solution:

```powershell
# 1. Connect to source environment
$sourceConn = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive

# 2. Export the solution
Export-DataverseSolution -Connection $sourceConn -SolutionName "MySolution" -OutFile "MySolution.zip"

# 3. Unpack for editing (always overwrites/allows delete)
Expand-DataverseSolutionFile -Path "MySolution.zip" -OutputPath "MySolution_Src" -UnpackMsapp

# 4. Edit files in MySolution_Src/ (e.g., update customizations, forms, etc.)
# ... manual edits or automated scripts ...

# 5. Pack the solution back (Canvas App folders with .msapp extension are automatically packed)
Compress-DataverseSolutionFile -Path "MySolution_Src" -OutputPath "MySolution_Modified.zip"

# 6. Connect to target environment
$targetConn = Get-DataverseConnection -Url "https://test.crm.dynamics.com" -Interactive

# 7. Import the modified solution
Import-DataverseSolution -Connection $targetConn -InFile "MySolution_Modified.zip"
```

## Package Type Support

Both cmdlets support the `-PackageType` parameter to work with managed and unmanaged solutions:

### Unmanaged (Default)
```powershell
# Unpack an unmanaged solution
Expand-DataverseSolutionFile -Path "MySolution.zip" -OutputPath "src"

# Pack an unmanaged solution
Compress-DataverseSolutionFile -Path "src" -OutputPath "MySolution.zip"
```

### Managed
```powershell
# Unpack a managed solution
Expand-DataverseSolutionFile -Path "MySolution_managed.zip" -OutputPath "src" -PackageType Managed

# Pack a managed solution (from a previous 'Both' unpack)
Compress-DataverseSolutionFile -Path "src" -OutputPath "MySolution_managed.zip" -PackageType Managed
```

### Both (Dual Managed and Unmanaged)
```powershell
# Unpack both managed and unmanaged (creates separate folders)
Expand-DataverseSolutionFile -Path "MySolution.zip" -OutputPath "src" -PackageType Both

# This creates both managed and unmanaged folder structures
# You can then pack either one using -PackageType Managed or -PackageType Unmanaged
```

## Best Practices

### Version Control
- Always unpack solutions before committing to source control
- Use `.gitignore` to exclude `.zip` files (keep only unpacked folders)
- Use `-UnpackMsapp` consistently across your team
- The cmdlets always overwrite and allow delete for consistency
- Use `-PackageType Both` if you need to work with both managed and unmanaged solutions

### CI/CD Pipelines
- Install PAC CLI in your build agents: https://aka.ms/PowerAppsCLI
- Use `Compress-DataverseSolutionFile` in your build pipeline to create deployable artifacts
- Use `Import-DataverseSolution` in your release pipeline to deploy to environments
- Store unpacked solution folders in your repository, not ZIP files
- Use consistent `-PackageType` across your pipeline

### Collaboration
- Agree on a standard workflow with your team (always pack/unpack with same flags)
- The cmdlets automatically handle overwrites and deletions
- Review solution diffs in pull requests before merging
- Decide as a team whether to use Unmanaged, Managed, or Both package types

### Canvas Apps
- Canvas App folders with `.msapp` extension are automatically packed (no switch needed)
- Packing creates temporary copies to avoid modifying source (ensure sufficient disk space)
- Name your Canvas App folders with the `.msapp` extension (e.g., `MyApp.msapp/`)
- Both pack and unpack operations handle .msapp folders automatically

## Troubleshooting

### PAC CLI not found
If you see errors about PAC CLI not being found:
- Install Power Apps CLI from https://aka.ms/PowerAppsCLI
- Ensure `pac` or `pac.exe` is in your system PATH
- Restart your PowerShell session after installation
- Test by running `pac` in PowerShell to verify installation

### Unpack/Pack failures
If unpack or pack operations fail:
- Check that the solution file is valid (try opening it in Windows Explorer)
- Ensure you have write permissions to the output folder
- Use `-Verbose` to see detailed PAC CLI output
- Try running `pac solution unpack` manually to see raw error messages
- Verify your `-PackageType` matches the solution file (e.g., don't use Managed with an unmanaged ZIP)

### Canvas App folder naming
For Canvas Apps to be automatically packed:
- Name your Canvas App folders with the `.msapp` extension (e.g., `MyApp.msapp/`)
- The folder name will be used as the .msapp file name
- Use `-Verbose` to see which folders are being packed

## Related Cmdlets

- [Export-DataverseSolution](../Export-DataverseSolution.md) - Export a solution from Dataverse
- [Import-DataverseSolution](../Import-DataverseSolution.md) - Import a solution to Dataverse
- [Get-DataverseSolution](../Get-DataverseSolution.md) - Get solution information
- [Remove-DataverseSolution](../Remove-DataverseSolution.md) - Remove a solution

## Further Reading

- [Power Apps CLI documentation](https://learn.microsoft.com/power-platform/developer/cli/introduction)
- [Solution concepts in Dataverse](https://learn.microsoft.com/power-apps/developer/data-platform/solution-concepts-alm)
- [ALM with Microsoft Power Platform](https://learn.microsoft.com/power-platform/alm/)
