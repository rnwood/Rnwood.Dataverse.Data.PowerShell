. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAllEntities Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAllEntities SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAllEntities returns entity metadata" {
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest", {
                param($request)
                
                # Validate request parameters were properly converted
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest"
                $request.EntityFilters | Should -Not -BeNull
                $request.RetrieveAsIfPublished | Should -BeOfType [System.Boolean]
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesResponse
                
                # Create minimal entity metadata array
                $entityMetadataList = New-Object 'System.Collections.Generic.List[Microsoft.Xrm.Sdk.Metadata.EntityMetadata]'
                
                # Add contact entity metadata
                $contactMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                $contactMetadata.GetType().GetProperty("LogicalName").SetValue($contactMetadata, "contact")
                $contactMetadata.GetType().GetProperty("SchemaName").SetValue($contactMetadata, "Contact")
                $entityMetadataList.Add($contactMetadata)
                
                # Add account entity metadata
                $accountMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                $accountMetadata.GetType().GetProperty("LogicalName").SetValue($accountMetadata, "account")
                $accountMetadata.GetType().GetProperty("SchemaName").SetValue($accountMetadata, "Account")
                $entityMetadataList.Add($accountMetadata)
                
                $response.Results["EntityMetadata"] = $entityMetadataList.ToArray()
                return $response
            })
            
            # Call the SDK cmdlet
            $response = Invoke-DataverseRetrieveAllEntities -Connection $script:conn -EntityFilters Entity -RetrieveAsIfPublished $true
            
            # Verify response type as documented
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesResponse"
            $response.GetType().Name | Should -Be "RetrieveAllEntitiesResponse"
            
            # Verify response contains expected data
            $response.EntityMetadata | Should -Not -BeNull
            $response.EntityMetadata | Should -BeOfType [Microsoft.Xrm.Sdk.Metadata.EntityMetadata[]]
            $response.EntityMetadata.Count | Should -Be 2
            $response.EntityMetadata[0].LogicalName | Should -Be "contact"
            $response.EntityMetadata[1].LogicalName | Should -Be "account"
            
            # Verify the proxy captured the request with correct parameters
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest"
            $proxy.LastRequest.EntityFilters.ToString() | Should -Be "Entity"
            $proxy.LastRequest.RetrieveAsIfPublished | Should -Be $true
        }
    }

    }
}
