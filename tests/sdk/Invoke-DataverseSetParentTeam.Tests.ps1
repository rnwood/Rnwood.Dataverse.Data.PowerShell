. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetParentTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetParentTeam SDK Cmdlet" {

        It "Invoke-DataverseSetParentTeam executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetParentTeamRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SetParentTeam"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SetParentTeamResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSetParentTeam -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetParentTeam"
        }

    }
}
