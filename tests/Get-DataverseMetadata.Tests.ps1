. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseEntityMetadata' {
    Context 'Single Entity Retrieval' {
        It "Returns metadata for a specific entity" {
            $connection = getMockConnection
            
            # Get metadata for contact entity
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            
            # Verify result structure
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
            $result.SchemaName | Should -Not -BeNullOrEmpty
            $result.PrimaryIdAttribute | Should -Not -BeNullOrEmpty
            $result.PrimaryNameAttribute | Should -Not -BeNullOrEmpty
            $result.IsCustomEntity | Should -Not -BeNullOrEmpty
        }

        It "Returns metadata with attributes when -IncludeAttributes is specified" {
            $connection = getMockConnection
            
            # Get metadata with attributes
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact -IncludeAttributes
            
            # Verify attributes are included
            $result | Should -Not -BeNullOrEmpty
            $result.Attributes | Should -Not -BeNullOrEmpty
            $result.Attributes.Count | Should -BeGreaterThan 0
            
            # Verify attribute structure
            $firstAttr = $result.Attributes[0]
            $firstAttr.LogicalName | Should -Not -BeNullOrEmpty
            $firstAttr.AttributeType | Should -Not -BeNullOrEmpty
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $result = Get-DataverseEntityMetadata -EntityName contact
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
        }
    }

    Context 'All Entities Retrieval' {
        It "Returns all entities when no EntityName is specified" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            $connection = getMockConnection
            
            # Get all entities
            $results = Get-DataverseEntityMetadata -Connection $connection
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -BeGreaterThan 0
            
            # Verify contact is in the list
            $contact = $results | Where-Object { $_.LogicalName -eq 'contact' }
            $contact | Should -Not -BeNullOrEmpty
        }

        It "Returns all entities with attributes when -IncludeAttributes is specified" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            $connection = getMockConnection
            
            # Get all entities with attributes
            $results = Get-DataverseEntityMetadata -Connection $connection -IncludeAttributes
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -BeGreaterThan 0
            
            # Verify at least one entity has attributes
            $withAttributes = $results | Where-Object { $_.Attributes -ne $null }
            $withAttributes | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Published Metadata Retrieval' {
        It "Retrieves unpublished metadata by default" {
            $connection = getMockConnection
            
            # Get metadata without -Published flag (should include unpublished)
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact
            
            # Verify result exists (default behavior should work)
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
        }

        It "Retrieves published metadata with -Published flag" {
            $connection = getMockConnection
            
            # Get only published metadata
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact -Published
            
            # Verify result exists
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
        }

        It "Supports -Published with -IncludeAttributes" {
            $connection = getMockConnection
            
            # Get published metadata with attributes
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName contact -Published -IncludeAttributes
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "contact"
            $result.Attributes | Should -Not -BeNullOrEmpty
        }
    }
}

Describe 'Get-DataverseEntityMetadata' {
    Context 'Entity List Retrieval' {
        It "Returns list of all entities" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            $connection = getMockConnection
            
            # Get all entities
            $results = Get-DataverseEntityMetadata -Connection $connection
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -BeGreaterThan 0
            
            # Verify basic properties
            $firstEntity = $results[0]
            $firstEntity.LogicalName | Should -Not -BeNullOrEmpty
            $firstEntity.SchemaName | Should -Not -BeNullOrEmpty
        }

        It "Returns detailed information with -IncludeAttributes" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            $connection = getMockConnection
            
            # Get entities with details
            $results = Get-DataverseEntityMetadata -Connection $connection -IncludeAttributes
            
            # Verify details are included
            $results | Should -Not -BeNullOrEmpty
            $contact = $results | Where-Object { $_.LogicalName -eq 'contact' }
            $contact | Should -Not -BeNullOrEmpty
            $contact.DisplayName | Should -Not -BeNullOrEmpty
            $contact.PrimaryIdAttribute | Should -Not -BeNullOrEmpty
        }

        It "Filters to custom entities" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            # To filter custom entities, use: $results | Where-Object { $_.IsCustomEntity }
            $connection = getMockConnection
            
            # Get only custom entities
            $results = Get-DataverseEntityMetadata -Connection $connection
            
            # Verify all results are custom
            if ($results) {
                $customEntities = $results | Where-Object { $_.IsCustomEntity }
                $customEntities | ForEach-Object {
                    $_.IsCustomEntity | Should -Be $true
                }
            }
        }

        It "Works with default connection" -Skip {
            # Skip: FakeXrmEasy doesn't support RetrieveAllEntitiesRequest
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $results = Get-DataverseEntityMetadata
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
        }
    }
}

