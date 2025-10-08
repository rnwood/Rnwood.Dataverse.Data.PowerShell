. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInstantiateFilters Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InstantiateFilters SDK Cmdlet" {

        It "Invoke-DataverseInstantiateFilters executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InstantiateFiltersRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InstantiateFilters"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InstantiateFiltersResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInstantiateFilters -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InstantiateFilters"
        }

    }
}
