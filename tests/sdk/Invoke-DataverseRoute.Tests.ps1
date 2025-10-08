. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRoute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Route SDK Cmdlet" {

        It "Invoke-DataverseRoute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RouteRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "Route"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RouteResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRoute -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "Route"
        }

    }
}
