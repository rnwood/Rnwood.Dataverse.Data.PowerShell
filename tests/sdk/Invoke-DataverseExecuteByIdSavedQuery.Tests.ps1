. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteByIdSavedQuery Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteByIdSavedQuery SDK Cmdlet" {

        It "Invoke-DataverseExecuteByIdSavedQuery executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExecuteByIdSavedQueryRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExecuteByIdSavedQuery"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExecuteByIdSavedQueryResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExecuteByIdSavedQuery -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExecuteByIdSavedQuery"
        }

    }
}
