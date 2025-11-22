. $PSScriptRoot/Common.ps1

# Helper function to create interceptor for Set-DataverseEntityKeyMetadata tests
# This cmdlet needs special handling because it calls GetBaseLanguageCode() which requires
# WhoAmI and organization entity retrieval
function Get-SetEntityKeyInterceptor {
    param([scriptblock]$AdditionalInterceptor = $null)
    
    return {
        param($request)
        
        # Handle RetrieveEntityRequest - return cached entity metadata
        if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
            $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
            $entityMetadata = $global:TestMetadataCache["contact"]
            $response.Results.Add("EntityMetadata", $entityMetadata)
            return $response
        }
        
        # Handle CreateEntityKeyRequest
        if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
            $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
            $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
            
            # Call additional interceptor if provided to capture request
            if ($null -ne $AdditionalInterceptor) {
                $AdditionalInterceptor.Invoke($request)
            }
            
            return $response
        }
        
        # Handle WhoAmI request (string-based, used by GetBaseLanguageCode)
        if ($request.GetType().FullName -eq 'Microsoft.Xrm.Sdk.OrganizationRequest' -and $request.RequestName -eq 'WhoAmI') {
            $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            $response.Results.Add("UserId", [Guid]::NewGuid())
            $response.Results.Add("BusinessUnitId", [Guid]::NewGuid())
            $response.Results.Add("OrganizationId", [Guid]::Parse("00000000-0000-0000-0000-000000000001"))
            return $response
        }
        
        # Handle RetrieveRequest for organization entity (used by GetBaseLanguageCode)
        if ($request.GetType().Name -eq 'RetrieveRequest') {
            $retrieveReq = $request -as [Microsoft.Xrm.Sdk.Messages.RetrieveRequest]
            if ($null -ne $retrieveReq -and $null -ne $retrieveReq.Target -and $retrieveReq.Target.LogicalName -eq 'organization') {
                $org = New-Object Microsoft.Xrm.Sdk.Entity("organization", $retrieveReq.Target.Id)
                $org.Attributes.Add("languagecode", 1033) # English (US)
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $response.Results.Add("Entity", $org)
                return $response
            }
        }
        
        return $null
    }.GetNewClosure()
}

Describe 'Set-DataverseEntityKeyMetadata' {
    # Note: This cmdlet requires multiple interceptors because it calls GetBaseLanguageCode()
    # which needs WhoAmI and organization entity retrieval support
    
    Context 'Parameter Validation' {
        It "Throws error when KeyAttributes is empty" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor)
            
            # Try with empty KeyAttributes array
            { Set-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -SchemaName "test_key" -KeyAttributes @() -ErrorAction Stop } |
                Should -Throw "*KeyAttributes*"
        }
    }
    
    Context 'Request Creation' {
        It "Creates CreateEntityKeyRequest with correct EntityName" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor -AdditionalInterceptor {
                param($request)
                $script:capturedRequest = $request
            })
            
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
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor -AdditionalInterceptor {
                param($request)
                $script:capturedRequest = $request
            })
            
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
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor -AdditionalInterceptor {
                param($request)
                $script:capturedRequest = $request
            })
            
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
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor -AdditionalInterceptor {
                param($request)
                $script:keyCreated = $true
            })
            
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
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor (Get-SetEntityKeyInterceptor -AdditionalInterceptor {
                param($request)
                $script:createCalled = $true
            })
            
            # Create key with Force (should skip existence check)
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -Force `
                -Confirm:$false
            
            # Verify key was created
            $script:createCalled | Should -Be $true
        }
    }
}
