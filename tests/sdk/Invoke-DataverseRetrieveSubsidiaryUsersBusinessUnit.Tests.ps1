. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveSubsidiaryUsersBusinessUnit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveSubsidiaryUsersBusinessUnit SDK Cmdlet" {

        It "Invoke-DataverseRetrieveSubsidiaryUsersBusinessUnit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveSubsidiaryUsersBusinessUnitRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveSubsidiaryUsersBusinessUnit"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveSubsidiaryUsersBusinessUnitResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveSubsidiaryUsersBusinessUnit -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveSubsidiaryUsersBusinessUnit"
        }

    }
}
