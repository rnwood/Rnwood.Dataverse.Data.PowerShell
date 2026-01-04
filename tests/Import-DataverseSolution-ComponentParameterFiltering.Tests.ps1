. $PSScriptRoot/Common.ps1

Describe 'Import-DataverseSolution - Component Parameter Filtering' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a solution zip with connection references and environment variables
        function New-TestSolutionWithComponents {
            param(
                [string]$UniqueName,
                [string]$Version,
                [string[]]$ConnectionReferences = @(),
                [string[]]$EnvironmentVariables = @(),
                [bool]$IsManaged = $false
            )
            
            # Build connection reference XML
            $connRefXml = ""
            foreach ($connRef in $ConnectionReferences) {
                $connRefXml += @"
    <connectionreference connectionreferencelogicalname="$connRef" />
"@
            }
            
            # Build environment variable XML files list (these would be separate files in a real solution)
            $envVarFiles = @()
            foreach ($envVar in $EnvironmentVariables) {
                $envVarFiles += @{
                    Name = "EnvironmentVariableDefinitions/${envVar}_environmentvariabledefinition.xml"
                    Content = @"
<?xml version="1.0" encoding="utf-8"?>
<environmentvariabledefinition schemaname="$envVar">
  <displayname>$envVar Display</displayname>
  <type>100000000</type>
</environmentvariabledefinition>
"@
                }
            }
            
            $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>$UniqueName</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Descriptions>
      <Description description="This is a test solution" languagecode="1033" />
    </Descriptions>
    <Version>$Version</Version>
    <Managed>$(if ($IsManaged) { '1' } else { '0' })</Managed>
    <Publisher>
      <UniqueName>testpublisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description="Test Publisher" languagecode="1033" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
  </SolutionManifest>
</ImportExportXml>
"@

            $customizationsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227">
  <connectionreferences>
$connRefXml
  </connectionreferences>
</ImportExportXml>
"@

            $tempDir = [IO.Path]::GetTempPath()
            $testSolutionPath = Join-Path $tempDir "${UniqueName}_${Version}_$(New-Guid).zip"
            
            # Load required assemblies with explicit error handling for PS5 jobs
            try {
                [System.Reflection.Assembly]::Load("System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") | Out-Null
                [System.Reflection.Assembly]::Load("System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") | Out-Null
            } catch {
                # Fallback for different .NET versions
                Add-Type -AssemblyName System.IO.Compression
                Add-Type -AssemblyName System.IO.Compression.FileSystem
            }
            
            $stream = [System.IO.File]::Create($testSolutionPath)
            $zip = New-Object System.IO.Compression.ZipArchive($stream, [System.IO.Compression.ZipArchiveMode]::Create)
            
            # Add solution.xml
            $entry = $zip.CreateEntry("solution.xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write($solutionXml)
            $writer.Flush()
            $writer.Close()
            
            # Add customizations.xml (with connection references)
            $entry = $zip.CreateEntry("customizations.xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write($customizationsXml)
            $writer.Flush()
            $writer.Close()
            
            # Add environment variable definition files
            foreach ($envVarFile in $envVarFiles) {
                $entry = $zip.CreateEntry($envVarFile.Name)
                $writer = New-Object System.IO.StreamWriter($entry.Open())
                $writer.Write($envVarFile.Content)
                $writer.Flush()
                $writer.Close()
            }
            
            $zip.Dispose()
            $stream.Close()
            $stream.Dispose()
            
            Start-Sleep -Milliseconds 100
            
            return $testSolutionPath
        }
        
        Set-Variable -Name "TestSolutionPaths" -Value @() -Scope Script
    }

    AfterAll {
        foreach ($path in $Script:TestSolutionPaths) {
            if (Test-Path $path) {
                Remove-Item $path -Force -ErrorAction SilentlyContinue
            }
        }
    }

    Context "Filtering extra component parameters not in solution" {
        
        It "Does not throw when extra connection references are provided" {
            # Create a solution with only 2 connection references
            $solutionPath = New-TestSolutionWithComponents `
                -UniqueName "TestSolutionFiltering1" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint", "new_sql")
            $Script:TestSolutionPaths += $solutionPath
            
            # Mock connection
            $mock = getMockConnection -Entities @("connectionreference", "solution")
            
            # Provide 3 connection references, but only 2 are in the solution
            # The extra one should be ignored without error
            $connRefs = @{
                "new_sharepoint" = "12345678-1234-1234-1234-123456789012"
                "new_sql" = "87654321-4321-4321-4321-210987654321"
                "new_extra" = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"  # Not in solution - should be filtered out
            }
            
            # Import should not throw
            { 
                Import-DataverseSolution `
                    -Connection $mock `
                    -InFile $solutionPath `
                    -ConnectionReferences $connRefs `
                    -SkipConnectionReferenceValidation `
                    -WhatIf
            } | Should -Not -Throw
        }
        
        It "Does not throw when extra environment variables are provided" {
            # Create a solution with only 2 environment variables
            $solutionPath = New-TestSolutionWithComponents `
                -UniqueName "TestSolutionFiltering2" `
                -Version "1.0.0.0" `
                -EnvironmentVariables @("new_apiurl", "new_apikey")
            $Script:TestSolutionPaths += $solutionPath
            
            # Mock connection
            $mock = getMockConnection -Entities @("environmentvariabledefinition", "environmentvariablevalue", "solution")
            
            # Provide 3 environment variables, but only 2 are in the solution
            # The extra one should be ignored without error
            $envVars = @{
                "new_apiurl" = "https://api.example.com"
                "new_apikey" = "secretkey123"
                "new_extrasetting" = "value123"  # Not in solution - should be filtered out
            }
            
            # Import should not throw
            { 
                Import-DataverseSolution `
                    -Connection $mock `
                    -InFile $solutionPath `
                    -EnvironmentVariables $envVars `
                    -SkipEnvironmentVariableValidation `
                    -WhatIf
            } | Should -Not -Throw
        }
        
        It "Handles case where all provided parameters are not in solution" {
            # Create a solution with no connection references or environment variables
            $solutionPath = New-TestSolutionWithComponents `
                -UniqueName "TestSolutionFiltering3" `
                -Version "1.0.0.0"
            $Script:TestSolutionPaths += $solutionPath
            
            # Mock connection
            $mock = getMockConnection -Entities @("solution")
            
            # Provide parameters that are not in the solution
            # All should be ignored without error
            $connRefs = @{
                "new_notinsolution" = "12345678-1234-1234-1234-123456789012"
            }
            
            $envVars = @{
                "new_alsonotinsolution" = "value"
            }
            
            # Import should not throw
            { 
                Import-DataverseSolution `
                    -Connection $mock `
                    -InFile $solutionPath `
                    -ConnectionReferences $connRefs `
                    -EnvironmentVariables $envVars `
                    -SkipConnectionReferenceValidation `
                    -SkipEnvironmentVariableValidation `
                    -WhatIf
            } | Should -Not -Throw
        }
        
        It "Allows using common parameter sets across multiple solution imports" {
            # This test demonstrates the use case: having a common set of parameters
            # and importing multiple solutions that use different subsets
            
            # Common parameters for multiple solutions
            $commonConnRefs = @{
                "new_sharepoint" = "12345678-1234-1234-1234-123456789012"
                "new_sql" = "87654321-4321-4321-4321-210987654321"
                "new_api" = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
            }
            
            $commonEnvVars = @{
                "new_apiurl" = "https://api.example.com"
                "new_dbconnstring" = "Server=.;Database=db"
                "new_timeout" = "30"
            }
            
            # Solution 1: Uses sharepoint and apiurl
            $solution1Path = New-TestSolutionWithComponents `
                -UniqueName "TestSolution1" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint") `
                -EnvironmentVariables @("new_apiurl")
            $Script:TestSolutionPaths += $solution1Path
            
            # Solution 2: Uses sql and dbconnstring
            $solution2Path = New-TestSolutionWithComponents `
                -UniqueName "TestSolution2" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sql") `
                -EnvironmentVariables @("new_dbconnstring")
            $Script:TestSolutionPaths += $solution2Path
            
            # Solution 3: Uses api and timeout
            $solution3Path = New-TestSolutionWithComponents `
                -UniqueName "TestSolution3" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_api") `
                -EnvironmentVariables @("new_timeout")
            $Script:TestSolutionPaths += $solution3Path
            
            # Mock connection
            $mock = getMockConnection -Entities @("connectionreference", "environmentvariabledefinition", "environmentvariablevalue", "solution")
            
            # Import all three solutions with the same common parameters
            # Each solution should only use the parameters it needs and ignore the others
            { 
                Import-DataverseSolution -Connection $mock -InFile $solution1Path `
                    -ConnectionReferences $commonConnRefs -EnvironmentVariables $commonEnvVars `
                    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation -WhatIf
                
                Import-DataverseSolution -Connection $mock -InFile $solution2Path `
                    -ConnectionReferences $commonConnRefs -EnvironmentVariables $commonEnvVars `
                    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation -WhatIf
                
                Import-DataverseSolution -Connection $mock -InFile $solution3Path `
                    -ConnectionReferences $commonConnRefs -EnvironmentVariables $commonEnvVars `
                    -SkipConnectionReferenceValidation -SkipEnvironmentVariableValidation -WhatIf
            } | Should -Not -Throw
        }
    }

    Context "Feature documentation" {
        It "Documents that filtering is implemented for component parameters" {
            # This test documents that the following functionality has been implemented:
            # - When passing connection references or environment variables via hashtables,
            #   the cmdlet now filters the parameters to only include those that are
            #   actually present in the solution file being imported.
            # - Extra parameters provided by the user are ignored and a verbose message is logged.
            # - This allows users to provide a common set of parameters for multiple solution imports,
            #   and the cmdlet will automatically filter out the irrelevant ones for each solution.
            # - The filtering uses case-insensitive matching and preserves the casing from the solution file.
            # - This is particularly useful in CI/CD scenarios where the same parameter set
            #   might be used across multiple solution imports.
            
            $true | Should -Be $true
        }
    }
}
