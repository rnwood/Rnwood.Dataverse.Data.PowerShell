. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveProvisionedLanguages Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveProvisionedLanguages SDK Cmdlet" {
        It "Invoke-DataverseRetrieveProvisionedLanguages retrieves provisioned languages" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesRequest"
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesResponse
                $response.Results["RetrieveProvisionedLanguages"] = @(1033, 1036, 1031)
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveProvisionedLanguages -Connection $script:conn
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesResponse"
            $response.RetrieveProvisionedLanguages | Should -Not -BeNull
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesRequest"
        }
    }
}
