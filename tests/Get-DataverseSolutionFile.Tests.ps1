. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSolutionFile' {

    BeforeAll {
        # Set module path
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        New-Item -ItemType Directory $tempmodulefolder | Out-Null
        Copy-Item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder
        
        Import-Module Rnwood.Dataverse.Data.PowerShell -Force
        
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
        
        Set-Variable -Name "TestSolutionPath" -Value $testSolutionPath -Scope Script
    }

    AfterAll {
        if (Test-Path $Script:TestSolutionPath) {
            Remove-Item $Script:TestSolutionPath -Force -ErrorAction SilentlyContinue
        }
    }

    It "Parses solution file and returns metadata" {
        $result = Get-DataverseSolutionFile -Path $Script:TestSolutionPath
        
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
        Start-Sleep -Milliseconds 100  # Allow file to be fully released
        $bytes = [System.IO.File]::ReadAllBytes($Script:TestSolutionPath)
        $result = $bytes | Get-DataverseSolutionFile
        
        $result | Should -Not -BeNullOrEmpty
        $result.UniqueName | Should -Be "TestSolution"
        $result.Name | Should -Be "Test Solution"
    }
    
    It "Returns error for missing file" {
        { Get-DataverseSolutionFile -Path "/nonexistent/path.zip" -ErrorAction Stop } | Should -Throw "*not found*"
    }
}
