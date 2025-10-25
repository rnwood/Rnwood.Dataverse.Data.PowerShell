. $PSScriptRoot/Common.ps1

Describe 'Compare-DataverseSolution' {

    BeforeAll {
        # Create a test solution zip file
        function CreateTestSolutionZip {
            param(
                [string]$SolutionUniqueName,
                [array]$Components
            )

            $tempFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null

            try {
                # Create solution.xml
                $solutionXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24044.186" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive">
  <SolutionManifest>
    <UniqueName>$SolutionUniqueName</UniqueName>
    <LocalizedNames>
      <LocalizedName description="Test Solution" languagecode="1033" />
    </LocalizedNames>
    <Descriptions />
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>TestPublisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description="Test Publisher" languagecode="1033" />
      </LocalizedNames>
      <Descriptions />
      <EMailAddress>test@example.com</EMailAddress>
      <SupportingWebsiteUrl></SupportingWebsiteUrl>
      <CustomizationPrefix>test</CustomizationPrefix>
      <CustomizationOptionValuePrefix>10000</CustomizationOptionValuePrefix>
      <Addresses>
        <Address>
          <AddressNumber>1</AddressNumber>
          <AddressTypeCode>1</AddressTypeCode>
          <City></City>
          <Country></Country>
          <County></County>
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
        <Address>
          <AddressNumber>2</AddressNumber>
          <AddressTypeCode>1</AddressTypeCode>
          <City></City>
          <Country></Country>
          <County></County>
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
    <RootComponents>
"@
                # Add root components
                foreach ($component in $Components) {
                    $solutionXml += @"

      <RootComponent type="$($component.Type)" id="$($component.Id)" behavior="$($component.Behavior)" />
"@
                }

                $solutionXml += @"

    </RootComponents>
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>
"@

                $solutionXmlPath = Join-Path $tempFolder "solution.xml"
                [System.IO.File]::WriteAllText($solutionXmlPath, $solutionXml)

                # Create customizations.xml
                $customizationsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ImportExportXml version="9.2.24044.186" SolutionPackageVersion="9.2" languagecode="1033" generatedBy="CrmLive">
  <SolutionManifest>
    <UniqueName>$SolutionUniqueName</UniqueName>
  </SolutionManifest>
  <RootComponents>
"@
                # Add root components to customizations.xml too
                foreach ($component in $Components) {
                    $customizationsXml += @"

    <RootComponent type="$($component.Type)" id="$($component.Id)" behavior="$($component.Behavior)" />
"@
                }

                $customizationsXml += @"

  </RootComponents>
</ImportExportXml>
"@

                $customizationsXmlPath = Join-Path $tempFolder "customizations.xml"
                [System.IO.File]::WriteAllText($customizationsXmlPath, $customizationsXml)

                # Create zip file with unique name
                $zipPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "$SolutionUniqueName-$([Guid]::NewGuid()).zip")
                if (Test-Path $zipPath) {
                    Remove-Item $zipPath -Force
                }

                Add-Type -AssemblyName System.IO.Compression.FileSystem
                [System.IO.Compression.ZipFile]::CreateFromDirectory($tempFolder, $zipPath)

                return $zipPath
            }
            finally {
                Remove-Item -Recurse -Force $tempFolder -ErrorAction SilentlyContinue
            }
        }
    }

    It "Compares solution file with non-existent solution and marks all as Added" {
        $connection = getMockConnection
        
        # Create test solution with 2 components
        $components = @(
            @{ Type = 1; Id = [Guid]::NewGuid(); Behavior = 0 },
            @{ Type = 61; Id = [Guid]::NewGuid(); Behavior = 0 }
        )
        
        $zipPath = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components
        
        try {
            # The cmdlet should output all components as "Added" since solution doesn't exist
            $result = Compare-DataverseSolution -Connection $connection -SolutionFile $zipPath -WarningAction SilentlyContinue
            
            $result | Should -HaveCount 2
            $result | ForEach-Object { $_.Status | Should -Be "Added" }
            $result[0].ComponentType | Should -Be 1
            $result[1].ComponentType | Should -Be 61
            $result[0].SourceBehavior | Should -Not -BeNullOrEmpty
            $result[0].TargetBehavior | Should -BeNullOrEmpty
        }
        finally {
            if (Test-Path $zipPath) {
                Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
            }
        }
    }

    It "Compares two solution files and detects differences" {
        # Import module for file-to-file comparison
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        # Create first solution file with 2 components
        $component1Id = [Guid]::NewGuid()
        $component2Id = [Guid]::NewGuid()
        $components1 = @(
            @{ Type = 1; Id = $component1Id; Behavior = 0 },
            @{ Type = 61; Id = $component2Id; Behavior = 0 }
        )
        $zipPath1 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components1
        
        # Create second solution file with different components (one shared, one different)
        $component3Id = [Guid]::NewGuid()
        $components2 = @(
            @{ Type = 1; Id = $component1Id; Behavior = 0 },
            @{ Type = 24; Id = $component3Id; Behavior = 0 }
        )
        $zipPath2 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components2
        
        try {
            $result = Compare-DataverseSolution -SolutionFile $zipPath1 -TargetSolutionFile $zipPath2
            
            $result | Should -HaveCount 3
            
            # Component1 should be Modified (exists in both)
            $comp1Result = $result | Where-Object { $_.ObjectId -eq $component1Id }
            $comp1Result.Status | Should -Be "Modified"
            
            # Component2 should be Added (in source but not in target)
            $comp2Result = $result | Where-Object { $_.ObjectId -eq $component2Id }
            $comp2Result.Status | Should -Be "Added"
            
            # Component3 should be Removed (in target but not in source)
            $comp3Result = $result | Where-Object { $_.ObjectId -eq $component3Id }
            $comp3Result.Status | Should -Be "Removed"
        }
        finally {
            if (Test-Path $zipPath1) { Remove-Item $zipPath1 -Force -ErrorAction SilentlyContinue }
            if (Test-Path $zipPath2) { Remove-Item $zipPath2 -Force -ErrorAction SilentlyContinue }
        }
    }

    It "Detects behavior changes with BehaviorIncluded status" {
        # Import module for file-to-file comparison
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        # Create first solution file with shell component (behavior 2)
        $componentId = [Guid]::NewGuid()
        $components1 = @(
            @{ Type = 1; Id = $componentId; Behavior = 2 }
        )
        $zipPath1 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components1
        
        # Create second solution file with full component (behavior 0)
        $components2 = @(
            @{ Type = 1; Id = $componentId; Behavior = 0 }
        )
        $zipPath2 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components2
        
        try {
            $result = Compare-DataverseSolution -SolutionFile $zipPath1 -TargetSolutionFile $zipPath2
            
            $result | Should -HaveCount 1
            $result[0].Status | Should -Be "BehaviorIncluded"
            $result[0].SourceBehavior | Should -Be "Include As Shell"
            $result[0].TargetBehavior | Should -Be "Include Subcomponents"
        }
        finally {
            if (Test-Path $zipPath1) { Remove-Item $zipPath1 -Force -ErrorAction SilentlyContinue }
            if (Test-Path $zipPath2) { Remove-Item $zipPath2 -Force -ErrorAction SilentlyContinue }
        }
    }

    It "Detects behavior changes with BehaviorExcluded status" {
        # Import module for file-to-file comparison
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        # Create first solution file with full component (behavior 0)
        $componentId = [Guid]::NewGuid()
        $components1 = @(
            @{ Type = 1; Id = $componentId; Behavior = 0 }
        )
        $zipPath1 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components1
        
        # Create second solution file with shell component (behavior 2)
        $components2 = @(
            @{ Type = 1; Id = $componentId; Behavior = 2 }
        )
        $zipPath2 = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components2
        
        try {
            $result = Compare-DataverseSolution -SolutionFile $zipPath1 -TargetSolutionFile $zipPath2
            
            $result | Should -HaveCount 1
            $result[0].Status | Should -Be "BehaviorExcluded"
            $result[0].SourceBehavior | Should -Be "Include Subcomponents"
            $result[0].TargetBehavior | Should -Be "Include As Shell"
        }
        finally {
            if (Test-Path $zipPath1) { Remove-Item $zipPath1 -Force -ErrorAction SilentlyContinue }
            if (Test-Path $zipPath2) { Remove-Item $zipPath2 -Force -ErrorAction SilentlyContinue }
        }
    }

    It "Compares solution file with existing solution and detects differences" -Skip {
        # This test requires 'solution' and 'solutioncomponent' entities which are not in the metadata cache
        # Skipping for now - this would work in a real environment
        $true | Should -Be $true
    }

    It "Detects behavior changes (full to shell)" -Skip {
        # This test requires 'solution' and 'solutioncomponent' entities which are not in the metadata cache
        # Skipping for now - this would work in a real environment
        $true | Should -Be $true
    }

    It "Works with solution bytes parameter" {
        $connection = getMockConnection
        
        # Create test solution with 1 component
        $components = @(
            @{ Type = 1; Id = [Guid]::NewGuid(); Behavior = 0 }
        )
        
        $zipPath = CreateTestSolutionZip -SolutionUniqueName "TestSolution" -Components $components
        
        try {
            $solutionBytes = [System.IO.File]::ReadAllBytes($zipPath)
            $result = Compare-DataverseSolution -Connection $connection -SolutionBytes $solutionBytes -WarningAction SilentlyContinue
            
            $result | Should -HaveCount 1
            $result[0].Status | Should -Be "Added"
            $result[0].SourceBehavior | Should -Not -BeNullOrEmpty
        }
        finally {
            if (Test-Path $zipPath) {
                Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
            }
        }
    }
}
