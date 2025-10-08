. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseProcessInboundEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ProcessInboundEmail SDK Cmdlet" {

        It "Invoke-DataverseProcessInboundEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ProcessInboundEmailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ProcessInboundEmail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ProcessInboundEmailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseProcessInboundEmail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ProcessInboundEmail"
        }

    }
}
