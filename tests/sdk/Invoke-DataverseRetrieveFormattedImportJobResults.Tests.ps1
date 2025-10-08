. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveFormattedImportJobResults Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveFormattedImportJobResults SDK Cmdlet" {

        It "Invoke-DataverseRetrieveFormattedImportJobResults executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveFormattedImportJobResults"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveFormattedImportJobResults -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveFormattedImportJobResults"
        }

    }
}
