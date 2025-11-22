. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseEntityKeyMetadata' {
    # Note: RetrieveEntityRequest is intercepted by Common.ps1's getMockConnection
    # which returns entity metadata from the test cache
    
    Context 'Command Parameter Validation' {
        It "Accepts EntityName parameter" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    # Return metadata without keys to keep test simple
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Should not throw with EntityName
            { Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact } | Should -Not -Throw
        }
        
        It "Accepts KeyName parameter" {
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
            
            # Should not throw with KeyName (will throw later if key not found, but parameters are valid)
            # We test for "KeyNotFound" error, not parameter validation error
            { Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "test_key" -ErrorAction Stop } |
                Should -Throw "*not found*"
        }
    }
    
    Context 'Request Creation' {
        It "Creates RetrieveEntityRequest with correct EntityName" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Execute cmdlet
            try {
                Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact
            } catch {}
            
            # Verify request was created correctly
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.LogicalName | Should -Be "contact"
        }
        
        It "Uses RetrieveAsIfPublished=true by default" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Execute cmdlet without -Published
            try {
                Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact
            } catch {}
            
            # Verify RetrieveAsIfPublished is true (unpublished)
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.RetrieveAsIfPublished | Should -Be $true
        }
        
        It "Uses RetrieveAsIfPublished=false with -Published" {
            $script:capturedRequest = $null
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $script:capturedRequest = $request
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Execute cmdlet with -Published
            try {
                Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -Published
            } catch {}
            
            # Verify RetrieveAsIfPublished is false (published only)
            $script:capturedRequest | Should -Not -BeNullOrEmpty
            $script:capturedRequest.RetrieveAsIfPublished | Should -Be $false
        }
    }
    
    Context 'Error Handling' {
        It "Throws error when entity has no keys and KeyName is specified" {
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
            
            # Try to get non-existent key
            { Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "nonexistent_key" -ErrorAction Stop } |
                Should -Throw "*not found*"
        }
    }
}
