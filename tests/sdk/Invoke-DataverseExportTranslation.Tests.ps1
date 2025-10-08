. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExportTranslation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExportTranslation SDK Cmdlet" {

        It "Invoke-DataverseExportTranslation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportTranslationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExportTranslation"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExportTranslationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExportTranslation -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExportTranslation"
        }

    }
}
