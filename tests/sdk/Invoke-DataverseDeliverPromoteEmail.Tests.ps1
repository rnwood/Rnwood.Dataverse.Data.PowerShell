. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeliverPromoteEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeliverPromoteEmail SDK Cmdlet" {

        It "Invoke-DataverseDeliverPromoteEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeliverPromoteEmail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeliverPromoteEmail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeliverPromoteEmail"
        }

    }
}
