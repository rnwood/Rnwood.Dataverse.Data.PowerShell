. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAvailableLanguages Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAvailableLanguages SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAvailableLanguages executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAvailableLanguagesRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveAvailableLanguages"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAvailableLanguagesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveAvailableLanguages -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAvailableLanguages"
        }

    }
}
