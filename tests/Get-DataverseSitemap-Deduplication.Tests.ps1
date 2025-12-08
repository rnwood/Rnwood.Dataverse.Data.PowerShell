. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseSitemap - Deduplication of Published and Unpublished Records' {
    Context 'When both published and unpublished versions exist' {
        BeforeAll {
            # Create a mock connection with custom request interceptor
            # to simulate published and unpublished records
            $script:publishedSitemapId = [guid]::NewGuid()
            $script:unpublishedSitemapId = [guid]::NewGuid()
            $script:onlyPublishedId = [guid]::NewGuid()
            $script:onlyUnpublishedId = [guid]::NewGuid()
            
            $requestInterceptor = {
                param($request)
                
                # Handle RetrieveUnpublishedMultipleRequest - return unpublished records
                if ($request.GetType().Name -eq 'RetrieveUnpublishedMultipleRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Create two unpublished sitemaps:
                    # 1. One that has both published and unpublished versions (should be returned)
                    $unpublishedVersion = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", $script:publishedSitemapId)
                    $unpublishedVersion["sitemapname"] = "TestSitemap-Unpublished"
                    $unpublishedVersion["sitemapnameunique"] = "testsitemap"
                    $unpublishedVersion["sitemapxml"] = "<SiteMap><Area Id='UnpublishedArea' /></SiteMap>"
                    $unpublishedVersion["createdon"] = [DateTime]::Now.AddDays(-10)
                    $unpublishedVersion["modifiedon"] = [DateTime]::Now.AddDays(-1)
                    $entityCollection.Entities.Add($unpublishedVersion)
                    
                    # 2. One that only exists in unpublished state
                    $onlyUnpublished = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", $script:onlyUnpublishedId)
                    $onlyUnpublished["sitemapname"] = "OnlyUnpublished"
                    $onlyUnpublished["sitemapnameunique"] = "onlyunpublished"
                    $onlyUnpublished["sitemapxml"] = "<SiteMap><Area Id='OnlyUnpublishedArea' /></SiteMap>"
                    $onlyUnpublished["createdon"] = [DateTime]::Now.AddDays(-5)
                    $onlyUnpublished["modifiedon"] = [DateTime]::Now
                    $entityCollection.Entities.Add($onlyUnpublished)
                    
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
                
                # Handle RetrieveMultipleRequest - return published records
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Create two published sitemaps:
                    # 1. One that also has an unpublished version (should be filtered out)
                    $publishedVersion = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", $script:publishedSitemapId)
                    $publishedVersion["sitemapname"] = "TestSitemap-Published"
                    $publishedVersion["sitemapnameunique"] = "testsitemap"
                    $publishedVersion["sitemapxml"] = "<SiteMap><Area Id='PublishedArea' /></SiteMap>"
                    $publishedVersion["createdon"] = [DateTime]::Now.AddDays(-10)
                    $publishedVersion["modifiedon"] = [DateTime]::Now.AddDays(-2)
                    $entityCollection.Entities.Add($publishedVersion)
                    
                    # 2. One that only exists in published state
                    $onlyPublished = New-Object Microsoft.Xrm.Sdk.Entity("sitemap", $script:onlyPublishedId)
                    $onlyPublished["sitemapname"] = "OnlyPublished"
                    $onlyPublished["sitemapnameunique"] = "onlypublished"
                    $onlyPublished["sitemapxml"] = "<SiteMap><Area Id='OnlyPublishedArea' /></SiteMap>"
                    $onlyPublished["createdon"] = [DateTime]::Now.AddDays(-20)
                    $onlyPublished["modifiedon"] = [DateTime]::Now.AddDays(-15)
                    $entityCollection.Entities.Add($onlyPublished)
                    
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
                
                return $null
            }
            
            $script:connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('sitemap')
        }

        It "Returns unpublished version when both published and unpublished exist (default behavior)" {
            $sitemaps = Get-DataverseSitemap -Connection $script:connection
            $sitemaps | Should -Not -BeNullOrEmpty
            
            # Should have 3 total sitemaps (not 4):
            # - Unpublished version of TestSitemap (not the published version)
            # - OnlyUnpublished
            # - OnlyPublished
            $sitemaps.Count | Should -Be 3
            
            # Check that we have the unpublished version of TestSitemap
            $testSitemap = $sitemaps | Where-Object { $_.UniqueName -eq "testsitemap" }
            $testSitemap | Should -Not -BeNullOrEmpty
            $testSitemap.Name | Should -Be "TestSitemap-Unpublished"
            $testSitemap.SitemapXml | Should -Be "<SiteMap><Area Id='UnpublishedArea' /></SiteMap>"
            
            # Check that we have OnlyUnpublished
            $onlyUnpub = $sitemaps | Where-Object { $_.UniqueName -eq "onlyunpublished" }
            $onlyUnpub | Should -Not -BeNullOrEmpty
            $onlyUnpub.Name | Should -Be "OnlyUnpublished"
            
            # Check that we have OnlyPublished
            $onlyPub = $sitemaps | Where-Object { $_.UniqueName -eq "onlypublished" }
            $onlyPub | Should -Not -BeNullOrEmpty
            $onlyPub.Name | Should -Be "OnlyPublished"
        }
        
        It "Returns only published records when -Published flag is used" {
            $sitemaps = Get-DataverseSitemap -Connection $script:connection -Published
            $sitemaps | Should -Not -BeNullOrEmpty
            
            # Should have 2 sitemaps (only published):
            # - Published version of TestSitemap
            # - OnlyPublished
            $sitemaps.Count | Should -Be 2
            
            # Check that we have the published version of TestSitemap
            $testSitemap = $sitemaps | Where-Object { $_.UniqueName -eq "testsitemap" }
            $testSitemap | Should -Not -BeNullOrEmpty
            $testSitemap.Name | Should -Be "TestSitemap-Published"
            $testSitemap.SitemapXml | Should -Be "<SiteMap><Area Id='PublishedArea' /></SiteMap>"
            
            # Check that we have OnlyPublished
            $onlyPub = $sitemaps | Where-Object { $_.UniqueName -eq "onlypublished" }
            $onlyPub | Should -Not -BeNullOrEmpty
            $onlyPub.Name | Should -Be "OnlyPublished"
            
            # Should NOT have OnlyUnpublished when -Published is used
            $onlyUnpub = $sitemaps | Where-Object { $_.UniqueName -eq "onlyunpublished" }
            $onlyUnpub | Should -BeNullOrEmpty
        }
        
        It "Deduplicates by ID, not by unique name" {
            $sitemaps = Get-DataverseSitemap -Connection $script:connection
            
            # All IDs should be unique
            $ids = $sitemaps | ForEach-Object { $_.Id }
            $uniqueIds = $ids | Select-Object -Unique
            $ids.Count | Should -Be $uniqueIds.Count
            
            # Should have exactly 3 unique IDs
            $uniqueIds.Count | Should -Be 3
        }
    }
}

