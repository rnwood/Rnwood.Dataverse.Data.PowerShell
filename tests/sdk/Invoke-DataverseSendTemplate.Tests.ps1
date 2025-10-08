. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSendTemplate Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SendTemplate SDK Cmdlet" {

        It "Invoke-DataverseSendTemplate executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SendTemplateRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SendTemplate"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SendTemplateResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSendTemplate -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SendTemplate"
        }

    }
}
