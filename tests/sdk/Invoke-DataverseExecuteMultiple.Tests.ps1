. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteMultiple SDK Cmdlet" {

        It "Invoke-DataverseExecuteMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExecuteMultiple"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.ExecuteMultipleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExecuteMultiple -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExecuteMultiple"
        }

    }
}
