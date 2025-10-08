. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveParentGroupsResourceGroup Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveParentGroupsResourceGroup SDK Cmdlet" {

        It "Invoke-DataverseRetrieveParentGroupsResourceGroup executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveParentGroupsResourceGroupRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveParentGroupsResourceGroup"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveParentGroupsResourceGroupResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveParentGroupsResourceGroup -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveParentGroupsResourceGroup"
        }

    }
}
