. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveParsedDataImportFile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveParsedDataImportFile SDK Cmdlet" {

        It "Invoke-DataverseRetrieveParsedDataImportFile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveParsedDataImportFile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveParsedDataImportFile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveParsedDataImportFile"
        }

    }
}
