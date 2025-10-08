. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSendFax Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SendFax SDK Cmdlet" {

        It "Invoke-DataverseSendFax executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SendFaxRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SendFax"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SendFaxResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSendFax -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SendFax"
        }

    }
}
