. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveEntity SDK Cmdlet" {
        It "Invoke-DataverseRetrieveEntity retrieves contact entity metadata" {
            # Call the SDK cmdlet with contact entity
            $response = Invoke-DataverseRetrieveEntity -Connection $script:conn -LogicalName "contact" -EntityFilters Entity
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveEntityResponse"
            $response.EntityMetadata | Should -Not -BeNull
            $response.EntityMetadata.LogicalName | Should -Be "contact"
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveEntityRequest"
            $proxy.LastRequest.LogicalName | Should -Be "contact"
        }
    }
}
