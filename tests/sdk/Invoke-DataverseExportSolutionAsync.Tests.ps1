. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExportSolutionAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExportSolutionAsync SDK Cmdlet" {

        It "Invoke-DataverseExportSolutionAsync executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportSolutionAsyncRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ExportSolutionAsync"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ExportSolutionAsyncResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseExportSolutionAsync -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExportSolutionAsync"
        }

    }
}
