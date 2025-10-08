. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserPrivileges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserPrivileges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserPrivileges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveUserPrivileges"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveUserPrivileges -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserPrivileges"
        }

    }
}
