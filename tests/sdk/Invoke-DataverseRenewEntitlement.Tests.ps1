. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRenewEntitlement Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RenewEntitlement SDK Cmdlet" {

        It "Invoke-DataverseRenewEntitlement executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RenewEntitlementRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RenewEntitlement"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RenewEntitlementResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRenewEntitlement -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RenewEntitlement"
        }

    }
}
