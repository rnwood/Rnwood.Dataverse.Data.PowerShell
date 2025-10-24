# Example: Import a Solution using Import-DataverseSolution
# This example demonstrates the new Import-DataverseSolution cmdlet

# Connect to Dataverse (replace with your actual connection details)
# $connection = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive

# Example 1: Import a solution from a file with progress monitoring
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\MySolution.zip" -Verbose

# Example 2: Import with connection references set
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\MySolution.zip" `
#     -ConnectionReferences @{
#         'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
#         'new_sql' = '87654321-4321-4321-4321-210987654321'
#     }

# Example 3: Import as holding solution for upgrade (auto-fallback to regular import if not exists)
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\MySolution_v2.zip" `
#     -HoldingSolution `
#     -Verbose

# Example 4: Import with overwrite and publish workflows
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\MySolution.zip" `
#     -OverwriteUnmanagedCustomizations `
#     -PublishWorkflows `
#     -Verbose

# Example 5: Import solution from bytes (e.g., from Export-DataverseSolution)
# $bytes = Export-DataverseSolution -Connection $sourceConn -SolutionName "MySolution" -PassThru
# $bytes | Import-DataverseSolution -Connection $targetConn -OverwriteUnmanagedCustomizations

# Example 6: Import large solution with custom timeout and polling
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\LargeSolution.zip" `
#     -TimeoutSeconds 3600 `
#     -PollingIntervalSeconds 10 `
#     -Verbose

# Example 7: Use WhatIf to see what would happen without actually importing
# Import-DataverseSolution -Connection $connection -InFile "C:\Solutions\MySolution.zip" `
#     -WhatIf

# Example 8: Complete export-import workflow between environments
# # Export from source
# $solutionBytes = Export-DataverseSolution -Connection $sourceConnection `
#     -SolutionName "MySolution" `
#     -Managed `
#     -PassThru
#
# # Import to target with connection references
# $result = $solutionBytes | Import-DataverseSolution -Connection $targetConnection `
#     -OverwriteUnmanagedCustomizations `
#     -ConnectionReferences @{
#         'new_prod_sharepoint' = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
#     }
#
# Write-Host "Import completed. Job ID: $($result.ImportJobId)"

Write-Host "These are example commands for Import-DataverseSolution cmdlet"
Write-Host "Uncomment and modify the examples above to use them with your environment"
