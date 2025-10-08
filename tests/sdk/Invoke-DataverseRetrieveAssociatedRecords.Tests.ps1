. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAssociatedRecords Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAssociatedRecords SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAssociatedRecords executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAssociatedRecordsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAssociatedRecords"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAssociatedRecordsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAssociatedRecords -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAssociatedRecords"
        }

    }
}
