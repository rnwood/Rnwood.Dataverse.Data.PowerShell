. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseEntityKeyMetadata' {
    Context 'Single Key Retrieval' {
        It "Returns metadata for a specific key" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                # Intercept RetrieveEntityRequest and add mock key metadata
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    
                    # Get base entity metadata
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
                    # Create mock key
                    $key = New-Object Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
                    $key.LogicalName = "contact_emailaddress1_key"
                    $key.SchemaName = "contact_emailaddress1_key"
                    $key.DisplayName = New-Object Microsoft.Xrm.Sdk.Label((New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Email Address Key", 1033)), @())
                    $key.MetadataId = [Guid]::NewGuid()
                    # Don't set EntityLogicalName - it's read-only and derived from context
                    $key.KeyAttributes = @("emailaddress1")
                    
                    # Add key to entity metadata
                    $entityMetadata.Keys = @($key)
                    
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Get metadata for specific key
            $result = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "contact_emailaddress1_key"
            
            # Verify result structure
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact_emailaddress1_key"
            $result.SchemaName | Should -Not -BeNullOrEmpty
            # Note: EntityLogicalName may not be set in mock scenarios
            $result.KeyAttributes | Should -Not -BeNullOrEmpty
            $result.KeyAttributes.Count | Should -Be 1
            $result.KeyAttributes[0] | Should -Be "emailaddress1"
        }

        It "Returns all keys when KeyName is not specified" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
                    # Create multiple mock keys
                    $key1 = New-Object Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
                    $key1.LogicalName = "contact_emailaddress1_key"
                    $key1.SchemaName = "contact_emailaddress1_key"
                    $key1.DisplayName = New-Object Microsoft.Xrm.Sdk.Label((New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Email Address Key", 1033)), @())
                    $key1.MetadataId = [Guid]::NewGuid()
                    # EntityLogicalName is read-only
                    $key1.KeyAttributes = @("emailaddress1")
                    
                    $key2 = New-Object Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
                    $key2.LogicalName = "contact_firstname_lastname_key"
                    $key2.SchemaName = "contact_firstname_lastname_key"
                    $key2.DisplayName = New-Object Microsoft.Xrm.Sdk.Label((New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Name Key", 1033)), @())
                    $key2.MetadataId = [Guid]::NewGuid()
                    # EntityLogicalName is read-only
                    $key2.KeyAttributes = @("firstname", "lastname")
                    
                    $entityMetadata.Keys = @($key1, $key2)
                    
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Get all keys
            $results = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 2
            
            # Verify keys are sorted by LogicalName
            $results[0].LogicalName | Should -Be "contact_emailaddress1_key"
            $results[1].LogicalName | Should -Be "contact_firstname_lastname_key"
        }

        It "Throws error when key not found" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    $entityMetadata.Keys = @()
                    $response.Results.Add("EntityMetadata", $entityMetadata)
                    return $response
                }
                
                return $null
            }
            
            # Try to get non-existent key
            { Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "nonexistent_key" -ErrorAction Stop } |
                Should -Throw
        }

        It "Works with default connection" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
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
            
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $result = Get-DataverseEntityKeyMetadata -EntityName contact -KeyName "contact_emailaddress1_key"
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact_emailaddress1_key"
        }
    }

    Context 'Published Metadata Retrieval' {
        It "Retrieves unpublished metadata by default" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
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
            
            # Get metadata without -Published flag
            $result = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "contact_emailaddress1_key"
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact_emailaddress1_key"
        }

        It "Retrieves published metadata with -Published flag" {
            $connection = getMockConnection -Entities @("contact") -RequestInterceptor {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveEntityRequest') {
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveEntityResponse
                    $entityMetadata = $global:TestMetadataCache["contact"]
                    
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
            
            # Get only published metadata
            $result = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "contact_emailaddress1_key" -Published
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact_emailaddress1_key"
        }
    }
}
