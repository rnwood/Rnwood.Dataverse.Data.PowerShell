. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSyncBulkOperation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SyncBulkOperation SDK Cmdlet" {

        It "Invoke-DataverseSyncBulkOperation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SyncBulkOperationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SyncBulkOperation"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SyncBulkOperationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSyncBulkOperation -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SyncBulkOperation"
        }

    }
}
