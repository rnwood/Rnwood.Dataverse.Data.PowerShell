. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSitemap' {
    Context 'Cmdlet Structure' {
        It "Get-DataverseSitemap cmdlet exists and has expected parameters" {
            $command = Get-Command Get-DataverseSitemap -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Name'
            $command.Parameters.Keys | Should -Contain 'Id'
            $command.Parameters.Keys | Should -Contain 'UniqueName'
            $command.Parameters.Keys | Should -Contain 'Unpublished'
            $command.Parameters.Keys | Should -Contain 'Connection'
        }
    }
}

Describe 'Set-DataverseSitemap' {
    Context 'Cmdlet Structure' {
        It "Set-DataverseSitemap cmdlet exists and has expected parameters" {
            $command = Get-Command Set-DataverseSitemap -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Name'
            $command.Parameters.Keys | Should -Contain 'Id'
            $command.Parameters.Keys | Should -Contain 'SitemapXml'
            $command.Parameters.Keys | Should -Contain 'PassThru'
            $command.Parameters.Keys | Should -Contain 'Publish'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }

        It "Rejects invalid XML" {
            $connection = getMockConnection
            
            $invalidXml = "<SiteMap><Area Id='Test'"
            
            # Should throw error for invalid XML
            { Set-DataverseSitemap -Connection $connection -Name "InvalidSitemap" -SitemapXml $invalidXml } | Should -Throw
        }

        It "Creates new sitemap when UniqueName is provided but doesn't exist" {
            $connection = getMockConnection
            
            # Use a unique name that definitely doesn't exist
            $uniqueName = "TestUniqueName_$(Get-Random)"
            
            # This should create a new sitemap since the unique name doesn't exist
            $results = Set-DataverseSitemap -Connection $connection -Name "Test Sitemap" -UniqueName $uniqueName -PassThru
            
            # Should return multiple objects: message string and GUID
            $results | Should -HaveCount 2
            $results[0] | Should -BeOfType [string]
            $results[1] | Should -BeOfType [Guid]
            $results[1] | Should -Not -Be ([Guid]::Empty)
        }

        It "Updates sitemap without SitemapXml parameter preserves existing XML" {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create a custom sitemap XML
            $customXml = @"
<SiteMap IntroducedVersion="7.0.0.0">
  <Area Id="CustomArea" ResourceId="Custom.Title" DescriptionResourceId="Custom.Title" ShowGroups="true" IntroducedVersion="7.0.0.0">
    <Titles><Title LCID="1033" Title="Custom Area"/></Titles>
    <Group Id="CustomGroup" ResourceId="Custom.Group" DescriptionResourceId="Custom.Group" IntroducedVersion="7.0.0.0" IsProfile="false" ToolTipResourseId="SitemapDesigner.Unknown">
      <SubArea Id="CustomSubArea" Icon="/_imgs/imagestrips/transparent_spacer.gif" Entity="account" Client="All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web" AvailableOffline="true" PassParams="false" Sku="All,OnPremise,Live,SPLA"/>
    </Group>
  </Area>
</SiteMap>
"@
            
            # Create a sitemap with custom XML
            $createResult = Set-DataverseSitemap -Connection $connection -Name "Test Sitemap" -UniqueName "TestUniqueName" -SitemapXml $customXml -PassThru
            $sitemapId = $createResult[1]
            
            # Verify the sitemap was created with custom XML
            $createdSitemap = Get-DataverseSitemap -Connection $connection -Id $sitemapId
            $createdSitemap.SitemapXml | Should -Be $customXml
            
            # Now update with different name but no SitemapXml
            Set-DataverseSitemap -Connection $connection -UniqueName "TestUniqueName" -Name "Updated Name"
            
            # Verify the SitemapXml was not overwritten with default
            $updatedSitemap = Get-DataverseSitemap -Connection $connection -Id $sitemapId
            $updatedSitemap.SitemapXml | Should -Be $customXml
            $updatedSitemap.Name | Should -Be "Updated Name"
        }
    }
}

Describe 'Remove-DataverseSitemap' {
    Context 'Cmdlet Structure' {
        It "Remove-DataverseSitemap cmdlet exists and has expected parameters" {
            $command = Get-Command Remove-DataverseSitemap -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Name'
            $command.Parameters.Keys | Should -Contain 'Id'
            $command.Parameters.Keys | Should -Contain 'IfExists'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }

        It "Has parameter sets for ByName and ById" {
            $command = Get-Command Remove-DataverseSitemap -ErrorAction SilentlyContinue
            $parameterSets = $command.ParameterSets
            $parameterSets.Name | Should -Contain 'ByName'
            $parameterSets.Name | Should -Contain 'ById'
        }
    }
}

Describe 'Sitemap XML Manipulation' {
    Context 'XML Parsing and Manipulation' {
        It "Can parse sitemap XML" {
            $sitemapXml = @"
<SiteMap>
  <Area Id="Area1" ResourceId="Area1.Title">
    <Group Id="Group1" ResourceId="Group1.Title">
      <SubArea Id="SubArea1" ResourceId="SubArea1.Title" Entity="contact" />
    </Group>
  </Area>
</SiteMap>
"@
            
            # Parse XML
            $xml = [xml]$sitemapXml
            
            # Verify structure
            $xml.SiteMap | Should -Not -BeNullOrEmpty
            $xml.SiteMap.Area | Should -Not -BeNullOrEmpty
            $xml.SiteMap.Area.Id | Should -Be "Area1"
        }

        It "Can add new area to sitemap XML" {
            $sitemapXml = "<SiteMap><Area Id='Area1'/></SiteMap>"
            $xml = [xml]$sitemapXml
            
            # Add new area
            $newArea = $xml.CreateElement("Area")
            $newArea.SetAttribute("Id", "Area2")
            $xml.SiteMap.AppendChild($newArea) | Out-Null
            
            # Verify
            $xml.SiteMap.Area.Count | Should -Be 2
            $xml.SiteMap.Area[1].Id | Should -Be "Area2"
        }

        It "Can convert modified XML back to string" {
            $sitemapXml = "<SiteMap><Area Id='Initial'/></SiteMap>"
            $xml = [xml]$sitemapXml
            
            $newArea = $xml.CreateElement("Area")
            $newArea.SetAttribute("Id", "TestArea")
            $xml.DocumentElement.AppendChild($newArea) | Out-Null
            
            # Get modified XML as string
            $modifiedXml = $xml.OuterXml
            
            # Verify it's valid XML with our changes
            $modifiedXml | Should -Match "<Area"
            $modifiedXml | Should -Match "TestArea"
            
            # Verify we now have 2 areas
            $xml.SiteMap.Area.Count | Should -Be 2
        }
    }
}

Describe 'SitemapInfo Class' {
    Context 'Properties' {
        It "SitemapInfo type exists with expected properties" {
            $type = [Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo]
            $type | Should -Not -BeNullOrEmpty
            
            # Check properties exist
            $properties = $type.GetProperties().Name
            $properties | Should -Contain 'Id'
            $properties | Should -Contain 'Name'
            $properties | Should -Contain 'SitemapXml'
            $properties | Should -Contain 'IsManaged'
            $properties | Should -Contain 'UniqueName'
            $properties | Should -Contain 'CreatedOn'
            $properties | Should -Contain 'ModifiedOn'
        }
    }
}
