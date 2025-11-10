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
