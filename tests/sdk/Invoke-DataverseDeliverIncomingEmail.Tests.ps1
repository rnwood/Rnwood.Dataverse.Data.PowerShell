. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeliverIncomingEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeliverIncomingEmail SDK Cmdlet" {

        It "Invoke-DataverseDeliverIncomingEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeliverIncomingEmailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeliverIncomingEmail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeliverIncomingEmailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeliverIncomingEmail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeliverIncomingEmail"
        }

    }
}
