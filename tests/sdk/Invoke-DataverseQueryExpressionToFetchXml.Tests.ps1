. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQueryExpressionToFetchXml Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QueryExpressionToFetchXml SDK Cmdlet" {

        It "Invoke-DataverseQueryExpressionToFetchXml executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QueryExpressionToFetchXmlRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "QueryExpressionToFetchXml"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.QueryExpressionToFetchXmlResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseQueryExpressionToFetchXml -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "QueryExpressionToFetchXml"
        }

    }
}
