. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportFieldTranslation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportFieldTranslation SDK Cmdlet" {

        It "Invoke-DataverseImportFieldTranslation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportFieldTranslationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportFieldTranslation"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportFieldTranslationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportFieldTranslation -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportFieldTranslation"
        }

    }
}
