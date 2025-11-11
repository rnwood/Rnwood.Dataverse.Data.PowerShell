. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSitemap' {
    Context 'Cmdlet Structure' {
        It "Get-DataverseSitemap cmdlet exists and has expected parameters" {
            $command = Get-Command Get-DataverseSitemap -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Name'
            $command.Parameters.Keys | Should -Contain 'UniqueName'
            $command.Parameters.Keys | Should -Contain 'Id'
            $command.Parameters.Keys | Should -Contain 'Published'
            $command.Parameters.Keys | Should -Contain 'Connection'
        }
    }

    Context 'Sitemap Retrieval' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Add mock sitemaps to FakeXrmEasy
            $sitemap1 = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemap1["sitemapname"] = "TestSitemap1"
            $sitemap1["sitemapnameunique"] = "testsitemap1"
            $sitemap1["sitemapxml"] = "<SiteMap></SiteMap>"
            $sitemap1["createdon"] = [DateTime]::Now.AddDays(-30)
            $sitemap1["modifiedon"] = [DateTime]::Now.AddDays(-5)
            $sitemap1Id = $connection.Create($sitemap1)
            
            $sitemap2 = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemap2["sitemapname"] = "TestSitemap2"
            $sitemap2["sitemapnameunique"] = "testsitemap2"
            $sitemap2["sitemapxml"] = "<SiteMap><Area Id='TestArea' /></SiteMap>"
            $sitemap2["createdon"] = [DateTime]::Now.AddDays(-60)
            $sitemap2["modifiedon"] = [DateTime]::Now.AddDays(-10)
            $connection.Create($sitemap2) | Out-Null
        }

        It "Can retrieve all sitemaps" {
            $sitemaps = Get-DataverseSitemap -Connection $connection
            $sitemaps | Should -Not -BeNullOrEmpty
            $sitemaps.Count | Should -BeGreaterOrEqual 2
        }

        It "Can retrieve sitemap by name" {
            $sitemap = Get-DataverseSitemap -Connection $connection -Name "TestSitemap1"
            $sitemap | Should -Not -BeNullOrEmpty
            $sitemap.Name | Should -Be "TestSitemap1"
            $sitemap.UniqueName | Should -Be "testsitemap1"
        }

        It "Can retrieve sitemap by unique name" {
            $sitemap = Get-DataverseSitemap -Connection $connection -UniqueName "testsitemap2"
            $sitemap | Should -Not -BeNullOrEmpty
            $sitemap.Name | Should -Be "TestSitemap2"
            $sitemap.UniqueName | Should -Be "testsitemap2"
        }

        It "Can retrieve sitemap by ID" {
            $allSitemaps = Get-DataverseSitemap -Connection $connection
            $testSitemap = $allSitemaps | Where-Object { $_.Name -eq "TestSitemap1" } | Select-Object -First 1
            $testSitemap | Should -Not -BeNullOrEmpty
            
            $sitemap = Get-DataverseSitemap -Connection $connection -Id $testSitemap.Id
            $sitemap | Should -Not -BeNullOrEmpty
            $sitemap.Id | Should -Be $testSitemap.Id
            $sitemap.Name | Should -Be "TestSitemap1"
        }

        It "Returns SitemapInfo objects with correct properties" {
            $sitemap = Get-DataverseSitemap -Connection $connection -Name "TestSitemap1"
            $sitemap | Should -Not -BeNullOrEmpty
            $sitemap.GetType().Name | Should -Be "SitemapInfo"
            
            # Check properties exist
            $sitemap.Id | Should -Not -BeNullOrEmpty
            $sitemap.Name | Should -Be "TestSitemap1"
            $sitemap.UniqueName | Should -Be "testsitemap1"
            $sitemap.SitemapXml | Should -Be "<SiteMap></SiteMap>"
            $sitemap.CreatedOn | Should -Not -BeNullOrEmpty
            $sitemap.ModifiedOn | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Error Handling' {
        It "Returns empty result when sitemap not found by name" {
            $connection = getMockConnection -Entities @('sitemap')
            $sitemap = Get-DataverseSitemap -Connection $connection -Name "NonExistentSitemap"
            $sitemap | Should -BeNullOrEmpty
        }

        It "Returns empty result when sitemap not found by unique name" {
            $connection = getMockConnection -Entities @('sitemap')
            $sitemap = Get-DataverseSitemap -Connection $connection -UniqueName "nonexistentsitemap"
            $sitemap | Should -BeNullOrEmpty
        }

        It "Returns empty result when sitemap not found by ID" {
            $connection = getMockConnection -Entities @('sitemap')
            $sitemap = Get-DataverseSitemap -Connection $connection -Id ([Guid]::NewGuid())
            $sitemap | Should -BeNullOrEmpty
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
            $command.Parameters.Keys | Should -Contain 'UniqueName'
            $command.Parameters.Keys | Should -Contain 'SitemapXml'
            $command.Parameters.Keys | Should -Contain 'PassThru'
            $command.Parameters.Keys | Should -Contain 'Publish'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }
    }

    Context 'Creating New Sitemaps' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
        }

        It "Can create new sitemap with name only" {
            $result = Set-DataverseSitemap -Connection $connection -Name "NewSitemap" -PassThru
            $result | Should -Not -BeNullOrEmpty
            $result | Should -BeOfType [Guid]
            
            # Verify it was created
            $createdSitemap = Get-DataverseSitemap -Connection $connection -Name "NewSitemap"
            $createdSitemap | Should -Not -BeNullOrEmpty
            $createdSitemap.Name | Should -Be "NewSitemap"
        }

        It "Can create new sitemap with name and unique name" {
            $result = Set-DataverseSitemap -Connection $connection -Name "NewSitemap2" -UniqueName "newsitemap2" -PassThru
            $result | Should -Not -BeNullOrEmpty
            $result | Should -BeOfType [Guid]
            
            # Verify it was created
            $createdSitemap = Get-DataverseSitemap -Connection $connection -UniqueName "newsitemap2"
            $createdSitemap | Should -Not -BeNullOrEmpty
            $createdSitemap.Name | Should -Be "NewSitemap2"
            $createdSitemap.UniqueName | Should -Be "newsitemap2"
        }

        It "Can create new sitemap with custom XML" {
            $customXml = @"
<SiteMap>
    <Area Id="TestArea" ResourceId="Area_Test">
        <Group Id="TestGroup" ResourceId="Group_Test">
            <SubArea Id="TestSubArea" Entity="contact" />
        </Group>
    </Area>
</SiteMap>
"@
            $result = Set-DataverseSitemap -Connection $connection -Name "CustomSitemap" -SitemapXml $customXml -PassThru
            $result | Should -Not -BeNullOrEmpty
            
            # Verify it was created with custom XML
            $createdSitemap = Get-DataverseSitemap -Connection $connection -Name "CustomSitemap"
            $createdSitemap | Should -Not -BeNullOrEmpty
            $createdSitemap.SitemapXml | Should -Be $customXml
        }

        It "Uses default XML when SitemapXml not provided" {
            $result = Set-DataverseSitemap -Connection $connection -Name "DefaultXmlSitemap" -PassThru
            $result | Should -Not -BeNullOrEmpty
            
            # Verify it was created with default XML
            $createdSitemap = Get-DataverseSitemap -Connection $connection -Name "DefaultXmlSitemap"
            $createdSitemap | Should -Not -BeNullOrEmpty
            $createdSitemap.SitemapXml | Should -Not -BeNullOrEmpty
            $createdSitemap.SitemapXml | Should -Match "<SiteMap"
        }
    }

    Context 'Updating Existing Sitemaps' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create a test sitemap to update
            $existingSitemap = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $existingSitemap["sitemapname"] = "ExistingSitemap"
            $existingSitemap["sitemapnameunique"] = "existingsitemap"
            $existingSitemap["sitemapxml"] = "<SiteMap></SiteMap>"
            $script:existingSitemapId = $connection.Create($existingSitemap)
        }

        It "Can update existing sitemap by ID" {
            $newXml = "<SiteMap><Area Id='UpdatedArea' /></SiteMap>"
            Set-DataverseSitemap -Connection $connection -Id $script:existingSitemapId -Name "UpdatedName" -SitemapXml $newXml
            
            # Verify update
            $updatedSitemap = Get-DataverseSitemap -Connection $connection -Id $script:existingSitemapId
            $updatedSitemap.Name | Should -Be "UpdatedName"
            $updatedSitemap.SitemapXml | Should -Be $newXml
        }

        It "Can update existing sitemap by unique name" {
            $anotherSitemap = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $anotherSitemap["sitemapname"] = "AnotherSitemap"
            $anotherSitemap["sitemapnameunique"] = "anothersitemap"
            $anotherSitemap["sitemapxml"] = "<SiteMap></SiteMap>"
            $connection.Create($anotherSitemap) | Out-Null
            
            $newXml = "<SiteMap><Area Id='AnotherUpdatedArea' /></SiteMap>"
            Set-DataverseSitemap -Connection $connection -UniqueName "anothersitemap" -Name "AnotherUpdatedName" -SitemapXml $newXml
            
            # Verify update
            $updatedSitemap = Get-DataverseSitemap -Connection $connection -UniqueName "anothersitemap"
            $updatedSitemap.Name | Should -Be "AnotherUpdatedName"
            $updatedSitemap.SitemapXml | Should -Be $newXml
        }

        It "Can update sitemap XML without changing Name" {
            $testSitemap = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $testSitemap["sitemapname"] = "OriginalName"
            $testSitemap["sitemapnameunique"] = "originalname"
            $testSitemap["sitemapxml"] = "<SiteMap></SiteMap>"
            $testSitemapId = $connection.Create($testSitemap)
            
            $newXml = "<SiteMap><Area Id='NewArea' /></SiteMap>"
            Set-DataverseSitemap -Connection $connection -Id $testSitemapId -SitemapXml $newXml
            
            # Verify update - Name should remain unchanged
            $updatedSitemap = Get-DataverseSitemap -Connection $connection -Id $testSitemapId
            $updatedSitemap.Name | Should -Be "OriginalName"
            $updatedSitemap.SitemapXml | Should -Be $newXml
        }
    }

    Context 'Error Handling' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
        }

        It "Throws error for invalid XML" {
            { Set-DataverseSitemap -Connection $connection -Name "InvalidXmlSitemap" -SitemapXml "invalid xml" } | Should -Throw
        }

        It "Throws error when updating non-existent sitemap by ID" {
            { Set-DataverseSitemap -Connection $connection -Id ([Guid]::NewGuid()) -Name "NonExistent" } | Should -Throw
        }

        It "Throws error when Name is not provided for creation" {
            { Set-DataverseSitemap -Connection $connection -SitemapXml "<SiteMap></SiteMap>" } | Should -Throw
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
            $command.Parameters.Keys | Should -Contain 'UniqueName'
            $command.Parameters.Keys | Should -Contain 'IfExists'
            $command.Parameters.Keys | Should -Contain 'Connection'
            $command.Parameters.Keys | Should -Contain 'WhatIf'
            $command.Parameters.Keys | Should -Contain 'Confirm'
        }
    }

    Context 'Removing Sitemaps' {
        BeforeAll {
            $connection = getMockConnection -Entities @('sitemap')
            
            # Create test sitemaps to remove
            $sitemap1 = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemap1["sitemapname"] = "RemoveTestSitemap1"
            $sitemap1["sitemapnameunique"] = "removetestsitemap1"
            $sitemap1["sitemapxml"] = "<SiteMap></SiteMap>"
            $script:removeTestId1 = $connection.Create($sitemap1)
            
            $sitemap2 = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemap2["sitemapname"] = "RemoveTestSitemap2"
            $sitemap2["sitemapnameunique"] = "removetestsitemap2"
            $sitemap2["sitemapxml"] = "<SiteMap></SiteMap>"
            $script:removeTestId2 = $connection.Create($sitemap2)
            
            $sitemap3 = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", [guid]::NewGuid())
            $sitemap3["sitemapname"] = "RemoveTestSitemap3"
            $sitemap3["sitemapnameunique"] = "removetestsitemap3"
            $sitemap3["sitemapxml"] = "<SiteMap></SiteMap>"
            $script:removeTestId3 = $connection.Create($sitemap3)
        }

        It "Can remove sitemap by name" {
            Remove-DataverseSitemap -Connection $connection -Name "RemoveTestSitemap1" -Confirm:$false
            
            # Verify removal
            $removedSitemap = Get-DataverseSitemap -Connection $connection -Name "RemoveTestSitemap1"
            $removedSitemap | Should -BeNullOrEmpty
        }

        It "Can remove sitemap by unique name" {
            Remove-DataverseSitemap -Connection $connection -UniqueName "removetestsitemap2" -Confirm:$false
            
            # Verify removal
            $removedSitemap = Get-DataverseSitemap -Connection $connection -UniqueName "removetestsitemap2"
            $removedSitemap | Should -BeNullOrEmpty
        }

        It "Can remove sitemap by ID" {
            Remove-DataverseSitemap -Connection $connection -Id $script:removeTestId3 -Confirm:$false
            
            # Verify removal
            $removedSitemap = Get-DataverseSitemap -Connection $connection -Id $script:removeTestId3
            $removedSitemap | Should -BeNullOrEmpty
        }

        It "Does not throw error when removing non-existent sitemap with IfExists flag" {
            { Remove-DataverseSitemap -Connection $connection -Name "NonExistentSitemap" -IfExists -Confirm:$false } | Should -Not -Throw
        }

        It "Throws error when removing non-existent sitemap without IfExists flag" {
            { Remove-DataverseSitemap -Connection $connection -Name "NonExistentSitemap" -Confirm:$false } | Should -Throw
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
            $properties | Should -Contain 'UniqueName'
            $properties | Should -Contain 'SitemapXml'
            $properties | Should -Contain 'CreatedOn'
            $properties | Should -Contain 'ModifiedOn'
            
            # Check that IsManaged property does not exist
            $properties | Should -Not -Contain 'IsManaged'
        }

        It "Can create SitemapInfo instance" {
            $sitemap = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
            $sitemap.Id = [Guid]::NewGuid()
            $sitemap.Name = "TestSitemap"
            $sitemap.UniqueName = "testsitemap"
            $sitemap.SitemapXml = "<SiteMap></SiteMap>"
            
            $sitemap.Name | Should -Be "TestSitemap"
            $sitemap.UniqueName | Should -Be "testsitemap"
            $sitemap.SitemapXml | Should -Be "<SiteMap></SiteMap>"
        }
    }
}