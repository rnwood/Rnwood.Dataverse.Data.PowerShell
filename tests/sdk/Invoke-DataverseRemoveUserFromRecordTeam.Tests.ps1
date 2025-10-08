. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveUserFromRecordTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveUserFromRecordTeam SDK Cmdlet" {

        It "Invoke-DataverseRemoveUserFromRecordTeam executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemoveUserFromRecordTeam"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemoveUserFromRecordTeam -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemoveUserFromRecordTeam"
        }

    }
}
