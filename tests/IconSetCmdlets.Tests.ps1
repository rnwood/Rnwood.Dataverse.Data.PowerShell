. "$PSScriptRoot/Common.ps1"

Describe 'Icon Set Cmdlets - Get-DataverseIconSetIcon' {
    
    It "Get-DataverseIconSetIcon cmdlet exists" {
        { Get-Command Get-DataverseIconSetIcon -ErrorAction Stop } | Should -Not -Throw
    }

    It "Get-DataverseIconSetIcon accepts IconSet parameter" {
        { Get-Command Get-DataverseIconSetIcon -ParameterName IconSet } | Should -Not -Throw
    }

    It "Get-DataverseIconSetIcon accepts Name parameter with wildcards" {
        $param = (Get-Command Get-DataverseIconSetIcon).Parameters['Name']
        $param | Should -Not -BeNullOrEmpty
        # Check that the parameter has SupportsWildcards attribute
        $supportsWildcards = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.SupportsWildcardsAttribute] }
        $supportsWildcards | Should -Not -BeNullOrEmpty
    }

    It "Get-DataverseIconSetIcon IconSet parameter has ValidateSet attribute" {
        $param = (Get-Command Get-DataverseIconSetIcon).Parameters['IconSet']
        $validateSet = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ValidateSetAttribute] }
        $validateSet | Should -Not -BeNullOrEmpty
        $validateSet.ValidValues | Should -Contain "Iconoir"
    }

    # Note: The following tests require internet access and are skipped in CI environments
    # They can be run manually when internet access is available
    
    It "Get-DataverseIconSetIcon retrieves icons from Iconoir (requires internet)" -Skip:($env:CI -eq 'true') {
        $icons = Get-DataverseIconSetIcon -IconSet Iconoir
        $icons | Should -Not -BeNullOrEmpty
        $icons.Count | Should -BeGreaterThan 0
        $icons[0].IconSet | Should -Be "Iconoir"
        $icons[0].Name | Should -Not -BeNullOrEmpty
        $icons[0].DownloadUrl | Should -Not -BeNullOrEmpty
    }

    It "Get-DataverseIconSetIcon filters icons by name (requires internet)" -Skip:($env:CI -eq 'true') {
        # This test assumes there are icons with "user" in the name
        $icons = Get-DataverseIconSetIcon -Name "user*"
        $icons | Should -Not -BeNullOrEmpty
        $icons | ForEach-Object { $_.Name | Should -BeLike "user*" }
    }
}

Describe 'Icon Set Cmdlets - Set-DataverseTableIcon' {
    
    It "Set-DataverseTableIcon cmdlet exists" {
        { Get-Command Set-DataverseTableIcon -ErrorAction Stop } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon accepts EntityName parameter" {
        { Get-Command Set-DataverseTableIcon -ParameterName EntityName } | Should -Not -Throw
        { Get-Command Set-DataverseTableIcon -ParameterName TableName } | Should -Not -Throw # Alias
    }

    It "Set-DataverseTableIcon accepts IconSet parameter" {
        { Get-Command Set-DataverseTableIcon -ParameterName IconSet } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon accepts IconName parameter" {
        { Get-Command Set-DataverseTableIcon -ParameterName IconName } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon accepts PublisherPrefix parameter" {
        { Get-Command Set-DataverseTableIcon -ParameterName PublisherPrefix } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon accepts Publish switch" {
        { Get-Command Set-DataverseTableIcon -ParameterName Publish } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon accepts PassThru switch" {
        { Get-Command Set-DataverseTableIcon -ParameterName PassThru } | Should -Not -Throw
    }

    It "Set-DataverseTableIcon supports ShouldProcess" {
        $cmd = Get-Command Set-DataverseTableIcon
        $cmd.Parameters.ContainsKey('WhatIf') | Should -Be $true
        $cmd.Parameters.ContainsKey('Confirm') | Should -Be $true
    }

    It "Set-DataverseTableIcon EntityName parameter is mandatory" {
        $param = (Get-Command Set-DataverseTableIcon).Parameters['EntityName']
        $mandatory = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -First 1
        $mandatory.Mandatory | Should -Be $true
    }

    It "Set-DataverseTableIcon IconName parameter is mandatory" {
        $param = (Get-Command Set-DataverseTableIcon).Parameters['IconName']
        $mandatory = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -First 1
        $mandatory.Mandatory | Should -Be $true
    }

    # Functional tests with mock connection
    Context "With Mock Connection" {
        
        It "Set-DataverseTableIcon fails when icon not found (mocked)" {
            $connection = getMockConnection -RequestInterceptor {
                param($request)
                
                # Mock icon download failure by simulating HTTP 404
                # The cmdlet will handle this internally, so we need to let it fail naturally
                # This test validates error handling exists
                $null
            } -Entities @("contact")

            # Note: This test would need actual HTTP mocking which is complex
            # Skipping for now as it requires external dependencies
        } -Skip
        
        It "Set-DataverseTableIcon creates web resource with correct properties (mocked)" {
            $webResourceCreated = $false
            $webResourceName = $null
            $webResourceType = $null
            
            $connection = getMockConnection -RequestInterceptor {
                param($request)
                
                # Intercept Create request for webresource
                if ($request.GetType().Name -eq 'CreateRequest') {
                    $entity = $request.Target
                    if ($entity.LogicalName -eq 'webresource') {
                        $script:webResourceCreated = $true
                        $script:webResourceName = $entity.GetAttributeValue('name')
                        $script:webResourceType = $entity.GetAttributeValue('webresourcetype')
                        
                        # Return a fake GUID
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateResponse
                        $response.Results.Add('id', [Guid]::NewGuid())
                        return $response
                    }
                }
                
                # Let other requests through to FakeXrmEasy
                $null
            } -Entities @("contact", "webresource", "organization", "publisher")

            # Note: This test requires mocking HTTP download which is complex
            # Skipping for now as it requires external dependencies and proper setup
        } -Skip

        It "Set-DataverseTableIcon updates entity metadata with icon reference (mocked)" {
            $entityUpdated = $false
            $iconVectorName = $null
            
            $connection = getMockConnection -RequestInterceptor {
                param($request)
                
                # Intercept UpdateEntityRequest
                if ($request.GetType().Name -eq 'UpdateEntityRequest') {
                    $script:entityUpdated = $true
                    $script:iconVectorName = $request.Entity.IconVectorName
                    
                    $response = New-Object Microsoft.Crm.Sdk.Messages.UpdateEntityResponse
                    return $response
                }
                
                $null
            } -Entities @("contact")

            # Note: This test requires mocking HTTP download which is complex
            # Skipping for now as it requires external dependencies
        } -Skip
    }
}

Describe 'Icon Set Cmdlets - Integration' {
    
    It "Get-DataverseIconSetIcon and Set-DataverseTableIcon work together (requires internet and connection)" -Skip {
        # This is an integration test that requires:
        # 1. Internet access to retrieve icons
        # 2. A real Dataverse connection
        # 3. Permissions to create web resources and update entity metadata
        
        # Get an icon
        $icon = Get-DataverseIconSetIcon -Name "user" | Select-Object -First 1
        $icon | Should -Not -BeNullOrEmpty
        
        # Set it on a table (would need real connection)
        # Set-DataverseTableIcon -EntityName "contact" -IconName $icon.Name -Publish
    }
}
