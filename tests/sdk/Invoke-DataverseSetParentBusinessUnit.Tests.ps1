. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetParentBusinessUnit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetParentBusinessUnit SDK Cmdlet" {

        It "Invoke-DataverseSetParentBusinessUnit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetParentBusinessUnit"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetParentBusinessUnit -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetParentBusinessUnit"
        }

    }
}
