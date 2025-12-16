---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseUninstallDependency

## SYNOPSIS
Retrieves dependencies that would prevent a solution from being uninstalled.

## SYNTAX

```
Get-DataverseUninstallDependency [-SolutionUniqueName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves dependencies that would prevent a solution from being uninstalled using the RetrieveDependenciesForUninstallRequest message.

When you attempt to uninstall a solution, Dataverse checks whether any components outside the solution depend on components within the solution. This cmdlet returns information about those dependencies so you can address them before attempting to uninstall.

For example, if a solution contains an attribute that is referenced by a form in another solution, this dependency will be reported and must be resolved before the solution can be uninstalled.

The cmdlet returns a collection of dependency entities, each describing a dependency that prevents uninstallation.

## EXAMPLES

### Example 1: Check dependencies before uninstalling a solution
```powershell
PS C:\> $dependencies = Get-DataverseUninstallDependency -Connection $c -SolutionUniqueName "MyCustomSolution"
PS C:\> if ($dependencies) {
>>     Write-Host "Cannot uninstall solution - found $($dependencies.Count) dependencies"
>>     $dependencies | Format-Table dependentcomponentobjectid, dependentcomponenttype, requiredcomponenttype
>> } else {
>>     Write-Host "Solution can be safely uninstalled"
>> }
```

Checks if a solution has any dependencies before attempting to uninstall it.

### Example 2: Identify external components that depend on the solution
```powershell
PS C:\> $dependencies = Get-DataverseUninstallDependency -SolutionUniqueName "TestSolution"
PS C:\> $dependencies | ForEach-Object {
>>     [PSCustomObject]@{
>>         DependentComponentId = $_.dependentcomponentobjectid
>>         DependentComponentType = $_.dependentcomponenttype
>>         RequiredComponentId = $_.requiredcomponentobjectid
>>         RequiredComponentType = $_.requiredcomponenttype
>>     }
>> } | Format-Table -AutoSize
```

Creates a detailed report of all dependencies preventing uninstallation.

### Example 3: Group dependencies by type
```powershell
PS C:\> $dependencies = Get-DataverseUninstallDependency -SolutionUniqueName "MySolution"
PS C:\> $dependencies | Group-Object dependentcomponenttype | Select-Object Count, Name | Format-Table

Count Name
----- ----
    5 24
    3 26
    2 80
```

Groups dependencies by component type to understand what types of components are blocking uninstallation.

### Example 4: Pipeline processing from solution list
```powershell
PS C:\> $solutions = Get-DataverseSolution | Where-Object { $_.ismanaged -eq $false }
PS C:\> $solutions | ForEach-Object {
>>     $deps = Get-DataverseUninstallDependency -SolutionUniqueName $_.uniquename
>>     if ($deps) {
>>         [PSCustomObject]@{
>>             SolutionName = $_.friendlyname
>>             UniqueName = $_.uniquename
>>             DependencyCount = $deps.Count
>>             CanUninstall = $false
>>         }
>>     } else {
>>         [PSCustomObject]@{
>>             SolutionName = $_.friendlyname
>>             UniqueName = $_.uniquename
>>             DependencyCount = 0
>>             CanUninstall = $true
>>         }
>>     }
>> } | Format-Table
```

Checks all unmanaged solutions to identify which ones can be safely uninstalled.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

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

### -SolutionUniqueName
Unique name of the solution to check for uninstall dependencies.

```yaml
Type: String
Parameter Sets: (All)
Aliases: UniqueName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
You can pipe objects with SolutionUniqueName or UniqueName properties to this cmdlet.

## OUTPUTS

### Microsoft.Xrm.Sdk.Entity
Returns dependency entities with information about each dependency that prevents uninstallation.

## NOTES
- This cmdlet uses the RetrieveDependenciesForUninstallRequest SDK message.
- The returned entities contain properties like dependentcomponentobjectid, dependentcomponenttype, requiredcomponentobjectid, and requiredcomponenttype.
- An empty result means the solution can be uninstalled without dependency issues.
- Dependencies must be resolved (e.g., by removing dependent components or uninstalling dependent solutions) before the solution can be uninstalled.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseDependency](Get-DataverseDependency.md)
[Get-DataverseDependentComponent](Get-DataverseDependentComponent.md)
[Get-DataverseMissingDependency](Get-DataverseMissingDependency.md)
[Remove-DataverseSolution](Remove-DataverseSolution.md)
