. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseProvisionLanguage Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ProvisionLanguage SDK Cmdlet" {

        It "Invoke-DataverseProvisionLanguage executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ProvisionLanguageRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ProvisionLanguage"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.ProvisionLanguageResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseProvisionLanguage -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ProvisionLanguage"
        }

    }
}