Describe 'Get-DataverseAppModule - Deduplication of Published and Unpublished Records' {
    Context 'When both published and unpublished versions exist' {
        BeforeAll {
            # Create a mock connection with custom request interceptor
            $script:publishedAppModuleId = [guid]::NewGuid()
            $script:onlyPublishedAppId = [guid]::NewGuid()
            $script:onlyUnpublishedAppId = [guid]::NewGuid()
            
            $requestInterceptor = {
                param($request)
                
                # Handle RetrieveUnpublishedMultipleRequest - return unpublished records
                if ($request.GetType().Name -eq 'RetrieveUnpublishedMultipleRequest') {
                    $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Unpublished version of app that also has published version
                    $unpublishedVersion = New-Object Microsoft.Xrm.Sdk.Entity("appmodule", $script:publishedAppModuleId)
                    $unpublishedVersion["appmoduleid"] = $script:publishedAppModuleId
                    $unpublishedVersion["uniquename"] = "testapp"
                    $unpublishedVersion["name"] = "Test App (Unpublished)"
                    $unpublishedVersion["description"] = "Unpublished description"
                    $entityCollection.Entities.Add($unpublishedVersion)
                    
                    # App that only exists in unpublished state
                    $onlyUnpublished = New-Object Microsoft.Xrm.Sdk.Entity("appmodule", $script:onlyUnpublishedAppId)
                    $onlyUnpublished["appmoduleid"] = $script:onlyUnpublishedAppId
                    $onlyUnpublished["uniquename"] = "unpublishedapp"
                    $onlyUnpublished["name"] = "Unpublished App"
                    $onlyUnpublished["description"] = "Only unpublished"
                    $entityCollection.Entities.Add($onlyUnpublished)
                    
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
                
                # Handle RetrieveMultipleRequest - return published records
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    
                    # Published version of app that also has unpublished version
                    $publishedVersion = New-Object Microsoft.Xrm.Sdk.Entity("appmodule", $script:publishedAppModuleId)
                    $publishedVersion["appmoduleid"] = $script:publishedAppModuleId
                    $publishedVersion["uniquename"] = "testapp"
                    $publishedVersion["name"] = "Test App (Published)"
                    $publishedVersion["description"] = "Published description"
                    $entityCollection.Entities.Add($publishedVersion)
                    
                    # App that only exists in published state
                    $onlyPublished = New-Object Microsoft.Xrm.Sdk.Entity("appmodule", $script:onlyPublishedAppId)
                    $onlyPublished["appmoduleid"] = $script:onlyPublishedAppId
                    $onlyPublished["uniquename"] = "publishedapp"
                    $onlyPublished["name"] = "Published App"
                    $onlyPublished["description"] = "Only published"
                    $entityCollection.Entities.Add($onlyPublished)
                    
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
                
                return $null
            }
            
            $script:connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('appmodule')
        }

        It "Returns unpublished version when both published and unpublished exist (default behavior)" {
            $appModules = Get-DataverseAppModule -Connection $script:connection
            $appModules | Should -Not -BeNullOrEmpty
            
            # Should have 3 total app modules (not 4)
            $appModules.Count | Should -Be 3
            
            # Check that we have the unpublished version of testapp
            $testApp = $appModules | Where-Object { $_.UniqueName -eq "testapp" }
            $testApp | Should -Not -BeNullOrEmpty
            $testApp.Name | Should -Be "Test App (Unpublished)"
            
            # Check that we have unpublished app
            $unpubApp = $appModules | Where-Object { $_.UniqueName -eq "unpublishedapp" }
            $unpubApp | Should -Not -BeNullOrEmpty
            
            # Check that we have published app
            $pubApp = $appModules | Where-Object { $_.UniqueName -eq "publishedapp" }
            $pubApp | Should -Not -BeNullOrEmpty
        }
        
        It "Returns only published records when -Published flag is used" {
            $appModules = Get-DataverseAppModule -Connection $script:connection -Published
            $appModules | Should -Not -BeNullOrEmpty
            
            # Should have 2 app modules (only published)
            $appModules.Count | Should -Be 2
            
            # Check that we have the published version of testapp
            $testApp = $appModules | Where-Object { $_.UniqueName -eq "testapp" }
            $testApp | Should -Not -BeNullOrEmpty
            $testApp.Name | Should -Be "Test App (Published)"
            
            # Should NOT have unpublished app
            $unpubApp = $appModules | Where-Object { $_.UniqueName -eq "unpublishedapp" }
            $unpubApp | Should -BeNullOrEmpty
        }
        
        It "Deduplicates by ID" {
            $appModules = Get-DataverseAppModule -Connection $script:connection
            
            # All IDs should be unique
            $ids = $appModules | ForEach-Object { $_.Id }
            $uniqueIds = $ids | Select-Object -Unique
            $ids.Count | Should -Be $uniqueIds.Count
            
            # Should have exactly 3 unique IDs
            $uniqueIds.Count | Should -Be 3
        }
    }
}
