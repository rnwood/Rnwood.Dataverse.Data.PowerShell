. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityMetadata - HasActivities and HasNotes' {
    Context 'Creating Entity with HasActivities' {
        It "Creates entity with HasActivities enabled" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest
            # This test validates that HasActivities can be set during entity creation
            $connection = getMockConnection
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName new_testactivity `
                -SchemaName new_TestActivity `
                -DisplayName "Test Activity Entity" `
                -DisplayCollectionName "Test Activity Entities" `
                -OwnershipType UserOwned `
                -PrimaryAttributeSchemaName new_name `
                -PrimaryAttributeDisplayName "Name" `
                -HasActivities `
                -Confirm:$false
            
            # Verify entity was created with HasActivities enabled
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName new_testactivity
            $result.HasActivities | Should -Be $true
        }

        It "Creates entity with HasNotes enabled" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest
            # This test validates that HasNotes can be set during entity creation
            $connection = getMockConnection
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName new_testnotes `
                -SchemaName new_TestNotes `
                -DisplayName "Test Notes Entity" `
                -DisplayCollectionName "Test Notes Entities" `
                -OwnershipType UserOwned `
                -PrimaryAttributeSchemaName new_name `
                -PrimaryAttributeDisplayName "Name" `
                -HasNotes `
                -Confirm:$false
            
            # Verify entity was created with HasNotes enabled
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName new_testnotes
            $result.HasNotes | Should -Be $true
        }

        It "Creates entity with both HasActivities and HasNotes enabled" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest
            # This test validates that both properties can be set during entity creation
            $connection = getMockConnection
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName new_testboth `
                -SchemaName new_TestBoth `
                -DisplayName "Test Both Entity" `
                -DisplayCollectionName "Test Both Entities" `
                -OwnershipType UserOwned `
                -PrimaryAttributeSchemaName new_name `
                -PrimaryAttributeDisplayName "Name" `
                -HasActivities `
                -HasNotes `
                -Confirm:$false
            
            # Verify entity was created with both properties enabled
            $result = Get-DataverseEntityMetadata -Connection $connection -EntityName new_testboth
            $result.HasActivities | Should -Be $true
            $result.HasNotes | Should -Be $true
        }
    }

    Context 'Updating Existing Entity - Enabling HasActivities' {
        It "Should not throw error when enabling HasActivities on existing entity" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest/CreateOneToManyRequest
            # This test validates that the cmdlet no longer throws "immutable" error
            # and instead attempts to create the relationship with activitypointer
            $connection = getMockConnection -Entities @('account')
            
            # This should not throw an "immutable" error
            # It will attempt to create a relationship with activitypointer
            # In a real environment, this creates the HasActivities relationship
            {
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName account `
                    -HasActivities `
                    -Confirm:$false `
                    -WarningAction SilentlyContinue
            } | Should -Not -Throw -Because "HasActivities is not immutable and can be changed after creation"
        }

        It "Should create relationship when enabling HasActivities" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateOneToManyRequest
            # This test validates the relationship creation logic
            $connection = getMockConnection -Entities @('account')
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName account `
                -HasActivities `
                -Confirm:$false `
                -WarningAction SilentlyContinue
            
            # In a real environment, this would create a relationship named 'Account_ActivityPointers'
            # Verify the relationship exists
            $relationships = Get-DataverseRelationshipMetadata -Connection $connection -EntityName account
            $activityRelationship = $relationships | Where-Object { $_.SchemaName -eq 'Account_ActivityPointers' }
            $activityRelationship | Should -Not -BeNullOrEmpty
            $activityRelationship.ReferencedEntity | Should -Be 'account'
            $activityRelationship.ReferencingEntity | Should -Be 'activitypointer'
        }
    }

    Context 'Updating Existing Entity - Enabling HasNotes' {
        It "Should not throw error when enabling HasNotes on existing entity" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateEntityRequest/CreateOneToManyRequest
            # This test validates that the cmdlet no longer throws "immutable" error
            $connection = getMockConnection -Entities @('account')
            
            # This should not throw an "immutable" error
            {
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName account `
                    -HasNotes `
                    -Confirm:$false `
                    -WarningAction SilentlyContinue
            } | Should -Not -Throw -Because "HasNotes is not immutable and can be changed after creation"
        }

        It "Should create relationship when enabling HasNotes" -Skip {
            # Skip: FakeXrmEasy doesn't support CreateOneToManyRequest
            # This test validates the relationship creation logic
            $connection = getMockConnection -Entities @('account')
            
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName account `
                -HasNotes `
                -Confirm:$false `
                -WarningAction SilentlyContinue
            
            # In a real environment, this would create a relationship named 'Account_Annotations'
            # Verify the relationship exists
            $relationships = Get-DataverseRelationshipMetadata -Connection $connection -EntityName account
            $notesRelationship = $relationships | Where-Object { $_.SchemaName -eq 'Account_Annotations' }
            $notesRelationship | Should -Not -BeNullOrEmpty
            $notesRelationship.ReferencedEntity | Should -Be 'account'
            $notesRelationship.ReferencingEntity | Should -Be 'annotation'
        }
    }

    Context 'Warning Messages for Disabling' {
        It "Should warn when trying to disable HasActivities" -Skip {
            # Skip: FakeXrmEasy doesn't support the full metadata operations
            # This test validates that disabling shows a warning (not an error)
            $connection = getMockConnection -Entities @('account')
            
            # Mock account entity with HasActivities = true
            # Attempt to disable should show a warning, not an error
            $warnings = @()
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName account `
                -HasActivities:$false `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue
            
            # Should receive a warning about manual deletion being required
            $warnings | Should -Not -BeNullOrEmpty
            $warnings -join '' | Should -Match 'Remove-DataverseRelationshipMetadata'
        }

        It "Should warn when trying to disable HasNotes" -Skip {
            # Skip: FakeXrmEasy doesn't support the full metadata operations
            # This test validates that disabling shows a warning (not an error)
            $connection = getMockConnection -Entities @('account')
            
            # Mock account entity with HasNotes = true
            # Attempt to disable should show a warning, not an error
            $warnings = @()
            Set-DataverseEntityMetadata -Connection $connection `
                -EntityName account `
                -HasNotes:$false `
                -Confirm:$false `
                -WarningVariable warnings `
                -WarningAction SilentlyContinue
            
            # Should receive a warning about manual deletion being required
            $warnings | Should -Not -BeNullOrEmpty
            $warnings -join '' | Should -Match 'Remove-DataverseRelationshipMetadata'
        }
    }

    Context 'Parameter Validation' {
        It "HasActivities parameter should exist and be a switch" {
            $connection = getMockConnection
            
            $cmdlet = Get-Command Set-DataverseEntityMetadata
            $cmdlet.Parameters.ContainsKey('HasActivities') | Should -Be $true
            $cmdlet.Parameters['HasActivities'].ParameterType.Name | Should -Be 'SwitchParameter'
        }

        It "HasNotes parameter should exist and be a switch" {
            $connection = getMockConnection
            
            $cmdlet = Get-Command Set-DataverseEntityMetadata
            $cmdlet.Parameters.ContainsKey('HasNotes') | Should -Be $true
            $cmdlet.Parameters['HasNotes'].ParameterType.Name | Should -Be 'SwitchParameter'
        }

        It "HasActivities parameter help message should indicate it supports activities" {
            $connection = getMockConnection
            
            $cmdlet = Get-Command Set-DataverseEntityMetadata
            $param = $cmdlet.Parameters['HasActivities']
            $helpAttr = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -First 1
            $helpAttr.HelpMessage | Should -Match 'activities'
        }

        It "HasNotes parameter help message should indicate it supports notes" {
            $connection = getMockConnection
            
            $cmdlet = Get-Command Set-DataverseEntityMetadata
            $param = $cmdlet.Parameters['HasNotes']
            $helpAttr = $param.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | Select-Object -First 1
            $helpAttr.HelpMessage | Should -Match 'notes'
        }
    }

    Context 'Regression Tests - No Immutability Error' {
        It "Should NOT throw 'immutable after creation' error for HasActivities" {
            # This is the core regression test for the bug fix
            # The old code would throw: "Cannot change HasActivities from 'False' to 'True'. This property is immutable after creation."
            # The new code should not throw this error
            
            $connection = getMockConnection -Entities @('contact')
            
            # Attempt to enable HasActivities - this previously failed with immutability error
            # Now it should attempt the operation (even if FakeXrmEasy can't fully execute it)
            $errorOccurred = $false
            $errorMessage = ""
            try {
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName contact `
                    -HasActivities `
                    -Confirm:$false `
                    -WarningAction SilentlyContinue `
                    -ErrorAction Stop
            }
            catch {
                $errorOccurred = $true
                $errorMessage = $_.Exception.Message
            }
            
            # The error should NOT be about immutability
            if ($errorOccurred) {
                $errorMessage | Should -Not -Match 'immutable after creation' -Because "HasActivities is not immutable and can be changed"
                $errorMessage | Should -Not -Match 'Cannot change HasActivities' -Because "HasActivities is not immutable and can be changed"
            }
        }

        It "Should NOT throw 'immutable after creation' error for HasNotes" {
            # This is the core regression test for the bug fix
            # The old code would throw: "Cannot change HasNotes from 'False' to 'True'. This property is immutable after creation."
            # The new code should not throw this error
            
            $connection = getMockConnection -Entities @('contact')
            
            # Attempt to enable HasNotes - this previously failed with immutability error
            $errorOccurred = $false
            $errorMessage = ""
            try {
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName contact `
                    -HasNotes `
                    -Confirm:$false `
                    -WarningAction SilentlyContinue `
                    -ErrorAction Stop
            }
            catch {
                $errorOccurred = $true
                $errorMessage = $_.Exception.Message
            }
            
            # The error should NOT be about immutability
            if ($errorOccurred) {
                $errorMessage | Should -Not -Match 'immutable after creation' -Because "HasNotes is not immutable and can be changed"
                $errorMessage | Should -Not -Match 'Cannot change HasNotes' -Because "HasNotes is not immutable and can be changed"
            }
        }
    }
}
