. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQueryMultipleSchedules Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QueryMultipleSchedules SDK Cmdlet" {

        It "Invoke-DataverseQueryMultipleSchedules executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QueryMultipleSchedulesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "QueryMultipleSchedules"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.QueryMultipleSchedulesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseQueryMultipleSchedules -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "QueryMultipleSchedules"
        }

    }
}
