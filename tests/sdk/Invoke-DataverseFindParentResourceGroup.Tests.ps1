. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseFindParentResourceGroup Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "FindParentResourceGroup SDK Cmdlet" {

        It "Invoke-DataverseFindParentResourceGroup executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.FindParentResourceGroupRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "FindParentResourceGroup"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.FindParentResourceGroupResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseFindParentResourceGroup -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "FindParentResourceGroup"
        }

    }
}
