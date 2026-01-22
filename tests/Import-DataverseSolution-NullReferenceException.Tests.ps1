. $PSScriptRoot/Common.ps1

Describe 'Import-DataverseSolution - NullReferenceException Bug Fix' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a solution zip without connection references or environment variables
        function New-TestSolutionWithoutComponents {
            param(
                [string]$UniqueName,
                [string]$Version,
                [bool]$IsManaged = $true
            )
            
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
            
            # Add customizations.xml
            $entry = $zip.CreateEntry("customizations.xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write($customizationsXml)
            $writer.Flush()
            $writer.Close()
            
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

    Context "Import solution without connection references or environment variables" {
        
        It "Does not throw NullReferenceException when solution has no components (new solution)" {
            # Create a solution with no connection references or environment variables
            $solutionPath = New-TestSolutionWithoutComponents `
                -UniqueName "SchemaMigrationTracking" `
                -Version "1.0.0.0" `
                -IsManaged $true
            $Script:TestSolutionPaths += $solutionPath
            
            # Mock connection - no solution exists yet
            $mock = getMockConnection -Entities @("solution")
            
            # Import should not throw NullReferenceException
            # This is testing the fix for the bug where GetComponentParameters() returns null
            # and the code tries to access componentParameters.Entities.Any() without null check
            # Using -WhatIf to prevent actual import execution
            { 
                Import-DataverseSolution `
                    -Connection $mock `
                    -InFile $solutionPath `
                    -WhatIf
            } | Should -Not -Throw
        }
        
        It "Does not throw NullReferenceException with UseUpdateIfVersionMajorMinorMatches flag" {
            # Create a solution with no connection references or environment variables
            $solutionPath = New-TestSolutionWithoutComponents `
                -UniqueName "SchemaMigrationTracking2" `
                -Version "1.0.0.0" `
                -IsManaged $true
            $Script:TestSolutionPaths += $solutionPath
            
            # Mock connection - no solution exists yet
            $mock = getMockConnection -Entities @("solution")
            
            # Import should not throw NullReferenceException
            # This tests the scenario from the bug report
            # Using -WhatIf to prevent actual import execution
            { 
                Import-DataverseSolution `
                    -Connection $mock `
                    -InFile $solutionPath `
                    -UseUpdateIfVersionMajorMinorMatches `
                    -SkipIfSameVersion `
                    -WhatIf
            } | Should -Not -Throw
        }
    }
}
