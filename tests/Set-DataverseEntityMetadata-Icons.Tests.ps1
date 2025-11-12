. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityMetadata - Icon Properties' {
    Context 'Setting Icon Properties on New Entity' {
        It "Creates entity with IconVectorName" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest
            $connection = getMockConnection
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName new_test `
                -SchemaName new_Test `
                -DisplayName "Test Entity" `
                -DisplayCollectionName "Test Entities" `
                -OwnershipType UserOwned `
                -PrimaryAttributeSchemaName new_name `
                -PrimaryAttributeDisplayName "Name" `
                -IconVectorName "custom_icon_vector"
            
            # Verify entity was created with icon
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName new_test
            $result.IconVectorName | Should -Be "custom_icon_vector"
        }

        It "Creates entity with all icon properties" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest
            $connection = getMockConnection
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName new_icontest `
                -SchemaName new_IconTest `
                -DisplayName "Icon Test" `
                -DisplayCollectionName "Icon Tests" `
                -OwnershipType UserOwned `
                -PrimaryAttributeSchemaName new_name `
                -PrimaryAttributeDisplayName "Name" `
                -IconVectorName "vector_icon" `
                -IconLargeName "large_icon.png" `
                -IconMediumName "medium_icon.png" `
                -IconSmallName "small_icon.png"
            
            # Verify all icons were set
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName new_icontest
            $result.IconVectorName | Should -Be "vector_icon"
            $result.IconLargeName | Should -Be "large_icon.png"
            $result.IconMediumName | Should -Be "medium_icon.png"
            $result.IconSmallName | Should -Be "small_icon.png"
        }
    }

    Context 'Updating Icon Properties on Existing Entity' {
        It "Updates IconVectorName on existing entity" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Update the contact entity icon
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "new_contact_icon"
            
            # Verify icon was updated
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $result.IconVectorName | Should -Be "new_contact_icon"
        }

        It "Updates all icon properties on existing entity" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Update all icon properties
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "updated_vector" `
                -IconLargeName "updated_large.png" `
                -IconMediumName "updated_medium.png" `
                -IconSmallName "updated_small.png"
            
            # Verify all icons were updated
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $result.IconVectorName | Should -Be "updated_vector"
            $result.IconLargeName | Should -Be "updated_large.png"
            $result.IconMediumName | Should -Be "updated_medium.png"
            $result.IconSmallName | Should -Be "updated_small.png"
        }

        It "Clears icon property by setting empty string" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Clear the icon
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName ""
            
            # Verify icon was cleared
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $result.IconVectorName | Should -BeNullOrEmpty
        }
    }
}

Describe 'Set-DataverseEntityMetadata - EntityMetadata Parameter' {
    Context 'Updating with EntityMetadata Object' {
        It "Updates entity using EntityMetadata object" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Get existing metadata
            $metadata = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            
            # Modify the metadata
            $metadata.IconVectorName = "modified_icon"
            $metadata.IconLargeName = "modified_large.png"
            
            # Update using the metadata object
            Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $metadata
            
            # Verify changes were applied
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $result.IconVectorName | Should -Be "modified_icon"
            $result.IconLargeName | Should -Be "modified_large.png"
        }

        It "Updates entity from pipeline using EntityMetadata" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Get and modify metadata, then update via pipeline
            Get-DataverseEntityMetadata -Connection $connection -EntityName contact | 
                ForEach-Object {
                    $_.IconVectorName = "pipeline_icon"
                    $_
                } |
                Set-DataverseEntityMetadata -Connection $connection
            
            # Verify changes
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $result.IconVectorName | Should -Be "pipeline_icon"
        }

        It "Throws error when EntityMetadata has no MetadataId" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Create metadata object without MetadataId
            $metadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
            $metadata.LogicalName = "contact"
            
            # Should throw error
            { Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $metadata -ErrorAction Stop } |
                Should -Throw "*MetadataId*"
        }

        It "Throws error when EntityMetadata has no LogicalName" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Create metadata object without LogicalName
            $metadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
            $metadata.MetadataId = [Guid]::NewGuid()
            
            # Should throw error
            { Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $metadata -ErrorAction Stop } |
                Should -Throw "*LogicalName*"
        }
    }

    Context 'EntityMetadata Parameter with PassThru' {
        It "Returns updated metadata when using -PassThru with EntityMetadata" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Get existing metadata
            $metadata = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            $metadata.IconVectorName = "passthru_icon"
            
            # Update with PassThru
            $result = Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $metadata -PassThru
            
            # Verify result is returned
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
        }
    }
}

