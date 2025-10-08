. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetReportHistoryLimit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetReportHistoryLimit SDK Cmdlet" {

        It "Invoke-DataverseGetReportHistoryLimit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetReportHistoryLimitRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetReportHistoryLimit"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetReportHistoryLimitResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetReportHistoryLimit -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetReportHistoryLimit"
        }

    }
}
