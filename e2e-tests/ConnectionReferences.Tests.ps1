$ErrorActionPreference = "Stop"

Describe "Connection References E2E Tests" -Skip {

    BeforeAll {
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder

        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can import solution with connection references using connector name fallback and specific logical name overrides" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'Continue'  # Enable verbose output
        
        # Retry helper function with exponential backoff
        function Invoke-WithRetry {
            param(
                [Parameter(Mandatory = $true)]
                [scriptblock]$ScriptBlock,
                [int]$MaxRetries = 5,
                [int]$InitialDelaySeconds = 10
            )
                
            $attempt = 0
            $delay = $InitialDelaySeconds
                
            while ($attempt -lt $MaxRetries) {
                try {
                    $attempt++
                    Write-Verbose "Attempt $attempt of $MaxRetries"
                    & $ScriptBlock
                    return  # Success, exit function
                }
                catch {
                    if ($attempt -eq $MaxRetries) {
                        Write-Error "All $MaxRetries attempts failed. Last error: $_"
                        throw
                    }
                        
                    Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
                    Start-Sleep -Seconds $delay
                    $delay = $delay * 2  # Exponential backoff
                }
            }
        }
            
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true    

            # Generate unique test identifiers with timestamp to enable age-based cleanup
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmm")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $solutionName = "E2EConnRef_${timestamp}_$testRunId"
            $publisherPrefix = "e2ecr"
                
            Write-Host "Test solution: $solutionName"
            Write-Host "Publisher prefix: $publisherPrefix"
                
            # Step 1: Cleanup any old test solutions and connection references from previous failed runs (older than 2 hours)
            Write-Host "Step 1: Cleanup old test artifacts from previous failed runs..."
            Invoke-WithRetry {
                $cutoffTime = [DateTime]::UtcNow.AddHours(-2)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                # Clean up old solutions
                $oldSolutions = Invoke-DataverseSql -Connection $connection -Sql "SELECT uniquename FROM solution WHERE uniquename LIKE 'E2EConnRef_%'" |
                    Where-Object { 
                        $_.uniquename -match "E2EConnRef_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp
                    }
                
                if ($oldSolutions.Count -gt 0) {
                    Write-Host "  Found $($oldSolutions.Count) old test solutions (>2 hours old) to clean up"
                    foreach ($sol in $oldSolutions) {
                        try {
                            Write-Host "    Deleting solution: $($sol.uniquename)"
                            Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM solution WHERE uniquename = '$($sol.uniquename)'" -Confirm:$false
                        }
                        catch {
                            Write-Warning "    Failed to delete solution $($sol.uniquename): $_"
                        }
                    }
                }
                
                # Clean up old connection references
                $oldConnRefs = Get-DataverseRecord -Connection $connection -TableName connectionreference -Columns connectionreferenceid, connectionreferencelogicalname |
                    Where-Object { 
                        $_.connectionreferencelogicalname -like "${publisherPrefix}_connref_*" -and
                        $_.connectionreferencelogicalname -match "${publisherPrefix}_connref_(\d{12})_" -and 
                        $matches[1] -lt $cutoffTimestamp
                    }
                
                if ($oldConnRefs.Count -gt 0) {
                    Write-Host "  Found $($oldConnRefs.Count) old test connection references (>2 hours old) to clean up"
                    foreach ($connRef in $oldConnRefs) {
                        try {
                            Write-Host "    Deleting connection reference: $($connRef.connectionreferencelogicalname)"
                            Remove-DataverseRecord -Connection $connection -TableName connectionreference -Id $connRef.connectionreferenceid -Confirm:$false
                        }
                        catch {
                            Write-Warning "    Failed to delete connection reference $($connRef.connectionreferencelogicalname): $_"
                        }
                    }
                }
            }
            Write-Host "✓ Cleanup completed"

            # Step 2: Find existing connections to use for the test
            Write-Host "Step 2: Finding existing Dataverse connections to use..."
            
            # List existing connections with the Dataverse connector
            $existingConnections = Get-DataverseRecord -Connection $connection -TableName connectioninstance -Columns connectioninstanceid, name, connectorid -FilterValues @{ connectorid = '/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps' }
            
            if ($existingConnections.Count -eq 0) {
                throw "No existing Dataverse connections found. At least one connection with connector 'shared_commondataserviceforapps' is required for this test."
            }
            
            # Use the first available Dataverse connection
            $dataverseConnectionId = $existingConnections[0].connectioninstanceid
            Write-Host "  Using existing Dataverse connection: $dataverseConnectionId (Name: $($existingConnections[0].name))"
            
            # For override testing, use a second connection if available, otherwise reuse the first
            if ($existingConnections.Count -gt 1) {
                $overrideConnectionId = $existingConnections[1].connectioninstanceid
                Write-Host "  Using second Dataverse connection for override: $overrideConnectionId (Name: $($existingConnections[1].name))"
            }
            else {
                $overrideConnectionId = $dataverseConnectionId
                Write-Host "  Reusing same Dataverse connection for override (only one available)"
            }
            
            Write-Host "✓ Connections selected"

            # Step 3: Create a test solution with connection references
            Write-Host "Step 3: Creating test solution with connection references..."
            
            # Create solution XML with connection references (all using Dataverse connector)
            $connRef1LogicalName = "${publisherPrefix}_connref_${timestamp}_${testRunId}_dv1"
            $connRef2LogicalName = "${publisherPrefix}_connref_${timestamp}_${testRunId}_dv2"
            $connRef3LogicalName = "${publisherPrefix}_connref_${timestamp}_${testRunId}_dv3"
            
            $customizationsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227">
  <connectionreferences>
    <connectionreference connectionreferencelogicalname="$connRef1LogicalName">
      <connectorid>/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps</connectorid>
    </connectionreference>
    <connectionreference connectionreferencelogicalname="$connRef2LogicalName">
      <connectorid>/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps</connectorid>
    </connectionreference>
    <connectionreference connectionreferencelogicalname="$connRef3LogicalName">
      <connectorid>/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps</connectorid>
    </connectionreference>
  </connectionreferences>
</ImportExportXml>
"@

            $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24082.227" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SolutionManifest>
    <UniqueName>$solutionName</UniqueName>
    <LocalizedNames>
      <LocalizedName description="E2E Connection Reference Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Descriptions>
      <Description description="Test solution for connection reference connector name fallback" languagecode="1033" />
    </Descriptions>
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>${publisherPrefix}testpub</UniqueName>
      <LocalizedNames>
        <LocalizedName description="E2E Test Publisher" languagecode="1033" />
      </LocalizedNames>
      <Descriptions>
        <Description description="Publisher for E2E tests" languagecode="1033" />
      </Descriptions>
      <EMailAddress>test@example.com</EMailAddress>
      <SupportingWebsiteUrl>https://example.com</SupportingWebsiteUrl>
      <CustomizationPrefix>$publisherPrefix</CustomizationPrefix>
      <CustomizationOptionValuePrefix>10000</CustomizationOptionValuePrefix>
      <Addresses>
        <Address>
          <AddressNumber>1</AddressNumber>
          <AddressTypeCode>1</AddressTypeCode>
          <City></City>
          <County></County>
          <Country></Country>
          <Fax></Fax>
          <FreightTermsCode></FreightTermsCode>
          <ImportSequenceNumber></ImportSequenceNumber>
          <Latitude></Latitude>
          <Line1></Line1>
          <Line2></Line2>
          <Line3></Line3>
          <Longitude></Longitude>
          <Name></Name>
          <PostalCode></PostalCode>
          <PostOfficeBox></PostOfficeBox>
          <PrimaryContactName></PrimaryContactName>
          <ShippingMethodCode>1</ShippingMethodCode>
          <StateOrProvince></StateOrProvince>
          <Telephone1></Telephone1>
          <Telephone2></Telephone2>
          <Telephone3></Telephone3>
          <TimeZoneRuleVersionNumber></TimeZoneRuleVersionNumber>
          <UPSZone></UPSZone>
          <UTCOffset></UTCOffset>
          <UTCConversionTimeZoneCode></UTCConversionTimeZoneCode>
        </Address>
      </Addresses>
    </Publisher>
  </SolutionManifest>
</ImportExportXml>
"@

            $contentTypesXml = @"
<?xml version="1.0" encoding="utf-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="xml" ContentType="application/octet-stream" />
</Types>
"@

            # Create solution zip file
            $solutionZipPath = [IO.Path]::Combine([IO.Path]::GetTempPath(), "${solutionName}.zip")
            
            Add-Type -AssemblyName System.IO.Compression
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            
            $stream = [System.IO.File]::Create($solutionZipPath)
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
            
            # Add [Content_Types].xml
            $entry = $zip.CreateEntry("[Content_Types].xml")
            $writer = New-Object System.IO.StreamWriter($entry.Open())
            $writer.Write($contentTypesXml)
            $writer.Flush()
            $writer.Close()
            
            $zip.Dispose()
            $stream.Dispose()
            
            Write-Host "  Created solution package: $solutionZipPath"
            Write-Host "✓ Test solution package created"

            # Step 4: Pre-create one connection reference in Dataverse (to test pre-existing scenario)
            Write-Host "Step 4: Creating pre-existing connection reference in Dataverse..."
            
            $preExistingConnRefId = [guid]::NewGuid()
            $preExistingConnRef = @{
                connectionreferenceid = $preExistingConnRefId
                connectionreferencelogicalname = $connRef1LogicalName
                connectionreferencedisplayname = "Pre-existing Dataverse Connection Reference 1"
                connectorid = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps"
                connectionid = $overrideConnectionId  # Already has a connection value
            }
            
            Invoke-WithRetry {
                Set-DataverseRecord -Connection $connection -TableName connectionreference -Record $preExistingConnRef -CreateOnly -Confirm:$false
            }
            Write-Host "  Created pre-existing connection reference: $connRef1LogicalName (with existing connection value)"
            Write-Host "✓ Pre-existing connection reference created"

            # Step 5: Import the solution with connection reference mappings
            Write-Host "Step 5: Importing solution with connector name fallback and specific override..."
            
            # Test the connector name fallback feature:
            # - Use 'shared_commondataserviceforapps' (connector name) to map all Dataverse connection references
            # - Override the first one specifically by its logical name
            $connectionReferences = @{
                # Connector name fallback - will map to connRef2 and connRef3
                'shared_commondataserviceforapps' = $dataverseConnectionId.ToString()
                # Specific logical name override - will map to connRef1 (overriding the connector name fallback)
                $connRef1LogicalName = $overrideConnectionId.ToString()
            }
            
            Invoke-WithRetry {
                Import-DataverseSolution -Connection $connection `
                    -InFile $solutionZipPath `
                    -ConnectionReferences $connectionReferences `
                    -OverwriteUnmanagedCustomizations `
                    -Confirm:$false `
                    -Verbose
            }
            Write-Host "✓ Solution imported successfully"

            # Step 6: Verify the connection references were set correctly
            Write-Host "Step 6: Verifying connection reference mappings..."
            
            # Verify connRef1 (pre-existing, should now use override connection due to specific logical name mapping)
            $connRef1 = Get-DataverseRecord -Connection $connection -TableName connectionreference `
                -FilterValues @{ connectionreferencelogicalname = $connRef1LogicalName } `
                -Columns connectionreferenceid, connectionreferencelogicalname, connectionid, connectorid
            
            if ($null -eq $connRef1) {
                throw "Connection reference 1 not found after import"
            }
            
            if ($connRef1.connectionid -ne $overrideConnectionId.ToString()) {
                throw "Connection reference 1 should have override connection ID $overrideConnectionId but has $($connRef1.connectionid)"
            }
            Write-Host "  ✓ Connection reference 1 has correct override connection (specific logical name took precedence)"
            
            # Verify connRef2 (new, should use Dataverse connection via connector name fallback)
            $connRef2 = Get-DataverseRecord -Connection $connection -TableName connectionreference `
                -FilterValues @{ connectionreferencelogicalname = $connRef2LogicalName } `
                -Columns connectionreferenceid, connectionreferencelogicalname, connectionid, connectorid
            
            if ($null -eq $connRef2) {
                throw "Connection reference 2 not found after import"
            }
            
            if ($connRef2.connectionid -ne $dataverseConnectionId.ToString()) {
                throw "Connection reference 2 should have Dataverse connection ID $dataverseConnectionId but has $($connRef2.connectionid)"
            }
            Write-Host "  ✓ Connection reference 2 has correct Dataverse connection (connector name fallback worked)"
            
            # Verify connRef3 (new, should use Dataverse connection via connector name fallback)
            $connRef3 = Get-DataverseRecord -Connection $connection -TableName connectionreference `
                -FilterValues @{ connectionreferencelogicalname = $connRef3LogicalName } `
                -Columns connectionreferenceid, connectionreferencelogicalname, connectionid, connectorid
            
            if ($null -eq $connRef3) {
                throw "Connection reference 3 not found after import"
            }
            
            if ($connRef3.connectionid -ne $dataverseConnectionId.ToString()) {
                throw "Connection reference 3 should have Dataverse connection ID $dataverseConnectionId but has $($connRef3.connectionid)"
            }
            Write-Host "  ✓ Connection reference 3 has correct Dataverse connection (connector name fallback worked)"
            Write-Host "✓ All connection reference mappings verified successfully"

            Write-Host ""
            Write-Host "=== Test Summary ==="
            Write-Host "✓ Connector name fallback feature working correctly:"
            Write-Host "  - Connection reference 1: Overridden by specific logical name (takes precedence)"
            Write-Host "  - Connection reference 2: Mapped via 'shared_commondataserviceforapps' connector name"
            Write-Host "  - Connection reference 3: Mapped via 'shared_commondataserviceforapps' connector name"
            Write-Host "✓ Pre-existing connection reference updated successfully"
            Write-Host "✓ New connection references created and mapped successfully"

        }
        finally {
            # Cleanup: Delete test artifacts
            Write-Host "Cleaning up test artifacts..."
            
            try {
                if ($connection -and $solutionName) {
                    Write-Host "  Deleting test solution: $solutionName"
                    Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM solution WHERE uniquename = '$solutionName'" -Confirm:$false -ErrorAction SilentlyContinue
                }
                
                if ($connection -and $connRef1LogicalName) {
                    Write-Host "  Deleting connection reference 1: $connRef1LogicalName"
                    Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM connectionreference WHERE connectionreferencelogicalname = '$connRef1LogicalName'" -Confirm:$false -ErrorAction SilentlyContinue
                }
                
                if ($connection -and $connRef2LogicalName) {
                    Write-Host "  Deleting connection reference 2: $connRef2LogicalName"
                    Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM connectionreference WHERE connectionreferencelogicalname = '$connRef2LogicalName'" -Confirm:$false -ErrorAction SilentlyContinue
                }
                
                if ($connection -and $connRef3LogicalName) {
                    Write-Host "  Deleting connection reference 3: $connRef3LogicalName"
                    Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM connectionreference WHERE connectionreferencelogicalname = '$connRef3LogicalName'" -Confirm:$false -ErrorAction SilentlyContinue
                }
                
                # Note: Not deleting connections since they are existing environment connections
                
                if ($solutionZipPath -and (Test-Path $solutionZipPath)) {
                    Write-Host "  Deleting solution zip file: $solutionZipPath"
                    Remove-Item $solutionZipPath -Force -ErrorAction SilentlyContinue
                }
            }
            catch {
                Write-Warning "Error during cleanup: $_"
            }
            
            Write-Host "✓ Cleanup completed"
        }
    }
}
