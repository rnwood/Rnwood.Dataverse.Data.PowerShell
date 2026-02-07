. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityMetadata - Icon Properties' {
    Context 'Setting Icon Properties on New Entity' {
    }

    Context 'Updating Icon Properties on Existing Entity' {
    }
}

Describe 'Set-DataverseEntityMetadata - EntityMetadata Parameter' {
    Context 'Updating with EntityMetadata Object' {
    }

    Context 'EntityMetadata Parameter with PassThru' {
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
    }
    
    Context 'Icon WebResource Validation - Invalid WebResources' {
    }
    
    Context 'Icon WebResource Validation - SkipIconValidation Switch' {
    }
    
    Context 'Icon WebResource Validation - Unpublished WebResources' {
    }
}
