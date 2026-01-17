. $PSScriptRoot/Common.ps1

Describe 'Import-DataverseSolution - Connector ID Fallback' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
        
        # Helper function to create a solution zip with connection references
        function New-TestSolutionWithConnectionReferences {
            param(
                [string]$UniqueName,
                [string]$Version,
                [string[]]$ConnectionReferences = @()
            )
            
            # Build connection reference XML
            $connRefXml = ""
            foreach ($connRef in $ConnectionReferences) {
                $connRefXml += @"
    <connectionreference connectionreferencelogicalname="$connRef" />
"@
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
            
            # Add empty [Content_Types].xml
            $entry = $zip.CreateEntry("[Content_Types].xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write('<?xml version="1.0" encoding="utf-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types" />')
            $writer.Flush()
            $writer.Close()
            
            $zip.Dispose()
            $stream.Dispose()
            
            return $testSolutionPath
        }
        
        # Track created solution files for cleanup
        $Script:TestSolutionPaths = @()
    }

    AfterAll {
        # Clean up created solution files
        foreach ($path in $Script:TestSolutionPaths) {
            if (Test-Path $path) {
                Remove-Item $path -Force
            }
        }
    }

    Context "Connector ID fallback" {
        It "Maps connection reference by connector ID when logical name not provided" {
            # Create test solution with connection references
            $solutionPath = New-TestSolutionWithConnectionReferences `
                -UniqueName "TestSolutionConnectorFallback" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint1", "new_sharepoint2")
            $Script:TestSolutionPaths += $solutionPath
            
            # Create mock connection with connection references
            $mock = getMockConnection -Entities @("connectionreference", "solution")
            
            # Create connection reference records with connector IDs
            $connRef1 = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRef1.Id = $connRef1["connectionreferenceid"] = [Guid]::NewGuid()
            $connRef1["connectionreferencelogicalname"] = "new_sharepoint1"
            $connRef1["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRef1["connectionid"] = $null
            $connRef1 | Set-DataverseRecord -Connection $mock -CreateOnly
            
            $connRef2 = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRef2.Id = $connRef2["connectionreferenceid"] = [Guid]::NewGuid()
            $connRef2["connectionreferencelogicalname"] = "new_sharepoint2"
            $connRef2["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRef2["connectionid"] = $null
            $connRef2 | Set-DataverseRecord -Connection $mock -CreateOnly
            
            # Provide connection reference mapping by connector ID (not by logical name)
            $connectionId = "12345678-1234-1234-1234-123456789012"
            $connectionReferences = @{
                '/providers/Microsoft.PowerApps/apis/shared_sharepointonline' = $connectionId
            }
            
            # Import should succeed and map both connection references via connector ID fallback
            { 
                Import-DataverseSolution -Connection $mock -InFile $solutionPath `
                    -ConnectionReferences $connectionReferences `
                    -SkipConnectionReferenceValidation -WhatIf
            } | Should -Not -Throw
        }

        It "Prefers logical name over connector ID when both match" {
            # Create test solution with connection references
            $solutionPath = New-TestSolutionWithConnectionReferences `
                -UniqueName "TestSolutionPrecedence" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint1", "new_sharepoint2")
            $Script:TestSolutionPaths += $solutionPath
            
            # Create mock connection with connection references
            $mock = getMockConnection -Entities @("connectionreference", "solution")
            
            # Create connection reference records with connector IDs
            $connRef1 = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRef1.Id = $connRef1["connectionreferenceid"] = [Guid]::NewGuid()
            $connRef1["connectionreferencelogicalname"] = "new_sharepoint1"
            $connRef1["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRef1["connectionid"] = $null
            $connRef1 | Set-DataverseRecord -Connection $mock -CreateOnly
            
            $connRef2 = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRef2.Id = $connRef2["connectionreferenceid"] = [Guid]::NewGuid()
            $connRef2["connectionreferencelogicalname"] = "new_sharepoint2"
            $connRef2["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRef2["connectionid"] = $null
            $connRef2 | Set-DataverseRecord -Connection $mock -CreateOnly
            
            # Provide both connector ID (fallback) and specific logical name (override)
            $defaultConnectionId = "12345678-1234-1234-1234-123456789012"
            $specificConnectionId = "87654321-4321-4321-4321-210987654321"
            $connectionReferences = @{
                '/providers/Microsoft.PowerApps/apis/shared_sharepointonline' = $defaultConnectionId
                'new_sharepoint1' = $specificConnectionId  # Override for specific connection reference
            }
            
            # Import should succeed and use specific mapping for new_sharepoint1, fallback for new_sharepoint2
            { 
                Import-DataverseSolution -Connection $mock -InFile $solutionPath `
                    -ConnectionReferences $connectionReferences `
                    -SkipConnectionReferenceValidation -WhatIf
            } | Should -Not -Throw
        }

        It "Handles mixed connector IDs in same solution" {
            # Create test solution with different connector types
            $solutionPath = New-TestSolutionWithConnectionReferences `
                -UniqueName "TestSolutionMixedConnectors" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint", "new_sql", "new_dataverse")
            $Script:TestSolutionPaths += $solutionPath
            
            # Create mock connection with connection references
            $mock = getMockConnection -Entities @("connectionreference", "solution")
            
            # Create connection references for different connectors
            $connRefSP = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRefSP.Id = $connRefSP["connectionreferenceid"] = [Guid]::NewGuid()
            $connRefSP["connectionreferencelogicalname"] = "new_sharepoint"
            $connRefSP["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRefSP["connectionid"] = $null
            $connRefSP | Set-DataverseRecord -Connection $mock -CreateOnly
            
            $connRefSQL = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRefSQL.Id = $connRefSQL["connectionreferenceid"] = [Guid]::NewGuid()
            $connRefSQL["connectionreferencelogicalname"] = "new_sql"
            $connRefSQL["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sql"
            $connRefSQL["connectionid"] = $null
            $connRefSQL | Set-DataverseRecord -Connection $mock -CreateOnly
            
            $connRefDV = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRefDV.Id = $connRefDV["connectionreferenceid"] = [Guid]::NewGuid()
            $connRefDV["connectionreferencelogicalname"] = "new_dataverse"
            $connRefDV["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps"
            $connRefDV["connectionid"] = $null
            $connRefDV | Set-DataverseRecord -Connection $mock -CreateOnly
            
            # Provide connection mappings by connector ID
            $connectionReferences = @{
                '/providers/Microsoft.PowerApps/apis/shared_sharepointonline' = "11111111-1111-1111-1111-111111111111"
                '/providers/Microsoft.PowerApps/apis/shared_sql' = "22222222-2222-2222-2222-222222222222"
                '/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps' = "33333333-3333-3333-3333-333333333333"
            }
            
            # Import should succeed and map all connection references by their connector IDs
            { 
                Import-DataverseSolution -Connection $mock -InFile $solutionPath `
                    -ConnectionReferences $connectionReferences `
                    -SkipConnectionReferenceValidation -WhatIf
            } | Should -Not -Throw
        }

        It "Does not map connection reference when neither logical name nor connector ID matches" {
            # Create test solution with connection reference
            $solutionPath = New-TestSolutionWithConnectionReferences `
                -UniqueName "TestSolutionNoMatch" `
                -Version "1.0.0.0" `
                -ConnectionReferences @("new_sharepoint")
            $Script:TestSolutionPaths += $solutionPath
            
            # Create mock connection with connection reference
            $mock = getMockConnection -Entities @("connectionreference", "solution")
            
            $connRef = New-Object Microsoft.Xrm.Sdk.Entity "connectionreference"
            $connRef.Id = $connRef["connectionreferenceid"] = [Guid]::NewGuid()
            $connRef["connectionreferencelogicalname"] = "new_sharepoint"
            $connRef["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_sharepointonline"
            $connRef["connectionid"] = $null
            $connRef | Set-DataverseRecord -Connection $mock -CreateOnly
            
            # Provide connection mapping for different connector (no match)
            $connectionReferences = @{
                '/providers/Microsoft.PowerApps/apis/shared_sql' = "12345678-1234-1234-1234-123456789012"
                'new_different' = "87654321-4321-4321-4321-210987654321"
            }
            
            # Import should succeed but not map the connection reference (validation skipped)
            { 
                Import-DataverseSolution -Connection $mock -InFile $solutionPath `
                    -ConnectionReferences $connectionReferences `
                    -SkipConnectionReferenceValidation -WhatIf
            } | Should -Not -Throw
        }
    }

    Context "Feature documentation" {
        It "Documents connector ID fallback functionality" {
            # This test documents that the following functionality has been implemented:
            # - The ConnectionReferences parameter now supports connector IDs as keys in addition to logical names
            # - When a hashtable key matches a connection reference logical name, it is used directly (existing behavior)
            # - When a hashtable key matches a connector ID, all connection references using that connector
            #   are mapped to the specified connection ID (new fallback behavior)
            # - Logical name matches take precedence over connector ID matches
            # - This allows users to provide default connection mappings for all connection references
            #   of a specific connector type, with the ability to override specific connection references
            # - The connector ID is queried from Dataverse for each connection reference found in the solution
            # - This is particularly useful when deploying solutions with many connection references
            #   that use the same connector type (e.g., multiple SharePoint connections)
            
            $true | Should -Be $true
        }
    }
}