Describe 'Get-DataverseEntityMetadata - Icon Properties Access' {
    Context 'Icon Properties in Output' {
        It "Returns EntityMetadata with icon properties accessible" {
            $connection = getMockConnection
            
            # Get metadata
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            
            # Verify icon properties exist (they may be null/empty)
            $result.PSObject.Properties.Name | Should -Contain "IconVectorName"
            $result.PSObject.Properties.Name | Should -Contain "IconLargeName"
            $result.PSObject.Properties.Name | Should -Contain "IconMediumName"
            $result.PSObject.Properties.Name | Should -Contain "IconSmallName"
        }

        It "Can access icon properties from retrieved metadata" {
            $connection = getMockConnection
            
            # Get metadata and access icon properties
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            
            # These should not throw errors (values may be null)
            $vectorIcon = $result.IconVectorName
            $largeIcon = $result.IconLargeName
            $mediumIcon = $result.IconMediumName
            $smallIcon = $result.IconSmallName
            
            # Test passes if no errors were thrown
            $true | Should -Be $true
        }
    }
}

Describe 'Set-DataverseEntityMetadata - Icon Validation' {
    Context 'Icon WebResource Validation - Valid WebResources' {
        It "Validates IconVectorName references valid SVG webresource" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # This test validates behavior that works in real Dataverse environments
            # Create request interceptor that returns valid SVG webresource
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return a valid SVG webresource (type 11)
                        $webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
                        $webResource.Id = [Guid]::NewGuid()
                        $webResource["name"] = "new_validsvgicon"
                        $webResource["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(11) # SVG type
                        
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $entityCollection.Entities.Add($webResource)
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Update contact entity with valid SVG icon (should not throw)
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "new_validsvgicon" `
                -ErrorAction Stop } | Should -Not -Throw
        }
        
        It "Allows empty IconVectorName to clear icon" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Empty string should be allowed (clears the icon) without validation
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "" `
                -ErrorAction Stop } | Should -Not -Throw
        }
        
        It "Allows null IconVectorName to skip icon update" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            $connection = getMockConnection
            
            # Not providing IconVectorName should work fine
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -DisplayName "Contact Updated" `
                -ErrorAction Stop } | Should -Not -Throw
        }
    }
    
    Context 'Icon WebResource Validation - Invalid WebResources' {
        It "Throws error when IconVectorName references non-existent webresource" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # Create request interceptor that returns no webresources
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return empty collection (no webresource found)
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Should throw error about non-existent webresource
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "nonexistent_icon" `
                -ErrorAction Stop } | Should -Throw "*does not reference a valid webresource*"
        }
        
        It "Throws error when IconVectorName references webresource with wrong type" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # Create request interceptor that returns webresource with wrong type
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return a PNG webresource (type 5) instead of SVG (type 11)
                        $webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
                        $webResource.Id = [Guid]::NewGuid()
                        $webResource["name"] = "new_pngicon"
                        $webResource["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(5) # PNG type
                        
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $entityCollection.Entities.Add($webResource)
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Should throw error about wrong webresource type
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "new_pngicon" `
                -ErrorAction Stop } | Should -Throw "*type 5*type 11*required*"
        }
    }
    
    Context 'Icon WebResource Validation - SkipIconValidation Switch' {
        It "Skips validation when SkipIconValidation is specified" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # Create request interceptor that would normally cause validation to fail
            # No webresource interceptor - queries will return empty
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return empty collection
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Should NOT throw error because validation is skipped
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "any_icon_name" `
                -SkipIconValidation `
                -ErrorAction Stop } | Should -Not -Throw
        }
        
        It "Skips validation for wrong type when SkipIconValidation is specified" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # Create request interceptor that returns webresource with wrong type
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return PNG instead of SVG
                        $webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
                        $webResource.Id = [Guid]::NewGuid()
                        $webResource["name"] = "wrong_type_icon"
                        $webResource["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(5) # PNG
                        
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $entityCollection.Entities.Add($webResource)
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Should NOT throw error because validation is skipped
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "wrong_type_icon" `
                -SkipIconValidation `
                -ErrorAction Stop } | Should -Not -Throw
        }
    }
    
    Context 'Icon WebResource Validation - Unpublished WebResources' {
        It "Validates IconVectorName against unpublished webresource" -Skip {
            # Skip: FakeXrmEasy doesn't support UpdateEntityRequest fully
            # Create request interceptor that returns unpublished webresource
            $requestInterceptor = {
                param($request)
                
                if ($request.GetType().Name -eq 'RetrieveUnpublishedMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Return unpublished SVG webresource
                        $webResource = New-Object Microsoft.Xrm.Sdk.Entity("webresource")
                        $webResource.Id = [Guid]::NewGuid()
                        $webResource["name"] = "new_unpublishedsvg"
                        $webResource["webresourcetype"] = New-Object Microsoft.Xrm.Sdk.OptionSetValue(11) # SVG
                        
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $entityCollection.Entities.Add($webResource)
                        
                        $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                    $query = $request.Query
                    if ($query.EntityName -eq 'webresource') {
                        # Published query returns empty (only unpublished exists)
                        $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $response.Results.Add("EntityCollection", $entityCollection)
                        return $response
                    }
                }
                
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor
            
            # Should validate successfully against unpublished webresource
            { Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -IconVectorName "new_unpublishedsvg" `
                -ErrorAction Stop } | Should -Not -Throw
        }
    }
}
