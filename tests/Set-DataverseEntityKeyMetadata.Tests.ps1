. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityKeyMetadata' {
    Context 'Create Key' {
        It "Creates a new alternate key" {
            $keyCreated = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                # Intercept RetrieveEntityRequest - return entity without keys
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $entityMetadata.Keys = @()
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                # Intercept CreateEntityKeyRequest
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:keyCreated = $true
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
                -KeyAttributes @("emailaddress1")
            
            # Verify key was created
            $keyCreated | Should -Be $true
        }

        It "Creates a key with display name" {
            $displayNameSet = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $entityMetadata.Keys = @()
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    if ($request.EntityKey.DisplayName.UserLocalizedLabel.Label -eq "Email Address Key") {
                        $script:displayNameSet = $true
                    }
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create a key with display name
            Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -DisplayName "Email Address Key" `
                -KeyAttributes @("emailaddress1")
            
            # Verify display name was set
            $displayNameSet | Should -Be $true
        }

        It "Creates a key with multiple attributes" {
            $multipleAttributes = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $entityMetadata.Keys = @()
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    if ($request.EntityKey.KeyAttributes.Count -eq 2 -and 
                        $request.EntityKey.KeyAttributes[0] -eq "firstname" -and
                        $request.EntityKey.KeyAttributes[1] -eq "lastname") {
                        $script:multipleAttributes = $true
                    }
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
                -KeyAttributes @("firstname", "lastname")
            
            # Verify multiple attributes were set
            $multipleAttributes | Should -Be $true
        }

        It "Returns key metadata with -PassThru" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
                    # If this is after creation, include the key
                    if ($request.LogicalName -eq "contact") {
                        $key = New-Object Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
                        $key.LogicalName = "contact_emailaddress1_key"
                        $key.SchemaName = "contact_emailaddress1_key"
                        $key.DisplayName = New-Object Microsoft.Xrm.Sdk.Label((New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Email Address Key", 1033)), @())
                        $key.MetadataId = [Guid]::NewGuid()
                        # EntityLogicalName is read-only
                        $key.KeyAttributes = @("emailaddress1")
                        $entityMetadata.Keys = @($key)
                    }
                    else {
                        $entityMetadata.Keys = @()
                    }
                    
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.CreateEntityKeyResponse
                    $response.Results.Add("EntityKeyId", [Guid]::NewGuid())
                    return $response
                }
                
                return $null
            }
            
            # Create key with PassThru
            $result = Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -PassThru
            
            # Verify result is returned
            $result | Should -Not -BeNullOrEmpty
            $result.SchemaName | Should -Be "contact_emailaddress1_key"
            $result.KeyAttributes | Should -Not -BeNullOrEmpty
        }

        It "Throws error when KeyAttributes is missing" {
            $connection = getMockConnection -Entities @("contact")
            
            # Try to create key without KeyAttributes
            { Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_key" `
                -ErrorAction Stop } | Should -Throw
        }

        It "Throws error when key already exists" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
                    # Return existing key
                    $key = New-Object Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
                    $key.LogicalName = "contact_emailaddress1_key"
                    $key.SchemaName = "contact_emailaddress1_key"
                    $key.DisplayName = New-Object Microsoft.Xrm.Sdk.Label((New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Email Address Key", 1033)), @())
                    $key.MetadataId = [Guid]::NewGuid()
                    # EntityLogicalName is read-only
                    $key.KeyAttributes = @("emailaddress1")
                    $entityMetadata.Keys = @($key)
                    
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Try to create duplicate key
            { Set-DataverseEntityKeyMetadata -Connection $connection `
                -EntityName contact `
                -SchemaName "contact_emailaddress1_key" `
                -KeyAttributes @("emailaddress1") `
                -ErrorAction Stop } | Should -Throw
        }
    }

    Context 'WhatIf Support' {
        It "Supports -WhatIf" {
            $keyCreated = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $entityMetadata.Keys = @()
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
            $keyCreated | Should -Be $false
        }
    }

    Context 'Force Parameter' {
        It "Skips existence check with -Force" {
            $keyCreated = $false
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                # Don't intercept RetrieveEntityRequest to test Force bypassing check
                
                if ($request.GetType().Name -eq 'CreateEntityKeyRequest') {
                    $script:keyCreated = $true
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
            
            # Verify key was created
            $keyCreated | Should -Be $true
        }
    }
}
