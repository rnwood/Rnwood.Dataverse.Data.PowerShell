. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseStatusUpdateBulkOperation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "StatusUpdateBulkOperation SDK Cmdlet" {

        It "Invoke-DataverseStatusUpdateBulkOperation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.StatusUpdateBulkOperationRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "StatusUpdateBulkOperation"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.StatusUpdateBulkOperationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseStatusUpdateBulkOperation -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "StatusUpdateBulkOperation"
        }

    }
}
