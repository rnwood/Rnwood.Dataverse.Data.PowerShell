. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteByIdUserQuery Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteByIdUserQuery SDK Cmdlet" {

        It "Invoke-DataverseExecuteByIdUserQuery executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExecuteByIdUserQueryRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExecuteByIdUserQuery"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExecuteByIdUserQueryResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExecuteByIdUserQuery -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExecuteByIdUserQuery"
        }

    }
}
