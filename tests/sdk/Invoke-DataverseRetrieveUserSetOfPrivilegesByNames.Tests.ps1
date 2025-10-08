. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserSetOfPrivilegesByNames Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserSetOfPrivilegesByNames SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserSetOfPrivilegesByNames executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByNamesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserSetOfPrivilegesByNames"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByNamesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserSetOfPrivilegesByNames -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserSetOfPrivilegesByNames"
        }

    }
}
