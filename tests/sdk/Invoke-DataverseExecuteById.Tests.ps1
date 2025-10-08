. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteById Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteById SDK Cmdlet" {

        It "Invoke-DataverseExecuteById executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.ExecuteByIdRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExecuteById"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.ExecuteByIdResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExecuteById -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExecuteById"
        }

    }
}
