. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseResetUserFilters Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ResetUserFilters SDK Cmdlet" {

        It "Invoke-DataverseResetUserFilters executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ResetUserFiltersRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ResetUserFilters"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ResetUserFiltersResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseResetUserFilters -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ResetUserFilters"
        }

    }
}
