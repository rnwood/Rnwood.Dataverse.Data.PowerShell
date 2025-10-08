. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePublishXml Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PublishXml SDK Cmdlet" {

        It "Invoke-DataversePublishXml executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishXmlRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PublishXml"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PublishXmlResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePublishXml -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PublishXml"
        }

    }
}
