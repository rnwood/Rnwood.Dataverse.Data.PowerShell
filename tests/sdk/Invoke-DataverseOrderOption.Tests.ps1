. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseOrderOption Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "OrderOption SDK Cmdlet" {

        It "Invoke-DataverseOrderOption executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.OrderOptionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "OrderOption"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.OrderOptionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseOrderOption -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "OrderOption"
        }

    }
}
