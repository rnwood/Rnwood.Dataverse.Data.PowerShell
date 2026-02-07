. $PSScriptRoot/Common.ps1

Describe 'Import-DataverseSolution - Version Check Logic' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a solution zip with specified version
        function New-TestSolutionZip {
            param(
                [string]$UniqueName,
                [string]$Version,
                [bool]$IsManaged = $false
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
            $entry = $zip.CreateEntry("solution.xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write($solutionXml)
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

    Context "Version extraction from solution file" {
        
        It "Extracts version from solution file correctly" {
            $solutionPath = New-TestSolutionZip -UniqueName "TestSolutionVersionCheck" -Version "1.2.3.4"
            $Script:TestSolutionPaths += $solutionPath
            
            # Test that Get-DataverseSolutionFile can read the version
            $info = Get-DataverseSolutionFile -Path $solutionPath
            
            $info | Should -Not -BeNullOrEmpty
            $info.Version | Should -Be ([Version]"1.2.3.4")
            $info.UniqueName | Should -Be "TestSolutionVersionCheck"
        }
    }

    Context "SkipIfSameVersion parameter exists and is documented" {
        
        It "SkipIfSameVersion parameter exists on Import-DataverseSolution cmdlet" {
            $cmd = Get-Command Import-DataverseSolution
            $param = $cmd.Parameters["SkipIfSameVersion"]
            
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be "SwitchParameter"
        }
        
        It "SkipIfSameVersion parameter has help message" {
            $cmd = Get-Command Import-DataverseSolution
            $param = $cmd.Parameters["SkipIfSameVersion"]
            
            $helpMessage = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -ExpandProperty HelpMessage -First 1
            
            $helpMessage | Should -Not -BeNullOrEmpty
            $helpMessage | Should -BeLike "*same*version*"
        }
    }

    Context "SkipIfLowerVersion parameter exists and is documented" {
        
        It "SkipIfLowerVersion parameter exists on Import-DataverseSolution cmdlet" {
            $cmd = Get-Command Import-DataverseSolution
            $param = $cmd.Parameters["SkipIfLowerVersion"]
            
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be "SwitchParameter"
        }
        
        It "SkipIfLowerVersion parameter has help message" {
            $cmd = Get-Command Import-DataverseSolution
            $param = $cmd.Parameters["SkipIfLowerVersion"]
            
            $helpMessage = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -ExpandProperty HelpMessage -First 1
            
            $helpMessage | Should -Not -BeNullOrEmpty
            $helpMessage | Should -BeLike "*lower*version*"
        }
    }
}
