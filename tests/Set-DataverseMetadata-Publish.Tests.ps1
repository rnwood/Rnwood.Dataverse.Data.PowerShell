. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseEntityMetadata -Publish Parameter' {
    Context 'Cmdlet Structure' {
        It "Has -Publish parameter" {
            $command = Get-Command Set-DataverseEntityMetadata -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Publish'
            
            # Verify it's a switch parameter
            $publishParam = $command.Parameters['Publish']
            $publishParam.ParameterType.Name | Should -Be 'SwitchParameter'
        }
    }

    Context 'WhatIf with -Publish' {
        It "Does not publish when -WhatIf is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support RetrieveEntityRequest
            # This test documents the expected behavior in a real environment:
            # - When -WhatIf is specified, no publish operation should occur
            # - The cmdlet should complete without errors
            $connection = getMockConnection -Entities @('contact')
            
            # Should not throw even with -Publish and -WhatIf
            { 
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName contact `
                    -DisplayName "Updated Contact" `
                    -Publish `
                    -WhatIf `
                    -ErrorAction Stop
            } | Should -Not -Throw
        }
    }
}

Describe 'Set-DataverseAttributeMetadata -Publish Parameter' {
    Context 'Cmdlet Structure' {
        It "Has -Publish parameter" {
            $command = Get-Command Set-DataverseAttributeMetadata -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Publish'
            
            # Verify it's a switch parameter
            $publishParam = $command.Parameters['Publish']
            $publishParam.ParameterType.Name | Should -Be 'SwitchParameter'
        }
    }

    Context 'WhatIf with -Publish' {
        It "Does not publish when -WhatIf is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support RetrieveAttributeRequest
            # This test documents the expected behavior in a real environment:
            # - When -WhatIf is specified, no publish operation should occur
            # - The cmdlet should complete without errors
            $connection = getMockConnection -Entities @('contact')
            
            # Should not throw even with -Publish and -WhatIf
            { 
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName contact `
                    -AttributeName new_testfield `
                    -SchemaName new_TestField `
                    -DisplayName "Test Field" `
                    -AttributeType String `
                    -MaxLength 100 `
                    -Publish `
                    -WhatIf `
                    -ErrorAction Stop
            } | Should -Not -Throw
        }
    }
}

Describe 'Set-DataverseRelationshipMetadata -Publish Parameter' {
    Context 'Cmdlet Structure' {
        It "Has -Publish parameter" {
            $command = Get-Command Set-DataverseRelationshipMetadata -ErrorAction SilentlyContinue
            $command | Should -Not -BeNullOrEmpty
            $command.Parameters.Keys | Should -Contain 'Publish'
            
            # Verify it's a switch parameter
            $publishParam = $command.Parameters['Publish']
            $publishParam.ParameterType.Name | Should -Be 'SwitchParameter'
        }
    }

    Context 'WhatIf with -Publish' {
        It "Does not publish when -WhatIf is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support RetrieveRelationshipRequest
            # This test documents the expected behavior in a real environment:
            # - When -WhatIf is specified, no publish operation should occur
            # - The cmdlet should complete without errors
            $connection = getMockConnection -Entities @('contact', 'account')
            
            # Should not throw even with -Publish and -WhatIf
            { 
                Set-DataverseRelationshipMetadata -Connection $connection `
                    -SchemaName new_account_contact `
                    -RelationshipType OneToMany `
                    -ReferencedEntity account `
                    -ReferencingEntity contact `
                    -LookupAttributeSchemaName new_AccountId `
                    -LookupAttributeDisplayName "Account" `
                    -Publish `
                    -WhatIf `
                    -ErrorAction Stop
            } | Should -Not -Throw
        }
    }
}

Describe 'RetrieveAsIfPublished Behavior' {
    Context 'Entity Metadata Retrieval' {
        It "Should retrieve unpublished entity metadata when it exists" -Skip {
            # Skipped: FakeXrmEasy doesn't support RetrieveAsIfPublished distinction
            # This test documents the expected behavior in a real environment:
            # - When RetrieveAsIfPublished=true, the cmdlet should see unpublished changes
            # - This prevents errors when updating entities that have unpublished changes
            $true | Should -Be $true
        }
    }

    Context 'Attribute Metadata Retrieval' {
        It "Should retrieve unpublished attribute metadata when it exists" -Skip {
            # Skipped: FakeXrmEasy doesn't support RetrieveAsIfPublished distinction
            # This test documents the expected behavior in a real environment:
            # - When RetrieveAsIfPublished=true, the cmdlet should see unpublished changes
            # - This prevents errors when updating attributes that have unpublished changes
            $true | Should -Be $true
        }
    }
}

Describe 'Publishing After Metadata Changes' {
    Context 'Entity Publishing' {
        It "Publishes entity after create/update when -Publish is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support PublishXmlRequest
            # This test documents the expected behavior in a real environment:
            # - After creating/updating entity, PublishXmlRequest is executed
            # - ParameterXml should contain the entity logical name
            # - Publishing happens only when -Publish switch is present
            $true | Should -Be $true
        }
    }

    Context 'Attribute Publishing' {
        It "Publishes entity after attribute create/update when -Publish is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support PublishXmlRequest
            # This test documents the expected behavior in a real environment:
            # - After creating/updating attribute, PublishXmlRequest is executed
            # - ParameterXml should contain the entity logical name (not just attribute)
            # - Publishing happens only when -Publish switch is present
            $true | Should -Be $true
        }
    }

    Context 'Relationship Publishing' {
        It "Publishes both entities after relationship create/update when -Publish is specified" -Skip {
            # Skipped: FakeXrmEasy doesn't support PublishXmlRequest
            # This test documents the expected behavior in a real environment:
            # - After creating/updating relationship, PublishXmlRequest is executed
            # - ParameterXml should contain both referenced and referencing entity names
            # - Publishing happens only when -Publish switch is present
            $true | Should -Be $true
        }
    }
}
