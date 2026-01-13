. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityMetadata - HasActivities and HasNotes Updates' {
    Context 'HasActivities Parameter' {
        It "Detects changes when only HasActivities is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support CreateEntityRequest/UpdateEntityRequest
            # This fix ensures HasActivities parameter is recognized as a change and sets hasChanges=true
            # The fix adds handling for HasActivities.IsPresent in the UpdateEntity method
            $connection = getMockConnection
            
            # This test verifies that the cmdlet recognizes HasActivities as a change
            # and does not issue a "No changes specified for update" warning
            
            $warnings = @()
            $result = Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -HasActivities:$true `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue `
                -ErrorAction SilentlyContinue
            
            # Should not have "No changes specified" warning
            $warnings | Should -Not -Contain "No changes specified for update"
        }
        
        It "Detects changes when HasActivities is set to false" -Skip {
            # Skipped: FakeXrmEasy doesn't support CreateEntityRequest/UpdateEntityRequest
            # This fix ensures HasActivities parameter is recognized as a change
            $connection = getMockConnection
            
            $warnings = @()
            $result = Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -HasActivities:$false `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue `
                -ErrorAction SilentlyContinue
            
            # Should not have "No changes specified" warning
            $warnings | Should -Not -Contain "No changes specified for update"
        }
    }
    
    Context 'HasNotes Parameter' {
        It "Detects changes when only HasNotes is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support CreateEntityRequest/UpdateEntityRequest
            # This fix ensures HasNotes parameter is recognized as a change and sets hasChanges=true
            # The fix adds handling for HasNotes.IsPresent in the UpdateEntity method
            $connection = getMockConnection
            
            $warnings = @()
            $result = Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -HasNotes:$true `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue `
                -ErrorAction SilentlyContinue
            
            # Should not have "No changes specified" warning
            $warnings | Should -Not -Contain "No changes specified for update"
        }
        
        It "Detects changes when HasNotes is set to false" -Skip {
            # Skipped: FakeXrmEasy doesn't support CreateEntityRequest/UpdateEntityRequest
            # This fix ensures HasNotes parameter is recognized as a change
            $connection = getMockConnection
            
            $warnings = @()
            $result = Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -HasNotes:$false `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue `
                -ErrorAction SilentlyContinue
            
            # Should not have "No changes specified" warning
            $warnings | Should -Not -Contain "No changes specified for update"
        }
    }
    
    Context 'Combined HasActivities and HasNotes' {
        It "Detects changes when both HasActivities and HasNotes are specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support CreateEntityRequest/UpdateEntityRequest
            # This fix ensures both HasActivities and HasNotes parameters are recognized as changes
            $connection = getMockConnection
            
            $warnings = @()
            $result = Set-DataverseEntityMetadata -Connection $connection `
                -EntityName contact `
                -HasActivities:$true `
                -HasNotes:$true `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue `
                -ErrorAction SilentlyContinue
            
            # Should not have "No changes specified" warning
            $warnings | Should -Not -Contain "No changes specified for update"
        }
    }
}
