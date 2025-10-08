. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveByResourceResourceGroup Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveByResourceResourceGroup SDK Cmdlet" {

        It "Invoke-DataverseRetrieveByResourceResourceGroup executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveByResourceResourceGroupRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveByResourceResourceGroup"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveByResourceResourceGroupResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveByResourceResourceGroup -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveByResourceResourceGroup"
        }

    }
}
