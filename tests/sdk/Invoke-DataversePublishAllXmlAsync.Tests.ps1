. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishAllXmlAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishAllXmlAsync SDK Cmdlet" {

        It "Invoke-DataversePublishAllXmlAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishAllXmlAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PublishAllXmlAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PublishAllXmlAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePublishAllXmlAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PublishAllXmlAsync"
        }

    }
}
