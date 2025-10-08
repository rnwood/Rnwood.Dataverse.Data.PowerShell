. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBulkDetectDuplicates Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "BulkDetectDuplicates SDK Cmdlet" {

        It "Invoke-DataverseBulkDetectDuplicates executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BulkDetectDuplicatesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "BulkDetectDuplicates"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.BulkDetectDuplicatesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseBulkDetectDuplicates -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "BulkDetectDuplicates"
        }

    }
}
