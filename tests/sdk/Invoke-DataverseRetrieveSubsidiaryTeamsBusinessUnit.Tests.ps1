. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveSubsidiaryTeamsBusinessUnit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveSubsidiaryTeamsBusinessUnit SDK Cmdlet" {

        It "Invoke-DataverseRetrieveSubsidiaryTeamsBusinessUnit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveSubsidiaryTeamsBusinessUnitRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveSubsidiaryTeamsBusinessUnit"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveSubsidiaryTeamsBusinessUnitResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveSubsidiaryTeamsBusinessUnit -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveSubsidiaryTeamsBusinessUnit"
        }

    }
}
