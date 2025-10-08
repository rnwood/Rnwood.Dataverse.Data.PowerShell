. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseConvertDateAndTimeBehavior Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ConvertDateAndTimeBehavior SDK Cmdlet" {

        It "Invoke-DataverseConvertDateAndTimeBehavior executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ConvertDateAndTimeBehaviorRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ConvertDateAndTimeBehaviorRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.ConvertDateAndTimeBehaviorResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseConvertDateAndTimeBehavior -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ConvertDateAndTimeBehaviorRequest"
        }

    }
}
