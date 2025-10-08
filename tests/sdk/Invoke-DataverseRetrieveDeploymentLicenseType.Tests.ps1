. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDeploymentLicenseType Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDeploymentLicenseType SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDeploymentLicenseType executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveDeploymentLicenseTypeRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveDeploymentLicenseType"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveDeploymentLicenseTypeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveDeploymentLicenseType -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDeploymentLicenseType"
        }

    }
}
