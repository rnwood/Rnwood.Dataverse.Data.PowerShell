. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveProcessInstances Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveProcessInstances SDK Cmdlet" {

        It "Invoke-DataverseRetrieveProcessInstances executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveProcessInstancesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveProcessInstances"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveProcessInstancesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveProcessInstances -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveProcessInstances"
        }

    }
}
