. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAadUserSetOfPrivilegesByNames SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByNamesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAadUserSetOfPrivilegesByNames"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByNamesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAadUserSetOfPrivilegesByNames"
        }

    }
}
