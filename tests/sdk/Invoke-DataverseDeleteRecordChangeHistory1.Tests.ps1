. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteRecordChangeHistory1 Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteRecordChangeHistory1 SDK Cmdlet" {

        It "Invoke-DataverseDeleteRecordChangeHistory1 executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistory1Request", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteRecordChangeHistory1"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistory1Response" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteRecordChangeHistory1 -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteRecordChangeHistory1"
        }

    }
}
