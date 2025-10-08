. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeliverImmediatePromoteEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeliverImmediatePromoteEmail SDK Cmdlet" {

        It "Invoke-DataverseDeliverImmediatePromoteEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeliverImmediatePromoteEmailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeliverImmediatePromoteEmail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeliverImmediatePromoteEmailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeliverImmediatePromoteEmail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeliverImmediatePromoteEmail"
        }

    }
}
