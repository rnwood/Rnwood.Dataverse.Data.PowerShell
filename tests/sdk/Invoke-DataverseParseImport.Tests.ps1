. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseParseImport Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ParseImport SDK Cmdlet" {

        It "Invoke-DataverseParseImport executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ParseImportRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ParseImport"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ParseImportResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseParseImport -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ParseImport"
        }

    }
}
