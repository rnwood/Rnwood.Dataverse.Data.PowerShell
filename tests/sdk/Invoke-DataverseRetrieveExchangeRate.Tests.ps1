. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveExchangeRate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveExchangeRate SDK Cmdlet" {

        It "Invoke-DataverseRetrieveExchangeRate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveExchangeRateRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveExchangeRate"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveExchangeRateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveExchangeRate -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveExchangeRate"
        }

    }
}