Describe 'Get-DataverseAttributeMetadata' {
    Context 'Single Attribute Retrieval' {
        It "Returns metadata for a specific attribute" {
            $connection = getMockConnection
            
            # Get attribute metadata
            $result = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact -AttributeName firstname
            
            # Verify result structure
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "firstname"
            $result.SchemaName | Should -Not -BeNullOrEmpty
            $result.AttributeType | Should -Not -BeNullOrEmpty
            $result.EntityLogicalName | Should -Be "contact"
        }

        It "Returns type-specific properties for string attribute" {
            $connection = getMockConnection
            
            # Get string attribute
            $result = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact -AttributeName firstname
            
            # Verify string-specific properties
            $result.MaxLength | Should -Not -BeNullOrEmpty
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $result = Get-DataverseAttributeMetadata -EntityName contact -AttributeName firstname
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "firstname"
        }
    }

    Context 'All Attributes Retrieval' {
        It "Returns all attributes when AttributeName is not specified" {
            $connection = getMockConnection
            
            # Get all attributes for entity
            $results = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -BeGreaterThan 0
            
            # Verify firstname is in the list
            $firstname = $results | Where-Object { $_.LogicalName -eq 'firstname' }
            $firstname | Should -Not -BeNullOrEmpty
        }

        It "Returns attributes sorted by LogicalName" {
            $connection = getMockConnection
            
            # Get all attributes
            $results = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact
            
            # Verify sorting
            $results | Should -Not -BeNullOrEmpty
            $logicalNames = @($results | ForEach-Object { $_.LogicalName })
            $sortedNames = @($logicalNames | Sort-Object)
            
            # Verify arrays have the same length
            $logicalNames.Count | Should -Be $sortedNames.Count
            
            # Compare arrays - should already be sorted
            $isSorted = $true
            for ($i = 0; $i -lt $logicalNames.Count - 1; $i++) {
                if ($logicalNames[$i] -gt $logicalNames[$i + 1]) {
                    $isSorted = $false
                    break
                }
            }
            $isSorted | Should -Be $true
        }
    }

    Context 'Published Metadata Retrieval' {
        It "Retrieves unpublished metadata by default" {
            $connection = getMockConnection
            
            # Get metadata without -Published flag
            $result = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact -AttributeName firstname
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "firstname"
        }

        It "Retrieves published metadata with -Published flag" {
            $connection = getMockConnection
            
            # Get only published metadata
            $result = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact -AttributeName firstname -Published
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.LogicalName | Should -Be "firstname"
        }

        It "Retrieves all attributes with -Published flag" {
            $connection = getMockConnection
            
            # Get all published attributes
            $results = Get-DataverseAttributeMetadata -Connection $connection -EntityName contact -Published
            
            # Verify results
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -BeGreaterThan 0
        }
    }
}

Describe 'Get-DataverseOptionSetMetadata' {
    Context 'Entity Attribute Option Set' {
        It "Returns option set for a choice attribute" {
            $connection = getMockConnection
            
            # Get option set for a choice field
            # Note: Using gendercode as it's a standard choice field on contact
            $result = Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName gendercode
            
            # Verify result structure
            $result | Should -Not -BeNullOrEmpty
            $result.Options | Should -Not -BeNullOrEmpty
            $result.Options.Count | Should -BeGreaterThan 0
            
            # Verify option structure
            $firstOption = $result.Options[0]
            $firstOption.Value | Should -Not -BeNullOrEmpty
            $firstOption.Label | Should -Not -BeNullOrEmpty
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $result = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName gendercode
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.Options | Should -Not -BeNullOrEmpty
        }

        It "Throws error for non-choice attribute" {
            $connection = getMockConnection
            
            # Try to get option set for a string field
            { Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName firstname -ErrorAction Stop } |
                Should -Throw
        }
    }

    Context 'Published Metadata Retrieval' {
        It "Retrieves unpublished metadata by default" {
            $connection = getMockConnection
            
            # Get option set without -Published flag
            $result = Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName gendercode
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.Options | Should -Not -BeNullOrEmpty
        }

        It "Retrieves published metadata with -Published flag" {
            $connection = getMockConnection
            
            # Get only published option set
            $result = Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName gendercode -Published
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.Options | Should -Not -BeNullOrEmpty
        }
    }
}
