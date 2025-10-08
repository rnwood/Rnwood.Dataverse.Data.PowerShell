. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAppComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAppComponents SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAppComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAppComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAppComponents"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAppComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAppComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAppComponents"
        }

    }
}
