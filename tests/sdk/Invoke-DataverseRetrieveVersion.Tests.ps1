. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveVersion Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveVersion SDK Cmdlet" {
        It "Invoke-DataverseRetrieveVersion returns version information" {
            # Call the SDK cmdlet
            $response = Invoke-DataverseRetrieveVersion -Connection $script:conn
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveVersionResponse"
            $response.Version | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveVersionRequest"
        }
    }
}
