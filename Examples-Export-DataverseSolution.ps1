# Example: Export a Solution using Export-DataverseSolution
# This example demonstrates the new Export-DataverseSolution cmdlet

# Connect to Dataverse (replace with your actual connection details)
# $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# Example 1: Export an unmanaged solution to a file with progress monitoring
# Export-DataverseSolution -Connection $connection -SolutionName "MySolution" -OutFile "C:\Exports\MySolution.zip" -Verbose

# Example 2: Export a managed solution and capture the bytes in a variable
# $solutionBytes = Export-DataverseSolution -Connection $connection -SolutionName "MySolution" -Managed -PassThru

# Example 3: Export a solution with settings included
# Export-DataverseSolution -Connection $connection -SolutionName "MySolution" `
#     -ExportAutoNumberingSettings `
#     -ExportCalendarSettings `
#     -ExportCustomizationSettings `
#     -OutFile "C:\Exports\MySolution_WithSettings.zip"

# Example 4: Export a large solution with custom timeout and polling interval
# Export-DataverseSolution -Connection $connection -SolutionName "LargeSolution" `
#     -OutFile "C:\Exports\LargeSolution.zip" `
#     -TimeoutSeconds 1200 `
#     -PollingIntervalSeconds 10 `
#     -Verbose

# Example 5: Export solution and save to both file and variable
# $bytes = Export-DataverseSolution -Connection $connection -SolutionName "MySolution" `
#     -OutFile "C:\Exports\MySolution.zip" `
#     -PassThru

# Example 6: Use WhatIf to see what would happen without actually exporting
# Export-DataverseSolution -Connection $connection -SolutionName "MySolution" `
#     -OutFile "C:\Exports\MySolution.zip" `
#     -WhatIf

Write-Host "These are example commands for Export-DataverseSolution cmdlet"
Write-Host "Uncomment and modify the examples above to use them with your environment"
