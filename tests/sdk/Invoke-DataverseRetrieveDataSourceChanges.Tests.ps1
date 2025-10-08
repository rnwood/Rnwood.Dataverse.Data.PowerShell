. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDataSourceChanges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDataSourceChanges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDataSourceChanges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveDataSourceChangesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDataSourceChanges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveDataSourceChangesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDataSourceChanges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDataSourceChanges"
        }

    }
}
