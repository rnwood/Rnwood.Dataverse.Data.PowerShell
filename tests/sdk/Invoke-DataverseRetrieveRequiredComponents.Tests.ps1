. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveRequiredComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveRequiredComponents SDK Cmdlet" {

        It "Invoke-DataverseRetrieveRequiredComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveRequiredComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveRequiredComponents"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveRequiredComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveRequiredComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveRequiredComponents"
        }

    }
}
