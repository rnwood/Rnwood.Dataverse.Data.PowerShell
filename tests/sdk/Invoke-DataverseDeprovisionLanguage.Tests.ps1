. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeprovisionLanguage Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeprovisionLanguage SDK Cmdlet" {

        It "Invoke-DataverseDeprovisionLanguage executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeprovisionLanguageRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeprovisionLanguageRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeprovisionLanguageResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeprovisionLanguage -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeprovisionLanguageRequest"
        }

    }
}
