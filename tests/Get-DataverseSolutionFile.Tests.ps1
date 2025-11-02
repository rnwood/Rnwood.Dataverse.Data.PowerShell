. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSolutionFile' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a test solution zip
        function New-TestSolutionZip {
            # Create a minimal test solution.xml content
            $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>TestSolution</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Descriptions>
      <Description description="This is a test solution" languagecode="1033" />
    </Descriptions>
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
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

            # Create a minimal test solution zip file
            $tempDir = [IO.Path]::GetTempPath()
            $testSolutionPath = Join-Path $tempDir "TestSolution_$(New-Guid).zip"
            
            # Create zip with solution.xml
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            
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

    It "Parses solution file and returns metadata" {
        $testSolutionPath = New-TestSolutionZip
        $Script:TestSolutionPaths += $testSolutionPath
        
        $result = Get-DataverseSolutionFile -Path $testSolutionPath
        
        $result | Should -Not -BeNullOrEmpty
        $result.UniqueName | Should -Be "TestSolution"
        $result.Name | Should -Be "Test Solution"
        $result.Description | Should -Be "This is a test solution"
        $result.Version | Should -Be ([Version]"1.0.0.0")
        $result.IsManaged | Should -Be $false
        $result.PublisherName | Should -Be "Test Publisher"
        $result.PublisherUniqueName | Should -Be "testpublisher"
        $result.PublisherPrefix | Should -Be "test"
    }

    It -Skip "Parses solution file from bytes" {
        $testSolutionPath = New-TestSolutionZip
        $Script:TestSolutionPaths += $testSolutionPath
        
        Start-Sleep -Milliseconds 100  # Allow file to be fully released
        $bytes = [System.IO.File]::ReadAllBytes($testSolutionPath)
        $result = $bytes | Get-DataverseSolutionFile
        
        $result | Should -Not -BeNullOrEmpty
        $result.UniqueName | Should -Be "TestSolution"
        $result.Name | Should -Be "Test Solution"
    }
    
    It "Returns error for missing file" {
        { Get-DataverseSolutionFile -Path "/nonexistent/path.zip" -ErrorAction Stop } | Should -Throw "*not found*"
    }
}
