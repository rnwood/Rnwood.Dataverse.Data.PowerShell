. $PSScriptRoot/Common.ps1

Describe 'Remove-DataverseEntityKeyMetadata' {
    # Note: DeleteEntityKeyRequest is intercepted by Common.ps1's getMockConnection
    # to avoid FakeXrmEasy limitations
    
    Context 'Delete Key' {
        It "Deletes an alternate key" {
            $script:keyDeleted = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                # Intercept DeleteEntityKeyRequest
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    $script:keyDeleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Delete the key
            Remove-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -KeyName "contact_emailaddress1_key" -Confirm:$false
            
            # Verify key was deleted
            $script:keyDeleted | Should -Be $true
        }

        It "Passes correct entity name" {
            $script:correctEntityName = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    if ($request.EntityLogicalName -eq "contact") {
                        $script:correctEntityName = $true
                    }
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Delete the key
            Remove-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -KeyName "contact_emailaddress1_key" -Confirm:$false
            
            # Verify entity name was correct
            $script:correctEntityName | Should -Be $true
        }

        It "Passes correct key name" {
            $script:correctKeyName = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    if ($request.Name -eq "contact_emailaddress1_key") {
                        $script:correctKeyName = $true
                    }
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Delete the key
            Remove-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -KeyName "contact_emailaddress1_key" -Confirm:$false
            
            # Verify key name was correct
            $script:correctKeyName | Should -Be $true
        }

        It "Works with default connection" {
            $script:keyDeleted = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    $script:keyDeleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Delete without explicit connection
            Remove-DataverseEntityKeyMetadata -EntityName contact -KeyName "contact_emailaddress1_key" -Confirm:$false
            
            # Verify key was deleted
            $script:keyDeleted | Should -Be $true
        }
    }

    Context 'WhatIf Support' {
        It "Supports -WhatIf" {
            $script:keyDeleted = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    $script:keyDeleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Delete with WhatIf
            Remove-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -KeyName "contact_emailaddress1_key" `
                -WhatIf
            
            # Verify key was NOT deleted
            $script:keyDeleted | Should -Be $false
        }

        It "Supports -Confirm:$false" {
            $script:keyDeleted = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    $script:keyDeleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Delete with Confirm:$false
            Remove-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -KeyName "contact_emailaddress1_key" `
                -Confirm:$false
            
            # Verify key was deleted
            $script:keyDeleted | Should -Be $true
        }
    }

    Context 'Pipeline Support' {
        It "Accepts EntityName from pipeline" {
            $script:keyDeleted = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'DeleteEntityKeyRequest') {
                    $script:keyDeleted = $true
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyResponse
                    return $response
                }
                
                return $null
            }
            
            # Pass entity name via pipeline
            "contact" | Remove-DataverseEntityKeyMetadata -Connection $connection -KeyName "contact_emailaddress1_key" -Confirm:$false
            
            # Verify key was deleted
            $script:keyDeleted | Should -Be $true
        }
    }
}
