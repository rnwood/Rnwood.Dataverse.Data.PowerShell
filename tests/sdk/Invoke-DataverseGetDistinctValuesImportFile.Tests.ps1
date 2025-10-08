. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetDistinctValuesImportFile Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetDistinctValuesImportFile SDK Cmdlet" {

        It "Invoke-DataverseGetDistinctValuesImportFile executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetDistinctValuesImportFile"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetDistinctValuesImportFile -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetDistinctValuesImportFile"
        }

    }
}
