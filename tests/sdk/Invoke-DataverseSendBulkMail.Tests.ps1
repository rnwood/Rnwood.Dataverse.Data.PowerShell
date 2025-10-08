. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSendBulkMail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SendBulkMail SDK Cmdlet" {

        It "Invoke-DataverseSendBulkMail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SendBulkMailRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SendBulkMail"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SendBulkMailResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSendBulkMail -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SendBulkMail"
        }

    }
}
