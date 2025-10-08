. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteOpenInstances Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteOpenInstances SDK Cmdlet" {

        It "Invoke-DataverseDeleteOpenInstances executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteOpenInstancesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteOpenInstances"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteOpenInstancesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteOpenInstances -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteOpenInstances"
        }

    }
}
