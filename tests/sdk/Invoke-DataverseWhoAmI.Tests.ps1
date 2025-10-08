. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseWhoAmI Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "WhoAmI SDK Cmdlet" {
        It "Invoke-DataverseWhoAmI returns valid response" {
            # Call the SDK cmdlet
            $response = Invoke-DataverseWhoAmI -Connection $script:conn
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "WhoAmIResponse"
            $response.UserId | Should -Not -BeNullOrEmpty
            $response.BusinessUnitId | Should -Not -BeNullOrEmpty
            $response.OrganizationId | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "WhoAmIRequest"
            $proxy.LastResponse | Should -Be $response
        }
    }
}
