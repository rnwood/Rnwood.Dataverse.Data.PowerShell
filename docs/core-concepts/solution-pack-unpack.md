# Solution Pack and Unpack

## Overview

The `Expand-DataverseSolution` and `Compress-DataverseSolution` cmdlets provide a PowerShell interface to the Power Apps CLI's solution pack and unpack functionality. These cmdlets are essential for managing Dataverse solutions in source control and enable collaborative development workflows.

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
- `Expand-DataverseSolution` uses `pac solution unpack`
- `Compress-DataverseSolution` uses `pac solution pack`

The cmdlets handle PAC CLI installation automatically:
1. First checks if `pac` is in your PATH
2. Then checks if it's installed as a .NET global tool
3. If not found, automatically installs it using `dotnet tool install --global Microsoft.PowerApps.CLI.Tool`

## Working with Canvas Apps (.msapp files)

Canvas Apps in Dataverse solutions are stored as `.msapp` files, which are themselves ZIP archives. To track Canvas Apps in source control effectively, you need to unpack these files as well.

### Unpacking Canvas Apps

Use the `-UnpackMsapp` switch to automatically extract `.msapp` files:

```powershell
# Export and unpack a solution with Canvas Apps
$conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

Export-DataverseSolution -Connection $conn -SolutionName "MySolution" -OutFile "MySolution.zip"

Expand-DataverseSolution -Path "MySolution.zip" -OutputPath "MySolution_Src" -UnpackMsapp
```

This will:
1. Unpack the solution ZIP to `MySolution_Src/`
2. Find all `.msapp` files in the unpacked solution
3. Extract each `.msapp` file to a folder with the same name (e.g., `MyApp.msapp` → `MyApp/`)

### Packing Canvas Apps

Use the `-PackMsapp` switch to automatically pack Canvas App folders before packing the solution:

```powershell
# Pack a solution with Canvas Apps
Compress-DataverseSolution -Path "MySolution_Src" -OutputPath "MySolution.zip" -PackMsapp
```

This will:
1. Create a temporary copy of the solution folder
2. Identify Canvas App folders (by their structure: Src/, DataSources/, etc.)
3. Zip each folder into an `.msapp` file
4. Pack the solution using the modified structure
5. Clean up temporary files

The `-PackMsapp` operation doesn't modify your source folder, ensuring your unpacked Canvas Apps remain in place for version control.

## Complete Workflow Example

Here's a complete example of exporting, editing, and re-importing a solution:

```powershell
# 1. Connect to source environment
$sourceConn = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive

# 2. Export the solution
Export-DataverseSolution -Connection $sourceConn -SolutionName "MySolution" -OutFile "MySolution.zip"

# 3. Unpack for editing
Expand-DataverseSolution -Path "MySolution.zip" -OutputPath "MySolution_Src" -UnpackMsapp -Clobber

# 4. Edit files in MySolution_Src/ (e.g., update customizations, forms, etc.)
# ... manual edits or automated scripts ...

# 5. Pack the solution back
Compress-DataverseSolution -Path "MySolution_Src" -OutputPath "MySolution_Modified.zip" -PackMsapp

# 6. Connect to target environment
$targetConn = Get-DataverseConnection -Url "https://test.crm.dynamics.com" -Interactive

# 7. Import the modified solution
Import-DataverseSolution -Connection $targetConn -InFile "MySolution_Modified.zip"
```

## Best Practices

### Version Control
- Always unpack solutions before committing to source control
- Use `.gitignore` to exclude `.zip` files (keep only unpacked folders)
- Use `-UnpackMsapp` consistently across your team

### CI/CD Pipelines
- Use `Compress-DataverseSolution` in your build pipeline to create deployable artifacts
- Use `Import-DataverseSolution` in your release pipeline to deploy to environments
- Store unpacked solution folders in your repository, not ZIP files

### Collaboration
- Agree on a standard workflow with your team (always pack/unpack with same flags)
- Use `-Clobber` when unpacking to avoid conflicts
- Review solution diffs in pull requests before merging

### Canvas Apps
- When using `-PackMsapp`, be aware that it creates temporary copies (ensure sufficient disk space)
- Canvas App folder structure is detected automatically (Src/, DataSources/, Connections/, AppCheckerResult.sarif)
- If a folder doesn't look like a Canvas App, it won't be packed

## Troubleshooting

### PAC CLI not found
If you see errors about PAC CLI not being found:
- Ensure you have .NET SDK installed
- Try running `dotnet tool install --global Microsoft.PowerApps.CLI.Tool` manually
- Check that `~/.dotnet/tools` is in your PATH

### Unpack/Pack failures
If unpack or pack operations fail:
- Check that the solution file is valid (try opening it in Windows Explorer)
- Ensure you have write permissions to the output folder
- Use `-Verbose` to see detailed PAC CLI output
- Try running `pac solution unpack` manually to see raw error messages

### Canvas App detection issues
If Canvas Apps aren't detected with `-PackMsapp`:
- Verify the folder has the typical .msapp structure (Src/, DataSources/, etc.)
- At least 2 indicators must be present for detection
- Use `-Verbose` to see which folders are being checked

## Related Cmdlets

- [Export-DataverseSolution](../Export-DataverseSolution.md) - Export a solution from Dataverse
- [Import-DataverseSolution](../Import-DataverseSolution.md) - Import a solution to Dataverse
- [Get-DataverseSolution](../Get-DataverseSolution.md) - Get solution information
- [Remove-DataverseSolution](../Remove-DataverseSolution.md) - Remove a solution

## Further Reading

- [Power Apps CLI documentation](https://learn.microsoft.com/power-platform/developer/cli/introduction)
- [Solution concepts in Dataverse](https://learn.microsoft.com/power-apps/developer/data-platform/solution-concepts-alm)
- [ALM with Microsoft Power Platform](https://learn.microsoft.com/power-platform/alm/)
