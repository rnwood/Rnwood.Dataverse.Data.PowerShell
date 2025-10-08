. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportTranslationAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportTranslationAsync SDK Cmdlet" {

        It "Invoke-DataverseImportTranslationAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportTranslationAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportTranslationAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportTranslationAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportTranslationAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportTranslationAsync"
        }

    }
}
