---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseMissingDependency

## SYNOPSIS
Retrieves missing dependencies for a solution.

## SYNTAX

```
Get-DataverseMissingDependency [-SolutionUniqueName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves missing dependencies for a solution using the RetrieveMissingDependenciesRequest message.

A missing dependency occurs when a solution requires a component that is not present in the target environment. For example, if a solution contains a form that uses a custom attribute, but that attribute is not included in the solution and doesn't exist in the target environment, it will be reported as a missing dependency.

This cmdlet is particularly useful before importing a solution to identify potential issues that would prevent successful import.

The cmdlet returns a collection of missing dependency entities, each describing a required component that is not present.

## EXAMPLES

### Example 1: Check for missing dependencies before import
```powershell
PS C:\> $missingDeps = Get-DataverseMissingDependency -Connection $c -SolutionUniqueName "MyCustomSolution"
PS C:\> if ($missingDeps) {
>>     Write-Host "Cannot import solution - found $($missingDeps.Count) missing dependencies"
>>     $missingDeps | Format-Table missingcomponentid, missingcomponenttype
>> } else {
>>     Write-Host "All dependencies satisfied - ready to import"
>> }
```

Checks for missing dependencies before importing a solution.

### Example 2: List missing dependencies with details
```powershell
PS C:\> $missingDeps = Get-DataverseMissingDependency -SolutionUniqueName "TestSolution"
PS C:\> $missingDeps | ForEach-Object {
>>     [PSCustomObject]@{
>>         MissingComponentId = $_.missingcomponentid
>>         MissingComponentType = $_.missingcomponenttype
>>         RequiredComponentId = $_.requiredcomponentid
>>         RequiredComponentType = $_.requiredcomponenttype
>>     }
>> } | Format-Table -AutoSize
```

Creates a detailed report of all missing dependencies.

### Example 3: Check multiple solutions
```powershell
PS C:\> $solutions = "Solution1", "Solution2", "Solution3"
PS C:\> $solutions | ForEach-Object {
>>     $missing = Get-DataverseMissingDependency -SolutionUniqueName $_
>>     if ($missing) {
>>         [PSCustomObject]@{
>>             SolutionName = $_
>>             MissingCount = $missing.Count
>>         }
>>     }
>> } | Format-Table
```

Checks multiple solutions for missing dependencies and reports counts.

### Example 4: Using UniqueName alias
```powershell
PS C:\> $solution = Get-DataverseSolution -Name "MySolution"
PS C:\> $missingDeps = $solution | Get-DataverseMissingDependency
```

Uses the UniqueName alias to accept solution objects from the pipeline.

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
Unique name of the solution to check for missing dependencies.

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
Returns missing dependency entities with information about each missing component.

## NOTES
- This cmdlet uses the RetrieveMissingDependenciesRequest SDK message.
- The returned entities contain properties like missingcomponentid, missingcomponenttype, requiredcomponentid, and requiredcomponenttype.
- An empty result means all dependencies are satisfied.
- This check should be performed before solution import to avoid import failures.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseDependency](Get-DataverseDependency.md)
[Get-DataverseDependentComponent](Get-DataverseDependentComponent.md)
[Get-DataverseUninstallDependency](Get-DataverseUninstallDependency.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
