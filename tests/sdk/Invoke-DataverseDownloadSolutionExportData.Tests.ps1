. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDownloadSolutionExportData Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DownloadSolutionExportData SDK Cmdlet" {

        It "Invoke-DataverseDownloadSolutionExportData executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "DownloadSolutionExportData"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseDownloadSolutionExportData -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DownloadSolutionExportData"
        }

    }
}
