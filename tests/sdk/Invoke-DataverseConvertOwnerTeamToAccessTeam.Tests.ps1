. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertOwnerTeamToAccessTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertOwnerTeamToAccessTeam SDK Cmdlet" {

        It "Invoke-DataverseConvertOwnerTeamToAccessTeam executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertOwnerTeamToAccessTeamRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ConvertOwnerTeamToAccessTeamRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.ConvertOwnerTeamToAccessTeamResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseConvertOwnerTeamToAccessTeam -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ConvertOwnerTeamToAccessTeamRequest"
        }

    }
}
