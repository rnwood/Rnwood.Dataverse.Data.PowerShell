. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseValidateFetchXmlExpression Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ValidateFetchXmlExpression SDK Cmdlet" {

        It "Invoke-DataverseValidateFetchXmlExpression executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ValidateFetchXmlExpressionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ValidateFetchXmlExpression"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ValidateFetchXmlExpressionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseValidateFetchXmlExpression -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ValidateFetchXmlExpression"
        }

    }
}
