. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserQueues Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserQueues SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserQueues executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserQueues"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserQueues -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserQueues"
        }

    }
}
