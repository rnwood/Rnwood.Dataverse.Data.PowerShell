. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBulkOperationStatusClose Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "BulkOperationStatusClose SDK Cmdlet" {

        It "Invoke-DataverseBulkOperationStatusClose executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BulkOperationStatusCloseRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "BulkOperationStatusClose"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.BulkOperationStatusCloseResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseBulkOperationStatusClose -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "BulkOperationStatusClose"
        }

    }
}
