. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveSubGroupsResourceGroup Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveSubGroupsResourceGroup SDK Cmdlet" {

        It "Invoke-DataverseRetrieveSubGroupsResourceGroup executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveSubGroupsResourceGroupRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveSubGroupsResourceGroup"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveSubGroupsResourceGroupResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveSubGroupsResourceGroup -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveSubGroupsResourceGroup"
        }

    }
}
