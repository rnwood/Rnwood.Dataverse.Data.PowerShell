. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePrivilegeSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePrivilegeSet SDK Cmdlet" {

        It "Invoke-DataverseRetrievePrivilegeSet executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePrivilegeSetRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrievePrivilegeSet"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrievePrivilegeSetResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrievePrivilegeSet -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrievePrivilegeSet"
        }

    }
}
