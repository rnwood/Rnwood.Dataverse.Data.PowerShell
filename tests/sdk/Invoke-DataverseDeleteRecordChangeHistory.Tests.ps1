. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteRecordChangeHistory Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteRecordChangeHistory SDK Cmdlet" {

        It "Invoke-DataverseDeleteRecordChangeHistory executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistoryRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteRecordChangeHistory"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistoryResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteRecordChangeHistory -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteRecordChangeHistory"
        }

    }
}
