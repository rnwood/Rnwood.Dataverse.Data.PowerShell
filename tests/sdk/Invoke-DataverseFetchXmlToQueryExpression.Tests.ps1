. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseFetchXmlToQueryExpression Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "FetchXmlToQueryExpression SDK Cmdlet" {

        It "Invoke-DataverseFetchXmlToQueryExpression executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "FetchXmlToQueryExpression"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseFetchXmlToQueryExpression -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "FetchXmlToQueryExpression"
        }

    }
}
