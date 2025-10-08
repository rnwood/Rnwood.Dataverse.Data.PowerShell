. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePropagateByExpression Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PropagateByExpression SDK Cmdlet" {

        It "Invoke-DataversePropagateByExpression executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PropagateByExpressionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "PropagateByExpression"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.PropagateByExpressionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataversePropagateByExpression -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "PropagateByExpression"
        }

    }
}
