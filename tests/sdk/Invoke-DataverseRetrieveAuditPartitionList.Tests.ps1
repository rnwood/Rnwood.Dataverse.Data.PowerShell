. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAuditPartitionList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAuditPartitionList SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAuditPartitionList executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAuditPartitionList"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAuditPartitionList -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAuditPartitionList"
        }

    }
}
