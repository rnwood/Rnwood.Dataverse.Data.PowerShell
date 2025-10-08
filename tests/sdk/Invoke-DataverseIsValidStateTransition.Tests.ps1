. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseIsValidStateTransition Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "IsValidStateTransition SDK Cmdlet" {

        It "Invoke-DataverseIsValidStateTransition executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.IsValidStateTransitionRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "IsValidStateTransition"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.IsValidStateTransitionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseIsValidStateTransition -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "IsValidStateTransition"
        }

    }
}
