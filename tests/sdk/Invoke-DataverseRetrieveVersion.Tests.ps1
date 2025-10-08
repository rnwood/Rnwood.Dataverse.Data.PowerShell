. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveVersion Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveVersionRequest SDK Cmdlet" {

        It "Invoke-DataverseRetrieveVersion executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveVersionRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveVersionRequest"
                
                # Create response
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveVersionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveVersion -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveVersionRequest"
        }

    }
}
