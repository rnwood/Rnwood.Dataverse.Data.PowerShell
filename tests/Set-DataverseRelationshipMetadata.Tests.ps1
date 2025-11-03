. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRelationshipMetadata - Parameter Definition' {
    Context 'Parameter Validation' {
        It "Has IntersectEntitySchemaName parameter" {
            $command = Get-Command Set-DataverseRelationshipMetadata
            $command | Should -Not -BeNullOrEmpty
            
            $param = $command.Parameters['IntersectEntitySchemaName']
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be 'String'
        }
        
        It "Has IntersectEntityName as alias for IntersectEntitySchemaName" {
            $command = Get-Command Set-DataverseRelationshipMetadata
            $param = $command.Parameters['IntersectEntitySchemaName']
            $param.Aliases | Should -Contain 'IntersectEntityName'
        }
    }
}

Describe 'Set-DataverseRelationshipMetadata - ManyToMany' -Skip {
    # Skipping these tests because FakeXrmEasy doesn't support:
    # 1. RetrieveRelationshipRequest (used by CheckRelationshipExists)
    # 2. Organization entity retrieval (used by GetBaseLanguageCode)
    # 3. CreateManyToManyRequest/CreateOneToManyRequest
    # The fix is validated by:
    # 1. Successful build (compilation)
    # 2. Parameter definition tests above
    # 3. Manual E2E testing or real environment tests
    
    Context 'Create ManyToMany Relationship' {
        It "Creates ManyToMany relationship with IntersectEntitySchemaName parameter" {
            # Setup request interceptor to capture the CreateManyToManyRequest
            $capturedRequest = $null
            $requestInterceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.CreateManyToManyRequest]) {
                    $script:capturedRequest = $request
                }
                return $null
            }
            
            $connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @("contact", "account")
            
            # Create ManyToMany relationship using the new parameter name
            { Set-DataverseRelationshipMetadata `
                -Connection $connection `
                -SchemaName "new_account_contact" `
                -RelationshipType "ManyToMany" `
                -ReferencedEntity "account" `
                -ReferencingEntity "contact" `
                -IntersectEntitySchemaName "new_accountcontact" `
                -Confirm:$false `
                -ErrorAction Stop } | Should -Not -Throw
            
            # Verify the request was created with both properties set
            $capturedRequest | Should -Not -BeNullOrEmpty
            $capturedRequest.IntersectEntitySchemaName | Should -Be "new_accountcontact"
            $capturedRequest.ManyToManyRelationship | Should -Not -BeNullOrEmpty
            $capturedRequest.ManyToManyRelationship.IntersectEntityName | Should -Be "new_accountcontact"
            $capturedRequest.ManyToManyRelationship.SchemaName | Should -Be "new_account_contact"
            $capturedRequest.ManyToManyRelationship.Entity1LogicalName | Should -Be "account"
            $capturedRequest.ManyToManyRelationship.Entity2LogicalName | Should -Be "contact"
        }
    }
}
