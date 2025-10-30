Describe 'Get-DataverseSitemap' {
    Context 'Cmdlet Structure' {
        It "Get-DataverseSitemap cmdlet exists and has expected parameters" {
            $command = Get-Command Get-DataverseSitemap -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Name'
            $command.Parameters.Keys | Should -Contain 'Id'
            $command.Parameters.Keys | Should -Contain 'SolutionUniqueName'
            $command.Parameters.Keys | Should -Contain 'AppUniqueName'
            $command.Parameters.Keys | Should -Contain 'Managed'
            $command.Parameters.Keys | Should -Contain 'Unmanaged'
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

        It "Validates XML format" {
            $validXml = "<SiteMap><Area Id='Test'/></SiteMap>"
            
            # Valid XML should not throw during validation
            # (it may fail later due to missing metadata, but validation should pass)
            $xmlDoc = $null
            { $xmlDoc = [xml]$validXml } | Should -Not -Throw
            $xmlDoc | Should -Not -BeNullOrEmpty
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
            $sitemapXml = "<SiteMap/>"
            $xml = [xml]$sitemapXml
            
            $newArea = $xml.CreateElement("Area")
            $newArea.SetAttribute("Id", "TestArea")
            $xml.SiteMap.AppendChild($newArea) | Out-Null
            
            # Get modified XML as string
            $modifiedXml = $xml.OuterXml
            
            # Verify it's valid XML with our changes
            $modifiedXml | Should -Match "<Area"
            $modifiedXml | Should -Match "TestArea"
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
            $properties | Should -Contain 'SolutionName'
            $properties | Should -Contain 'IsManaged'
            $properties | Should -Contain 'AppUniqueName'
            $properties | Should -Contain 'CreatedOn'
            $properties | Should -Contain 'ModifiedOn'
        }
    }
}
