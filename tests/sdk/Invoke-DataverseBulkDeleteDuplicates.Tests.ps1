. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBulkDeleteDuplicates Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "BulkDeleteDuplicates SDK Cmdlet" {

        It "Invoke-DataverseBulkDeleteDuplicates executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BulkDeleteDuplicatesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "BulkDeleteDuplicates"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.BulkDeleteDuplicatesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseBulkDeleteDuplicates -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "BulkDeleteDuplicates"
        }

    }
}
