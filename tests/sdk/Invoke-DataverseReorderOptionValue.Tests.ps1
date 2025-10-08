. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReorderOptionValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReorderOptionValue SDK Cmdlet" {

        It "Invoke-DataverseReorderOptionValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReorderOptionValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReorderOptionValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReorderOptionValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReorderOptionValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReorderOptionValue"
        }

    }
}
