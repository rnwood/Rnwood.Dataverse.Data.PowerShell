. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteTransaction Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteTransaction SDK Cmdlet" {

        It "Invoke-DataverseExecuteTransaction executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.ExecuteTransactionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExecuteTransaction"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.ExecuteTransactionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExecuteTransaction -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExecuteTransaction"
        }

    }
}
