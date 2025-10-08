. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveProvisionedLanguages Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveProvisionedLanguagesRequest SDK Cmdlet" {

        It "Invoke-DataverseRetrieveProvisionedLanguages executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveProvisionedLanguagesRequest"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveProvisionedLanguages -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveProvisionedLanguagesRequest"
        }

    }
}
