. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseImportRecordsImport Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ImportRecordsImport SDK Cmdlet" {

        It "Invoke-DataverseImportRecordsImport executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportRecordsImportRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ImportRecordsImport"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ImportRecordsImportResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseImportRecordsImport -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ImportRecordsImport"
        }

    }
}
