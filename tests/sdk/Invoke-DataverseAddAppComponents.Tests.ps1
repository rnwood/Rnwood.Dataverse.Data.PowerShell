. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddAppComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddAppComponents SDK Cmdlet" {

        It "Invoke-DataverseAddAppComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddAppComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "AddAppComponents"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.AddAppComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseAddAppComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "AddAppComponents"
        }

    }
}
