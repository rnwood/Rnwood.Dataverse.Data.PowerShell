. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportMappingsImportMap Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportMappingsImportMap SDK Cmdlet" {

        It "Invoke-DataverseImportMappingsImportMap executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportMappingsImportMapRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportMappingsImportMap"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportMappingsImportMapResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportMappingsImportMap -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportMappingsImportMap"
        }

    }
}
