---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Test-DataverseSolution

## SYNOPSIS
Validates a Dataverse solution against best practices and common issues.

## SYNTAX

```
Test-DataverseSolution [-Connection <ServiceClient>] [-UniqueName] <String> [-IncludeInfo] 
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Test-DataverseSolution cmdlet validates a Dataverse solution against a set of best practice rules to identify potential issues before deployment. It checks for common problems such as:

- Managed tables included with "Include Subcomponents" behavior (Rule SV001)
- Unmodified managed non-table components included unnecessarily (Rule SV002)
- Unmodified managed table subcomponents included unnecessarily (Rule SV003)

Each validation rule has a unique identifier and detailed documentation explaining why the issue matters and how to fix it.

## EXAMPLES

### Example 1: Validate a solution and display results
```powershell
PS C:\> $connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Test-DataverseSolution -Connection $connection -UniqueName "MySolution"

SolutionUniqueName : MySolution
IsValid            : False
Issues             : {SV001: Managed table 'contact' is included with 'Include Subcomponents'...}
TotalComponents    : 45
ValidationTimestamp: 2024-01-15T10:30:00Z
ErrorCount         : 1
WarningCount       : 3
InfoCount          : 0
```

This command validates the solution named "MySolution" and displays the results, showing any issues found.

### Example 2: Validate a solution with verbose output
```powershell
PS C:\> Test-DataverseSolution -Connection $connection -UniqueName "MySolution" -Verbose

VERBOSE: Starting validation of solution 'MySolution'...
VERBOSE: Found solution with ID: 12345678-1234-1234-1234-123456789abc
VERBOSE: Found 45 components in solution
VERBOSE: Validating Rule SV001: Managed tables with 'Include Subcomponents'...
VERBOSE: Rule SV001: Found 1 violations
VERBOSE: Validating Rule SV002: Managed non-table components not customized...
VERBOSE: Rule SV002: Found 2 violations
VERBOSE: Validating Rule SV003: Managed subcomponents not customized...
VERBOSE: Rule SV003: Found 1 violations
VERBOSE: Validation complete. Found 1 errors, 3 warnings, 0 info messages
```

This command validates the solution with verbose output showing the validation progress and summary.

### Example 3: Check validation results and export issues
```powershell
PS C:\> $results = Test-DataverseSolution -Connection $connection -UniqueName "MySolution"
PS C:\> if (-not $results.IsValid) {
    Write-Host "Solution has $($results.ErrorCount) errors and $($results.WarningCount) warnings"
    $results.Issues | Export-Csv -Path "validation-issues.csv" -NoTypeInformation
}
```

This example validates the solution, checks if it's valid, and exports the issues to a CSV file for review.

### Example 4: Filter and review specific rule violations
```powershell
PS C:\> $results = Test-DataverseSolution -Connection $connection -UniqueName "MySolution"
PS C:\> $sv001Issues = $results.Issues | Where-Object { $_.RuleId -eq 'SV001' }
PS C:\> foreach ($issue in $sv001Issues) {
    Write-Host "Error: $($issue.Message)"
    Write-Host "Component: $($issue.ComponentIdentifier)"
    Write-Host "Documentation: $($issue.DocumentationUrl)"
    Write-Host ""
}
```

This example filters validation results to show only SV001 violations (managed tables with Include Subcomponents).

### Example 5: Validate before solution export
```powershell
PS C:\> $results = Test-DataverseSolution -Connection $connection -UniqueName "MySolution"
PS C:\> if ($results.IsValid) {
    Export-DataverseSolution -Connection $connection -SolutionName "MySolution" -Path ".\MySolution.zip"
    Write-Host "Solution validated and exported successfully"
} else {
    Write-Warning "Solution has validation issues. Fix them before exporting."
    $results.Issues | Format-Table RuleId, Severity, Message -AutoSize
}
```

This example demonstrates a best practice workflow: validate the solution before exporting it.

### Example 6: Use WhatIf to preview validation
```powershell
PS C:\> Test-DataverseSolution -Connection $connection -UniqueName "MySolution" -WhatIf

What if: Performing the operation "Validate solution" on target "MySolution".
```

This shows what would happen without actually performing the validation.

## PARAMETERS

### -Connection
The Dataverse connection to use. If not specified, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -UniqueName
The unique name of the solution to validate.

```yaml
Type: String
Parameter Sets: (All)
Aliases: SolutionName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -IncludeInfo
Include informational messages in the validation output. By default, only warnings and errors are included.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
You can pipe a solution unique name to this cmdlet.

## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationResult
This cmdlet returns a SolutionValidationResult object containing:
- IsValid: Boolean indicating if the solution passed all validation rules
- Issues: List of SolutionValidationIssue objects for each problem found
- TotalComponents: Total number of components validated
- ErrorCount, WarningCount, InfoCount: Counts of issues by severity

## NOTES

### Validation Rules

#### SV001: Managed Table Include Subcomponents (Error)
Managed tables should not be included with "Include Subcomponents" behavior. This can cause conflicts when the managed table has been customized in the target environment.

**Documentation:** [SV001](solution-validation-rules/SV001.md)

#### SV002: Managed Non-Table Not Customized (Warning)
Managed non-table components (web resources, workflows, plugins, etc.) should only be included if they have been customized. Including unmodified managed components bloats the solution unnecessarily.

**Documentation:** [SV002](solution-validation-rules/SV002.md)

#### SV003: Managed Subcomponent Not Customized (Warning)
Managed table subcomponents (attributes, relationships, forms, views) should only be included if they have been customized. Including unmodified managed subcomponents is unnecessary.

**Documentation:** [SV003](solution-validation-rules/SV003.md)

### Best Practices
- Run validation before exporting solutions
- Fix all errors before deploying to production
- Review and address warnings to keep solutions clean
- Use verbose output to understand what's being checked
- Document any intentional rule violations

### Common Issues and Solutions
**Issue:** "Solution not found"
**Solution:** Check that the solution unique name is correct and that you have access to it.

**Issue:** Validation takes a long time
**Solution:** Large solutions with many components may take time to validate. Use -Verbose to see progress.

## RELATED LINKS

[Get-DataverseSolution](Get-DataverseSolution.md)
[Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
[Export-DataverseSolution](Export-DataverseSolution.md)
[Solution Validation Rules](solution-validation-rules/README.md)
[Power Platform ALM Guide](https://learn.microsoft.com/en-us/power-platform/alm/)
