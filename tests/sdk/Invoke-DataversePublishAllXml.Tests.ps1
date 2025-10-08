. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishAllXml Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishAllXml SDK Cmdlet" {
        It "Invoke-DataversePublishAllXml publishes all customizations" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishAllXmlRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.PublishAllXmlResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataversePublishAllXml -Connection $script:conn } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "PublishAllXmlRequest"
        }
    }
}
