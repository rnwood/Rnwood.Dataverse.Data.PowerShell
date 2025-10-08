. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExportMappingsImportMap Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExportMappingsImportMap SDK Cmdlet" {

        It "Invoke-DataverseExportMappingsImportMap executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportMappingsImportMapRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExportMappingsImportMap"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExportMappingsImportMapResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExportMappingsImportMap -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExportMappingsImportMap"
        }

    }
}
