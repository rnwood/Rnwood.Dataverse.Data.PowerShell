. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSolutionFileComponent' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a test solution zip with environment variables and connection references
        function New-TestSolutionZipWithComponents {
            param(
                [switch]$IncludeEnvironmentVariable,
                [switch]$IncludeConnectionReference,
                [switch]$IncludeEntity
            )
            
            # Create a minimal solution.xml with root components
            $rootComponentsXml = ""
            if ($IncludeEntity) {
                # Add an entity as a root component
                $rootComponentsXml = @"
    <RootComponents>
      <RootComponent type="1" schemaName="contact" behavior="0" />
    </RootComponents>
"@
            } else {
                $rootComponentsXml = "<RootComponents />"
            }
            
            $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>TestSolution</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>testpublisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description="Test Publisher" languagecode="1033" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
$rootComponentsXml
  </SolutionManifest>
</ImportExportXml>
"@

            # Create customizations.xml with connection reference if requested
            if ($IncludeConnectionReference) {
                $customizationsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227">
  <Entities />
  <connectionreferences>
    <connectionreference connectionreferenceid="12345678-1234-1234-1234-123456789012" connectionreferencelogicalname="test_connref1" />
  </connectionreferences>
</ImportExportXml>
"@
            } else {
                $customizationsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227">
  <Entities />
</ImportExportXml>
"@
            }
            
            # Create environment variable definition XML if requested
            $envVarXml = @"
<?xml version="1.0" encoding="utf-8"?>
<environmentvariabledefinition environmentvariabledefinitionid="87654321-4321-4321-4321-210987654321" schemaname="test_EnvVar1">
  <displayname description="Test Environment Variable" languagecode="1033" />
  <type>100000000</type>
</environmentvariabledefinition>
"@
            
            # Create zip file
            $tempDir = [IO.Path]::GetTempPath()
            $testSolutionPath = Join-Path $tempDir "TestSolutionWithComponents_$(New-Guid).zip"
            
            # Load required assemblies
            try {
                [System.Reflection.Assembly]::Load("System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") | Out-Null
                [System.Reflection.Assembly]::Load("System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") | Out-Null
            } catch {
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
            
            # Add environment variable if requested
            if ($IncludeEnvironmentVariable) {
                $entry = $zip.CreateEntry("environmentvariabledefinitions/test_EnvVar1/environmentvariabledefinition.xml")
                $writer = New-Object System.IO.StreamWriter($entry.Open())
                $writer.Write($envVarXml)
                $writer.Flush()
                $writer.Close()
            }
            
            $zip.Dispose()
            $stream.Close()
            $stream.Dispose()
            
            # Wait a moment to ensure file is fully written
            Start-Sleep -Milliseconds 200
            
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

    It "Discovers environment variables from solution file" {
        $conn = getMockConnection
        $testSolutionPath = New-TestSolutionZipWithComponents -IncludeEnvironmentVariable
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFileComponent -Connection $conn -SolutionFile $testSolutionPath
        
        $result | Should -Not -BeNullOrEmpty
        $envVarComponent = $result | Where-Object { $_.ComponentType -eq 380 }
        $envVarComponent | Should -Not -BeNullOrEmpty
        # ObjectId property in the output contains the UniqueName (schemaname) for display purposes
        $envVarComponent.ObjectId | Should -Be "test_EnvVar1"
        $envVarComponent.ComponentType | Should -Be 380
    }

    It "Discovers connection references from solution file" {
        $conn = getMockConnection
        $testSolutionPath = New-TestSolutionZipWithComponents -IncludeConnectionReference
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFileComponent -Connection $conn -SolutionFile $testSolutionPath
        
        $result | Should -Not -BeNullOrEmpty
        $connRefComponent = $result | Where-Object { $_.ComponentType -eq 635 }
        $connRefComponent | Should -Not -BeNullOrEmpty
        # ObjectId property in the output contains the UniqueName (logical name) for display purposes
        $connRefComponent.ObjectId | Should -Be "test_connref1"
        $connRefComponent.ComponentType | Should -Be 635
    }

    It "Discovers both environment variables and connection references" {
        $conn = getMockConnection
        $testSolutionPath = New-TestSolutionZipWithComponents -IncludeEnvironmentVariable -IncludeConnectionReference
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFileComponent -Connection $conn -SolutionFile $testSolutionPath
        
        $result | Should -Not -BeNullOrEmpty
        
        $envVarComponent = $result | Where-Object { $_.ComponentType -eq 380 }
        $envVarComponent | Should -Not -BeNullOrEmpty
        $envVarComponent.ObjectId | Should -Be "test_EnvVar1"
        
        $connRefComponent = $result | Where-Object { $_.ComponentType -eq 635 }
        $connRefComponent | Should -Not -BeNullOrEmpty
        $connRefComponent.ObjectId | Should -Be "test_connref1"
    }

    It "Discovers all component types including root components" {
        $conn = getMockConnection -Entities @("contact")
        $testSolutionPath = New-TestSolutionZipWithComponents -IncludeEnvironmentVariable -IncludeConnectionReference -IncludeEntity
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFileComponent -Connection $conn -SolutionFile $testSolutionPath
        
        $result | Should -Not -BeNullOrEmpty
        
        # Should have entity from RootComponents
        $entityComponent = $result | Where-Object { $_.ComponentType -eq 1 }
        $entityComponent | Should -Not -BeNullOrEmpty
        
        # Should have environment variable
        $envVarComponent = $result | Where-Object { $_.ComponentType -eq 380 }
        $envVarComponent | Should -Not -BeNullOrEmpty
        
        # Should have connection reference
        $connRefComponent = $result | Where-Object { $_.ComponentType -eq 635 }
        $connRefComponent | Should -Not -BeNullOrEmpty
    }

    It "Returns empty result when no components are present" {
        $conn = getMockConnection
        $testSolutionPath = New-TestSolutionZipWithComponents
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFileComponent -Connection $conn -SolutionFile $testSolutionPath
        
        # Should return no components (empty array or null)
        if ($result) {
            $result.Count | Should -Be 0
        }
    }
}
