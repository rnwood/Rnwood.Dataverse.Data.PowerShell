. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAllChildUsersSystemUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAllChildUsersSystemUser SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAllChildUsersSystemUser executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAllChildUsersSystemUserRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveAllChildUsersSystemUser"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAllChildUsersSystemUserResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveAllChildUsersSystemUser -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAllChildUsersSystemUser"
        }

    }
}
