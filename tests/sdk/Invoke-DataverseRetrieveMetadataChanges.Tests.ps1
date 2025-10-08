. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMetadataChanges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMetadataChanges SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMetadataChanges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveMetadataChanges"
                
                # Create response
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveMetadataChanges -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMetadataChanges"
        }

    }
}
