. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAllOptionSets Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAllOptionSets SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAllOptionSets executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAllOptionSetsRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveAllOptionSets"
                
                # Create response
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveAllOptionSetsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveAllOptionSets -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAllOptionSets"
        }

    }
}
