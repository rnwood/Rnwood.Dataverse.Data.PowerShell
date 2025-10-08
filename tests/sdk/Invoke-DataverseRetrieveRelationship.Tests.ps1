. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveRelationship Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveRelationship SDK Cmdlet" {
        It "Invoke-DataverseRetrieveRelationship retrieves relationship metadata" {
            $relationshipId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipRequest"
                $request.MetadataId | Should -BeOfType [System.Guid]
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipResponse
                $relationshipMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata
                $relationshipMetadata.GetType().GetProperty("MetadataId").SetValue($relationshipMetadata, $relationshipId)
                $response.Results["RelationshipMetadata"] = $relationshipMetadata
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveRelationship -Connection $script:conn -MetadataId $relationshipId
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipResponse"
            $response.RelationshipMetadata | Should -Not -BeNull
            $response.RelationshipMetadata.MetadataId | Should -Be $relationshipId
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.MetadataId | Should -Be $relationshipId
        }
    }
}
