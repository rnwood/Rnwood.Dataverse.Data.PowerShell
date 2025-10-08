. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAuditDetails Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAuditDetails SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAuditDetails executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAuditDetailsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAuditDetails"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAuditDetailsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAuditDetails -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAuditDetails"
        }

    }
}
