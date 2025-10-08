. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveAppComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveAppComponents SDK Cmdlet" {

        It "Invoke-DataverseRemoveAppComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveAppComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemoveAppComponents"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemoveAppComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemoveAppComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemoveAppComponents"
        }

    }
}
