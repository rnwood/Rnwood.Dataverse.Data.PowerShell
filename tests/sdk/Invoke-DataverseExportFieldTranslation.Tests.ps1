. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExportFieldTranslation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExportFieldTranslation SDK Cmdlet" {

        It "Invoke-DataverseExportFieldTranslation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportFieldTranslationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExportFieldTranslation"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExportFieldTranslationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExportFieldTranslation -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExportFieldTranslation"
        }

    }
}
