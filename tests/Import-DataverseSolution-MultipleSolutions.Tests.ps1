. $PSScriptRoot/Common.ps1

Describe 'Import-DataverseSolution - Multiple Solutions' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a solution zip with specified name and version
        function New-TestSolutionZip {
            param(
                [string]$UniqueName,
                [string]$Version = "1.0.0.0",
                [bool]$IsManaged = $false
            )
            
            $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>$UniqueName</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution $UniqueName" languagecode="1033" />
    </LocalizedNames>
    <Descriptions>
      <Description description="This is test solution $UniqueName" languagecode="1033" />
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
<ImportExportXml xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Entities />
  <Roles />
  <Workflows />
  <FieldSecurityProfiles />
  <Templates />
  <EntityMaps />
  <EntityRelationships />
  <OrganizationSettings />
  <optionsets />
  <CustomControls />
  <EntityDataProviders />
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
            $entry2 = $zip.CreateEntry("customizations.xml")
            $writer2 = New-Object System.IO.StreamWriter($entry2.Open())
            $writer2.Write($customizationsXml)
            $writer2.Flush()
            $writer2.Close()
            
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

    Context "Multiple solution file import" {
        
        It "Accepts array of solution file paths" {
            # Create three test solution files
            $solution1Path = New-TestSolutionZip -UniqueName "TestSolution1" -Version "1.0.0.0"
            $solution2Path = New-TestSolutionZip -UniqueName "TestSolution2" -Version "1.0.0.0"
            $solution3Path = New-TestSolutionZip -UniqueName "TestSolution3" -Version "1.0.0.0"
            
            $Script:TestSolutionPaths += $solution1Path
            $Script:TestSolutionPaths += $solution2Path
            $Script:TestSolutionPaths += $solution3Path
            
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to import with WhatIf to verify the cmdlet accepts multiple files
            # We use WhatIf so we don't actually try to execute the import
            { Import-DataverseSolution -InFile $solution1Path, $solution2Path, $solution3Path -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Accepts array of solution file paths via array syntax" {
            # Create three test solution files
            $solution1Path = New-TestSolutionZip -UniqueName "TestSolutionArray1" -Version "1.0.0.0"
            $solution2Path = New-TestSolutionZip -UniqueName "TestSolutionArray2" -Version "1.0.0.0"
            $solution3Path = New-TestSolutionZip -UniqueName "TestSolutionArray3" -Version "1.0.0.0"
            
            $Script:TestSolutionPaths += $solution1Path
            $Script:TestSolutionPaths += $solution2Path
            $Script:TestSolutionPaths += $solution3Path
            
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to import with WhatIf using array syntax
            { Import-DataverseSolution -InFile @($solution1Path, $solution2Path, $solution3Path) -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Still accepts single solution file (backward compatibility)" {
            # Create a single test solution file
            $solutionPath = New-TestSolutionZip -UniqueName "TestSolutionSingle" -Version "1.0.0.0"
            $Script:TestSolutionPaths += $solutionPath
            
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to import single file with WhatIf
            { Import-DataverseSolution -InFile $solutionPath -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Fails if any solution file doesn't exist" {
            # Create one valid solution file
            $solution1Path = New-TestSolutionZip -UniqueName "TestSolutionExists" -Version "1.0.0.0"
            $Script:TestSolutionPaths += $solution1Path
            
            # Use a non-existent file path
            $nonExistentPath = Join-Path ([IO.Path]::GetTempPath()) "NonExistent_$(New-Guid).zip"
            
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to import - should throw because second file doesn't exist
            { Import-DataverseSolution -InFile $solution1Path, $nonExistentPath -Connection $connection -ErrorAction Stop } | Should -Throw
        }
    }

    Context "InFile parameter validation" {
        
        It "InFile parameter accepts string array type" {
            $paramInfo = (Get-Command Import-DataverseSolution).Parameters['InFile']
            $paramInfo | Should -Not -BeNullOrEmpty
            $paramInfo.ParameterType.Name | Should -Be 'String[]'
        }
        
        It "InFile parameter is mandatory for FromFile parameter set" {
            $paramInfo = (Get-Command Import-DataverseSolution).Parameters['InFile']
            $paramInfo.Attributes | Where-Object { $_.TypeId.Name -eq 'ParameterAttribute' } | 
                Where-Object { $_.ParameterSetName -eq 'FromFile' } | 
                Select-Object -First 1 -ExpandProperty Mandatory | Should -Be $true
        }
    }
}
