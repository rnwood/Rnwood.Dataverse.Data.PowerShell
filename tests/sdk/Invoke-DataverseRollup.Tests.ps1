. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRollup Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Rollup SDK Cmdlet" {

        It "Invoke-DataverseRollup executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RollupRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "Rollup"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RollupResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRollup -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "Rollup"
        }

    }
}
