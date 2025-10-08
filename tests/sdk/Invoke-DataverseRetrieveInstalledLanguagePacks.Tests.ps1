. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveInstalledLanguagePacks Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveInstalledLanguagePacks SDK Cmdlet" {

        It "Invoke-DataverseRetrieveInstalledLanguagePacks executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePacksRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveInstalledLanguagePacks"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePacksResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveInstalledLanguagePacks -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveInstalledLanguagePacks"
        }

    }
}
