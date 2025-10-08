. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetHeaderColumnsImportFile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetHeaderColumnsImportFile SDK Cmdlet" {

        It "Invoke-DataverseGetHeaderColumnsImportFile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetHeaderColumnsImportFileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetHeaderColumnsImportFile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetHeaderColumnsImportFileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetHeaderColumnsImportFile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetHeaderColumnsImportFile"
        }

    }
}
