. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteAuditData Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteAuditData SDK Cmdlet" {

        It "Invoke-DataverseDeleteAuditData executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteAuditDataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DeleteAuditData"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DeleteAuditDataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDeleteAuditData -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteAuditData"
        }

    }
}
