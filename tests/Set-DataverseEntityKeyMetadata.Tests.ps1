. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityKeyMetadata' -Skip {
    # Skipping these tests because FakeXrmEasy doesn't support:
    # 1. RetrieveEntityRequest (used to check entity metadata)
    # 2. CreateEntityKeyRequest (used to create alternate keys)
    # These request types are validated by:
    # 1. Successful build (compilation)
    # 2. E2E tests in e2e-tests/EntityKeyMetadata.Tests.ps1
    
    Context 'Parameter Validation' {
        It "Throws error when KeyAttributes is empty" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Try with empty KeyAttributes array
            { Set-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -SchemaName "test_key" -KeyAttributes @() -ErrorAction Stop } |
                Should -Throw "*KeyAttributes*"
        }
    }
    
    Context 'Request Creation' {
        It "Creates CreateEntityKeyRequest with correct EntityName" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create a new key
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -Confirm:$false
            
            # Verify request was created correctly
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.EntityName | Should -Be "contact"
        }
        
        It "Creates EntityKeyMetadata with correct SchemaName" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create a new key
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -Confirm:$false
            
            # Verify EntityKeyMetadata was created correctly
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.EntityKey | Should -Not -BeNullOrEmpty
            $script:capturedRequest.EntityKey.SchemaName | Should -Be "contact_emailaddress1_key"
        }
        
        It "Sets KeyAttributes correctly" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create a key with multiple attributes
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_name_key" `
                -KeyAttributes @("firstname", "lastname") `
                -Confirm:$false
            
            # Verify attributes were set correctly
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.EntityKey.KeyAttributes.Count | Should -Be 2
            $script:capturedRequest.EntityKey.KeyAttributes[0] | Should -Be "firstname"
            $script:capturedRequest.EntityKey.KeyAttributes[1] | Should -Be "lastname"
        }
    }
    
    Context 'WhatIf Support' {
        It "Supports -WhatIf" {
            $script:keyCreated = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:keyCreated = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create key with WhatIf
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -WhatIf
            
            # Verify key was NOT created
            $script:keyCreated | Should -Be $false
        }
    }
    
    Context 'Force Parameter' {
        It "Skips existence check with -Force" {
            $script:retrieveCalled = $false
            $script:createCalled = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $script:retrieveCalled = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:createCalled = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create key with Force (should skip existence check)
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -Force
            
            # Verify retrieve was NOT called (existence check skipped)
            $script:retrieveCalled | Should -Be $false
            # Verify key was created
            $script:createCalled | Should -Be $true
        }
    }
}
